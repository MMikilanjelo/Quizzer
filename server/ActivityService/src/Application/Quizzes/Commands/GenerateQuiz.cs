using System.Collections.Immutable;
using Application.Abstractions;
using Application.Abstractions.Messaging;
using Application.Abstractions.Providers;
using Domain.Quizzes;
using ErrorOr;
using Marten;
using Microsoft.Extensions.Logging;

namespace Application.Quizzes.Commands;

public static class GenerateQuiz
{
    public sealed record Command(string QuizId) : ICommand;

    private sealed record GenerateContentResponse(List<QuizQuestion> Questions);

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

            var targetNodes = await graphClient.GetDiscoveryNodesAsync(
                quiz.Topic, quiz.UserId, limit: 5, cancellationToken);

            var masteryContext = string.Join("\n", targetNodes.Select(n =>
                $"- {n.Name} (ID: {n.Id}): Mastery Level {n.Mastery:P0}"));

            var nodeIds = targetNodes.Select(n => n.Id).ToList();

            string graphContext = await graphClient.GetGraphContextAsync(nodeIds, cancellationToken);

            string prompt = $@"
            You are an elite educational architect and subject matter expert designing an adaptive, highly engaging quiz about '{quiz.Topic}'. 

            Your goal is to test the student's true understanding of the concepts and how they relate to one another in the real world.

            ### 1. STUDENT MASTERY STATE
            The student's current proficiency in these specific concepts is:
            {masteryContext}

            ### 2. CURRICULUM TOPOLOGY (How concepts relate)
            {graphContext}

            ### 3. GENERATION RULES & ADAPTIVE DIFFICULTY
            Generate exactly 10 questions using the following difficulty distribution based on the student's Mastery State:
            - For Concepts with Mastery < 30% (Novice): Write fundamental, definitional, or 'What is' questions. Use simple, clear language.
            - For Concepts with Mastery 30% - 60% (Intermediate): Write 'How' and 'Why' questions. Test their understanding of mechanisms or common use cases.
            - For Concepts with Mastery > 60% (Advanced): Write complex, scenario-based, or troubleshooting questions. Force the student to apply the concept to a realistic problem.

            ### 4. TESTING RELATIONSHIPS (The 'Secret Sauce')
            Use the Curriculum Topology to write questions that test the boundaries between concepts. 
            - If A is a 'Hierarchy/Prerequisite' to B: Ask a question about why A must be understood before implementing B, or how A forms the foundation of B.
            - If A 'Contributes to' or 'Impacts' B: Ask a scenario question about technical tradeoffs. (e.g., 'If we optimize A, what is the expected impact on B?')
            - If A is 'Equivalent' to B: Test the student's ability to recognize both terms interchangeably in a practical context.

            ### 5. STRICT NEGATIVE CONSTRAINTS (CRITICAL)
            - NEVER use phrases like 'According to the context', 'Based on the graph', or 'As shown in the topology'. The questions must read naturally.
            - DO NOT reveal that you are adapting the difficulty. Never write 'Since your mastery is low...'.
            - DO NOT break character. Act strictly as the exam interface.

            ### 6. OUTPUT FORMAT
            For each generated question, you MUST return the exact `ConceptId` (Node ID) from the Mastery State that the question is primarily testing.";

            logger.LogInformation(prompt);

            ErrorOr<GenerateContentResponse> geminiResult = await geminiClient.GenerateAsync<GenerateContentResponse>(prompt, cancellationToken);

            if (geminiResult.IsError)
            {
                return geminiResult.Errors;
            }

            var domainResult = quiz.Fill(
                new FillQuizCommand
                {
                    Questions = geminiResult.Value.Questions.ToImmutableList(),
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