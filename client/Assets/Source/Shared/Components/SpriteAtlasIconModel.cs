namespace Source.Shared.Components
{
    public record SpriteAtlasIconModel(string Name, string Key)
    {
        public string Name { get; private set; } = Name;
        public string Key { get; private set; } = Key;
    }
}