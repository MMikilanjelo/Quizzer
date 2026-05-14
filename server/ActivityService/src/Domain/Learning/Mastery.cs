namespace Domain.Learning;

public record Mastery
{
    public required double Value
    {
        get;
        init => field = Math.Clamp(value, 0.0, 1.0);
    }

    public bool IsMastered => Value >= 0.85;

    public static Mastery Initial =>
        new()
        {
            Value = 0.1
        };

    public Mastery CalculateNext(bool isCorrect, BktParams bktParams)
    {
        double numerator = isCorrect
            ? Value * (1 - bktParams.PSlip)
            : Value * bktParams.PSlip;

        double denominator = isCorrect
            ? numerator + (1 - Value) * bktParams.PGuess
            : numerator + (1 - Value) * (1 - bktParams.PGuess);

        double pLObs = numerator / denominator;

        double nextValue = pLObs + (1 - pLObs) * bktParams.PTransition;

        return new Mastery { Value = nextValue };
    }
}