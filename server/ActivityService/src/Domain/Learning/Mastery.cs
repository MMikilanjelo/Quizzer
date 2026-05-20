namespace Domain.Learning;

public record Mastery
{
    public required double Value
    {
        get;
        init => field = Math.Clamp(value, 0.0, 1.0);
    }

    public bool IsMastered => Value >= 0.85;

    public static Mastery Initial => new() { Value = 0.1 };

    public Mastery CalculateNext(bool isCorrect, BktParams topicParams, QuestionDynamics questionDynamics)
    {
        double pGuess = questionDynamics.PGuess;
        double pSlip = questionDynamics.PSlip;

        double numerator = isCorrect
            ? Value * (1 - pSlip)
            : Value * pSlip;

        double denominator = isCorrect
            ? numerator + (1 - Value) * pGuess
            : numerator + (1 - Value) * (1 - pGuess);

        double pLObs = numerator / denominator;

        double nextValue = pLObs + (1 - pLObs) * topicParams.PTransition;

        return new Mastery { Value = nextValue };
    }
}