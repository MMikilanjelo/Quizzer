using Application.Abstractions.Messaging;
using Application.Quizzes.Views;
using Domain.Quizzes;
using ErrorOr;
using Marten;

namespace Application.Quizzes.Queries;

public static class GetQuiz
{
    public sealed record Query : IQuery<Response>
    {
        public required string QuizId { get; init; }
    }

    public sealed record Response(Model Quiz);

    public sealed record Model
    {
        public required string Id { get; init; }
        public required string UserId { get; init; }
        public required string Name { get; init; }
        public required List<string> Topics { get; init; }
        public required QuizSummaryView.QuizStatus Status { get; init; }
        public required List<QuestionModel> Questions { get; init; }
        public required List<string> AnsweredQuestionIds { get; init; }
        public string? CurrentQuestionId => Questions.Select(q => q.Id).FirstOrDefault(id => !AnsweredQuestionIds.Contains(id));
        public int TotalQuestions => Questions.Count;
        public int AnsweredCount => AnsweredQuestionIds.Count;
    }

    public sealed record QuestionModel(
        string Id,
        string ConceptId,
        string Text,
        List<string> Options
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

            var model = new Model
            {
                Id = quizView.Id,
                UserId = quizView.UserId,
                Topics = quizView.Topics,
                Status = quizView.Status,
                Name = quizView.Name,

                Questions = quizView.Questions.Select(q => new QuestionModel(
                    q.Id,
                    q.TopicId,
                    q.Text,
                    [.. q.Options]
                )).ToList(),

                AnsweredQuestionIds = quizView.Questions
                    .Where(q => q.SelectedAnswerIndex.HasValue)
                    .Select(q => q.Id)
                    .ToList()
            };

            return new Response(model);
        }
    }
}