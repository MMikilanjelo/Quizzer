using Application.Abstractions.Messaging;
using Application.Quizzes.Views;
using Domain.Quizzes;
using ErrorOr;
using Marten;

namespace Application.Quizzes.Queries;

public static class GetQuizAnalytics
{
    public sealed record Query : IQuery<Response>
    {
        public required string QuizId { get; init; }
    }

    public sealed record Response(Model Analytics);

    public sealed record Model
    {
        public required string Id { get; init; }
        public required string UserId { get; init; }
        public required string Name { get; init; }
        public required List<string> Topics { get; init; }
        public required QuizStatus Status { get; init; }
        public DateTime CreatedAt { get; init; }
        public int TotalQuestions { get; init; }
        public int AnsweredCount { get; init; }
        public int CorrectCount { get; init; }
        public int IncorrectCount { get; init; }
        public required List<MasteryDeltaModel> MasteryChanges { get; init; }
    }

    public sealed record MasteryDeltaModel(
        string ConceptId,
        double StartingMastery,
        double EndingMastery,
        int Attempts
    );

    internal sealed class Handler(IQuerySession session) : IQueryHandler<Query, Response>
    {
        public async Task<ErrorOr<Response>> Handle(Query query, CancellationToken cancellationToken)
        {
            var quizView = await session.LoadAsync<QuizSummaryView>(query.QuizId, cancellationToken);

            if (quizView is null)
            {
                return QuizErrors.NotFound;
            }

            var correctCount = quizView.Questions.Count(q =>
                q.SelectedAnswerIndex.HasValue &&
                q.SelectedAnswerIndex == q.CorrectAnswerIndex);

            var incorrectCount = quizView.AnsweredCount - correctCount;

            var model = new Model
            {
                Id = quizView.Id,
                UserId = quizView.UserId,
                Name = quizView.Name,
                Topics = quizView.Topics,
                Status = quizView.Status,
                CreatedAt = quizView.CreatedAt,

                TotalQuestions = quizView.QuestionCount,
                AnsweredCount = quizView.AnsweredCount,
                CorrectCount = correctCount,
                IncorrectCount = incorrectCount,

                MasteryChanges = quizView.MasteryChanges?.Select(m => new MasteryDeltaModel(
                    m.ConceptId,
                    m.StartingMastery,
                    m.EndingMastery,
                    m.AttemptsDuringQuiz
                )).ToList() ?? []
            };

            return new Response(model);
        }
    }
}