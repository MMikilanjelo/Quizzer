namespace Domain.Quizzes;

public sealed record QuizQuestion
{
    public required string Id { get; init; }
    public required string TopicId { get; init; }
    public required string Text { get; init; }
    public required IReadOnlyCollection<string> Options { get; init; }
    public required int CorrectIndex { get; init; }
    public int? SelectedIndex { get; init; }
    public bool IsAnswered => SelectedIndex.HasValue;
    public bool IsCorrect => IsAnswered && SelectedIndex == CorrectIndex;
}