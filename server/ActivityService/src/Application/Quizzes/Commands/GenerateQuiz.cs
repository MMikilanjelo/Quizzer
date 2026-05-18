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

    private sealed record GeneratedQuestion(
        string ConceptId,
        string Text,
        List<string> Options,
        int CorrectIndex
    );

    private sealed record GenerateContentResponse(
        List<GeneratedQuestion> Questions,
        Quiz.DifficultyLevel Difficulty
    );

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

            var targetNodes = await graphClient.GetTopicNodesAsync(quiz.DomainId, quiz.UserId, limit: 5, cancellationToken);
            var masteryContext = string.Join("\n", targetNodes.Select(n => $"- {n.Name} (ID: {n.Id}): Mastery Level {n.Mastery:P0}"));
            var nodeIds = targetNodes.Select(n => n.Id).ToList();

            var graphContext = await graphClient.GetGraphContextAsync(nodeIds, cancellationToken);

            var goalsContext = string.Join(", ", user.Goals);
            var interestsContext = string.Join(", ", user.Interests);

            string questionCountRule = quiz.DesiredQuestionsCount.HasValue
                ? $"Generate exactly {quiz.DesiredQuestionsCount.Value} questions using the following rules:"
                : "Determine the optimal number of questions to generate (between 5 and 15) to adequately test the student's mastery of the provided concepts, using the following rules:";

            string difficultyRules = quiz.Difficulty != Quiz.DifficultyLevel.Unspecified
                ? $"""
                   - Generate ALL questions strictly at the '{quiz.Difficulty}' difficulty level. 
                   - Do NOT adapt the difficulty based on mastery.
                   - You MUST set the `Difficulty` in your response to exactly '{quiz.Difficulty}'.
                   """
                : """
                  - Determine an appropriate overall difficulty (Easy, Medium, or Hard) based on the student's mastery.
                  - For Concepts with Mastery < 30% (Novice): Write fundamental, definitional, or 'What is' questions. Use simple, clear language.
                  - For Concepts with Mastery 30% - 60% (Intermediate): Write 'How' and 'Why' questions. Test their understanding of mechanisms or common use cases.
                  - For Concepts with Mastery > 60% (Advanced): Write complex, scenario-based, or troubleshooting questions. Force the student to apply the concept to a realistic problem.
                  - You MUST set the `Difficulty` in your response to the overall level you chose.
                  """;

            string prompt = $"""
                             You are an expert educational architect designing an adaptive, highly engaging quiz for the domain: '{quiz.DomainId}'.

                             Your objective is to generate questions that test the student's practical understanding of concepts and how they interrelate in real-world scenarios.

                             === INPUT DATA ===

                             <student_profile>
                             Baseline Proficiency: {user.Proficiency}
                             Core Learning Goals: {goalsContext}
                             Topic Interests: {interestsContext}
                             </student_profile>

                             <mastery_state>
                             {masteryContext}
                             </mastery_state>

                             <curriculum_topology>
                             {graphContext}
                             </curriculum_topology>

                             === GENERATION RULES ===

                             <adaptive_constraints>
                             {questionCountRule}
                             {difficultyRules}
                             </adaptive_constraints>

                             <personalization_rules>
                             - Tailor the framing, flavor scenarios, and technical vocabulary strictly to the <student_profile>.
                             - If a goal includes 'CareerBoost', focus scenario questions on production codebase issues, architecture trade-offs, or industry performance constraints.
                             - If interests include 'Programming', express technical context using concrete implementations or functional examples rather than abstract theory.
                             - Align the baseline linguistic tone strictly with the '{user.Proficiency}' level.
                             </personalization_rules>

                             <relationship_testing>
                             Use the <curriculum_topology> to write questions that test the boundaries between concepts:
                             - Hierarchy/Prerequisite (A -> B): Ask why A must be understood before implementing B, or how A forms the foundation of B.
                             - Contributes/Impacts (A -> B): Ask a scenario question about technical tradeoffs (e.g., "If we optimize A, what is the expected impact on B?").
                             - Equivalent (A = B): Test the student's ability to recognize both terms interchangeably in a practical context.
                             </relationship_testing>

                             === STRICT CONSTRAINTS (CRITICAL) ===
                             - NEVER use meta-phrases like "According to the context", "Based on the graph", or "As shown in the topology". The questions must read naturally as if written by a human professor.
                             - DO NOT break character. You are the exam engine. Provide NO conversational filler.
                             - Output ONLY valid JSON. Do not include markdown formatting, code blocks, or any text outside of the JSON object.

                             === OUTPUT FORMAT ===
                             You must return a JSON response matching the requested schema exactly.
                             - `Difficulty`: The overall difficulty level you applied (Easy, Medium, or Hard).
                             - `Questions`: The array of generated questions. For each question, you MUST return the exact `ConceptId` (Node ID) from the <mastery_state> that the question is primarily testing.
                             """;

            logger.LogInformation(prompt);

            var geminiResult = await geminiClient.GenerateAsync<GenerateContentResponse>(prompt, cancellationToken);

            if (geminiResult.IsError)
            {
                return geminiResult.Errors;
            }

            var domainQuestions = geminiResult.Value.Questions.Select(generatedQuestion => new QuizQuestion
            {
                Id = Guid.NewGuid().ToString(),
                TopicId = generatedQuestion.ConceptId,
                Text = generatedQuestion.Text,
                Options = generatedQuestion.Options,
                CorrectIndex = generatedQuestion.CorrectIndex
            }).ToList();

            var domainResult = quiz.Fill(
                new FillQuizCommand
                {
                    Questions = domainQuestions.ToImmutableList(),
                    Difficulty = geminiResult.Value.Difficulty,
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