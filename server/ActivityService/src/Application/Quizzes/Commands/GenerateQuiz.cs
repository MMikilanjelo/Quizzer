using System.Collections.Immutable;
using Application.Abstractions;
using Application.Abstractions.Messaging;
using Application.Abstractions.Providers;
using Domain.Quizzes;
using Domain.Users;
using ErrorOr;
using Marten;
using Microsoft.Extensions.Logging;

namespace Application.Quizzes.Commands;

public static class GenerateQuiz
{
    public sealed record Command(string QuizId) : ICommand;

    private sealed record GenerateContentResponse(List<QuizQuestion> Questions, Quiz.DifficultyLevel SystemDifficulty);

    internal sealed class Handler(
        IDocumentSession documentSession,
        IKnowledgeGraphClient graphClient,
        IStructuredContentGenerator geminiClient,
        IDateTimeProvider dateTimeProvider,
        ILogger<Handler> logger
    ) : ICommandHandler<Command>
    {
        public async Task<ErrorOr<Success>> HandleAsync(Command command, CancellationToken cancellationToken)
        {
            var stream = await documentSession.Events.FetchForWriting<Quiz>(command.QuizId, cancellationToken);

            var quiz = stream.Aggregate;

            if (quiz is null)
            {
                return QuizErrors.NotFound;
            }

            if (quiz.Status != Quiz.QuizStatus.Pending)
            {
                return QuizErrors.NotPending;
            }

            var user = await documentSession.LoadAsync<User>(quiz.UserId, cancellationToken);

            if (user is null)
            {
                return UserErrors.NotFound;
            }

            var targetNodes = await graphClient.GetDiscoveryNodesAsync(quiz.Topic, quiz.UserId, limit: 5, cancellationToken);
            var masteryContext = string.Join("\n", targetNodes.Select(n => $"- {n.Name} (ID: {n.Id}): Mastery Level {n.Mastery:P0}"));
            var nodeIds = targetNodes.Select(n => n.Id).ToList();

            var graphContext = await graphClient.GetGraphContextAsync(nodeIds, cancellationToken);

            var goalsContext = string.Join(", ", user.Goals);
            var interestsContext = string.Join(", ", user.Interests);

            string questionCountRule = quiz.DesiredQuestionsCount.HasValue
                ? $"Generate exactly {quiz.DesiredQuestionsCount.Value} questions using the following rules:"
                : "Determine the optimal number of questions to generate (between 5 and 15) to adequately test the student's mastery of the provided concepts, using the following rules:";

            string difficultyRules = quiz.Schedule == Quiz.ScheduleType.Manual && quiz.UserDifficulty != Quiz.DifficultyLevel.Unspecified
                ? $@"- This is a MANUAL session. Generate ALL questions strictly at the '{quiz.UserDifficulty}' difficulty level. 
                        - Do NOT adapt the difficulty based on mastery.
                        - You MUST set the `SystemDifficulty` in your response to exactly '{quiz.UserDifficulty}'."
                : @"- Determine an appropriate overall difficulty (Easy, Medium, or Hard) based on the student's mastery.
                       - For Concepts with Mastery < 30% (Novice): Write fundamental, definitional, or 'What is' questions. Use simple, clear language.
                       - For Concepts with Mastery 30% - 60% (Intermediate): Write 'How' and 'Why' questions. Test their understanding of mechanisms or common use cases.
                       - For Concepts with Mastery > 60% (Advanced): Write complex, scenario-based, or troubleshooting questions. Force the student to apply the concept to a realistic problem.
                       - You MUST set the `SystemDifficulty` in your response to the overall level you chose.";

            string prompt = $@"
            You are an elite educational architect and subject matter expert designing an adaptive, highly engaging quiz about '{quiz.Topic}'. 

            Your goal is to test the student's true understanding of the concepts and how they relate to one another in the real world.

            ### 1. STUDENT GLOBAL PROFILE
            - Baseline Proficiency: {user.Proficiency}
            - Core Learning Goals: {goalsContext}
            - Topic Interests: {interestsContext}

            ### 2. CONCEPT-SPECIFIC MASTERY STATE
            The student's current proficiency in these targeted sub-concepts is:
            {masteryContext}

            ### 3. CURRICULUM TOPOLOGY (How concepts relate)
            {graphContext}

            ### 4. GENERATION RULES & ADAPTIVE DIFFICULTY
            {questionCountRule}
            {difficultyRules}

            ### 5. PERSONALIZATION & CONTEXT MATCHING
            Tailor the framing, flavor scenarios, and technical vocabulary of the questions using the Student Global Profile. 
            - If their goal is 'CareerBoost', focus scenario questions on production codebase issues, architecture trade-offs, or industry performance constraints.
            - If their interest includes 'Programming', express technical context using concrete implementations or functional examples rather than abstract theory.
            - Keep the baseline linguistic tone aligned with their overall '{user.Proficiency}' level.

            ### 6. TESTING RELATIONSHIPS (The 'Secret Sauce')
            Use the Curriculum Topology to write questions that test the boundaries between concepts. 
            - If A is a 'Hierarchy/Prerequisite' to B: Ask a question about why A must be understood before implementing B, or how A forms the foundation of B.
            - If A 'Contributes to' or 'Impacts' B: Ask a scenario question about technical tradeoffs. (e.g., 'If we optimize A, what is the expected impact on B?')
            - If A is 'Equivalent' to B: Test the student's ability to recognize both terms interchangeably in a practical context.

            ### 7. STRICT NEGATIVE CONSTRAINTS (CRITICAL)
            - NEVER use phrases like 'According to the context', 'Based on the graph', or 'As shown in the topology'. The questions must read naturally.
            - DO NOT break character. Act strictly as the exam interface.

            ### 8. OUTPUT FORMAT
            You must return a JSON response matching the requested schema.
            - `SystemDifficulty`: The overall difficulty level you applied (Easy, Medium, or Hard).
            - `Questions`: The array of questions. For each generated question, you MUST return the exact `ConceptId` (Node ID) from the Mastery State that the question is primarily testing.";

            logger.LogInformation(prompt);

            var geminiResult = await geminiClient.GenerateAsync<GenerateContentResponse>(prompt, cancellationToken);

            if (geminiResult.IsError)
            {
                return geminiResult.Errors;
            }

            var domainResult = quiz.Fill(
                new FillQuizCommand
                {
                    Questions = geminiResult.Value.Questions.ToImmutableList(),
                    SystemDifficulty = geminiResult.Value.SystemDifficulty,
                    GeneratedAt = dateTimeProvider.UtcNow
                }
            );

            if (domainResult.IsError)
            {
                return domainResult.Errors;
            }

            stream.AppendOne(domainResult.Value);

            await documentSession.SaveChangesAsync(cancellationToken);

            return Result.Success;
        }
    }
}