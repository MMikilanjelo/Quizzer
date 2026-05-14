using UnityEngine.UIElements;

namespace Source.Shared.Components.Screens
{
    public interface IScreenWithHeaderView
    {
        VisualElement Header { get; }
    }
}