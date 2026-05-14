namespace Source.Shared
{
    public enum ErrorKind
    {
        None = 0,
        Connectivity = 1,
        Remote = 2,
        Local = 3,
        Cancelled = 4
    }

    public record Error
    {
        public string Code { get; private set; }
        public ErrorKind Kind { get; private set; }
        public bool IsHandled { get; private set; }

        public static readonly Error None = new("None", ErrorKind.None, false);

        public static readonly Error Cancelled = new(ErrorCodes.OperationCancelled, ErrorKind.Cancelled, false);
        public Error ToHandled() => this with { IsHandled = true };
        public static Error Local(string code) => new(code, ErrorKind.Local, false);
        public static Error Connectivity(string code) => new(code, ErrorKind.Connectivity, false);
        public static Error Remote(string code) => new(code, ErrorKind.Remote, false);

        private Error(string code, ErrorKind kind, bool isHandled)
        {
            Code = code;
            Kind = kind;
            IsHandled = isHandled;
        }
    }
}