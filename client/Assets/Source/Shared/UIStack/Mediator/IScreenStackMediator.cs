using Source.Shared.Components.Screens;

namespace Source.Shared.UIStack.Mediator
{
    public interface IScreenStackMediator
    {
        void Push(IScreenView view);
        void PopScreen();
        void PopAllScreens();
    }
}