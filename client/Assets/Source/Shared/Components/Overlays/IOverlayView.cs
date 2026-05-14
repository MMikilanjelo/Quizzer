namespace Source.Shared.Components.Overlays
{
    public interface IOverlayView : IView
    {
        void Initialize();
        void Dispose();
    }
}