using System.Collections.Generic;
using Source.Shared.Components;
using Source.Shared.Components.Dialogs;
using Source.Shared.Components.Elements.Backdrop;
using Source.Shared.Components.Overlays;
using Source.Shared.Components.Screens;
using UnityEngine.UIElements;

namespace Source.Shared.UIStack.Components
{
    public class UIStack : VisualElement
    {
        public VisualElement ScreensLayer { get; }
        public VisualElement HeaderLayer { get; }
        public VisualElement FooterLayer { get; }
        public VisualElement ModalsLayer { get; }
        public VisualElement OverlaysLayer { get; }
        public Backdrop BackdropOverlay { get; }

        public UIStack(UIDocument uiDocument)
        {
            var root = uiDocument.rootVisualElement;

            ScreensLayer = root.Q<VisualElement>("Screens");
            HeaderLayer = root.Q<VisualElement>("Header");
            FooterLayer = root.Q<VisualElement>("Footer");
            ModalsLayer = root.Q<VisualElement>("Modals");
            OverlaysLayer = root.Q<VisualElement>("Overlays");
            BackdropOverlay = root.Q<Backdrop>("Backdrop");
        }
    }
}