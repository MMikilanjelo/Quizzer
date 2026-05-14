using Source.Shared.Components;

namespace Source.Features.TechnicalDialogs.DecisionDialog
{
    public record DecisionDialogProps
    {
        public LocalizedStringProps Title { get; private set; }
        public LocalizedStringProps Message { get; private set; }
        public LocalizedStringProps PrimaryActionText { get; private set; }
        public LocalizedStringProps SecondaryActionText { get; private set; }
        public SpriteAtlasIconModel IconModel { get; private set; }

        public static readonly DecisionDialogProps NetworkError = new()
        {
            Title = new LocalizedStringProps("Error.Network.Title"),
            Message = new LocalizedStringProps("Error.Network.Message"),
            PrimaryActionText = new LocalizedStringProps("Common.Retry"),
            SecondaryActionText = new LocalizedStringProps("Common.Cancel"),
            IconModel = Shared.Icons.Icons.WifiX
        };
    }
}