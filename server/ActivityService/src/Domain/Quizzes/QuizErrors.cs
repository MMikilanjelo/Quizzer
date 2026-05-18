namespace Domain.Quizzes;

using ErrorOr;

public static class QuizErrors
{
    public static readonly Error Forbidden = Error.Forbidden(
        code: "Quiz.Forbidden",
        description: "You do not have permission to modify this quiz."
    );

    public static readonly Error NotFound = Error.NotFound(
        code: "Quiz.NotFound",
        description: "The requested quiz does not exist."
    );

    public static readonly Error NotPending = Error.Conflict(
        code: "Quiz.NotPending",
        description: "Cannot generate content for a quiz that is not pending."
    );

    public static readonly Error EmptyQuestions = Error.Validation(
        code: "Quiz.EmptyQuestions",
        description: "A quiz must have at least one question."
    );

    public static readonly Error NotActive = Error.Validation(
        code: "Quiz.NotActive",
        description: "The quiz is not in an active state to perform this action."
    );

    public static readonly Error AlreadyAnswered = Error.Conflict(
        code: "Quiz.AlreadyAnswered",
        description: "This question has already been answered."
    );

    public static readonly Error QuestionNotFound = Error.NotFound(
        code: "Quiz.QuestionNotFound",
        description: "The specified question was not found in this quiz."
    );

    public static readonly Error Incomplete = Error.Validation(
        code: "Quiz.Incomplete",
        description: "Please answer all questions before completing the quiz."
    );

    public static readonly Error GenerationFailed = Error.Failure(
        code: "Quiz.GenerationFailed",
        description: "Failed to generate quiz content."
    );
}