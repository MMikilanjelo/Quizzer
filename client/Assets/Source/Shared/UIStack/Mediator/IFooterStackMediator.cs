using Source.Shared.Components;

namespace Source.Shared.UIStack.Mediator
{
    public interface IFooterStackMediator
    {
        void AddToFooter(IView view);
        void RemoveFromFooter(IView view);
    }
}