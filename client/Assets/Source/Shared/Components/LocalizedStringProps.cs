namespace Source.Shared.Components
{
    public record LocalizedStringProps(string Key, params object[] Args)
    {
        public string Key { get; set; } = Key;

        public object[] Args { get; set; } = Args;

        public static implicit operator LocalizedStringProps(string key) =>
            new(key);

        public override string ToString()
        {
            return string.Format(Key, Args);
        }
    }
}