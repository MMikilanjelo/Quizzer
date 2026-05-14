using Source.Shared.Components;
using Source.Shared.Components.Overlays;

namespace Source.Shared.UIStack.Mediator
{
    public interface IOverlayStackMediator
    {
        void AddToOverlay(IView view);
        void Push(IOverlayView view);
        void PopOverlay();
        void PopAllOverlays();
        void ShowBackdrop();
        void HideBackdrop();
    }
}