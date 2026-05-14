namespace Source.Shared.Components.Screens
{
    public interface IScreenView : IView
    {
        void Initialize();
        void Dispose();
    }
}