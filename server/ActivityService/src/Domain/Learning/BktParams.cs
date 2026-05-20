namespace Domain.Learning;

public record BktParams
{
    public required double PTransition
    {
        get;
        init => field = Math.Clamp(value, 0.0, 1.0);
    }

    public static BktParams Initial => new()
    {
        PTransition = 0.05
    };
}

public record QuestionDynamics
{
    public required double PGuess
    {
        get;
        init => field = Math.Clamp(value, 0.0, 1.0);
    }

    public required double PSlip
    {
        get;
        init => field = Math.Clamp(value, 0.0, 1.0);
    }
}