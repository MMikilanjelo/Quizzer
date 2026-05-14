using System;
using Source.Shared.Components.Elements.Label;
using Source.Shared.Components.Manipulators;
using Source.Shared.Reactive.Commands;
using Source.Shared.Reactive.Disposables;
using UnityEngine;
using UnityEngine.UIElements;

namespace Source.Features.FAB.Components
{
    [UxmlElement]
    public partial class FabMenuButton : VisualElement
    {
        private readonly CustomLabel _label;
        private readonly VisualElement _iconElement;

        private Sprite _icon;
        private CommandManipulator _currentManipulator;

        [UxmlAttribute]
        public string Text
        {
            get => _label.text;
            set => _label.text = value;
        }

        [UxmlAttribute]
        public Sprite Icon
        {
            get => _icon;
            set
            {
                if (_icon == value)
                {
                    return;
                }

                _icon = value;
                _iconElement.style.backgroundImage = new StyleBackground(_icon);
            }
        }

        public IDisposable Bind(ICommand command)
        {
            Unbind();

            _currentManipulator = new CommandManipulator(command);
            this.AddManipulator(_currentManipulator);

            return new Disposable(Unbind);
        }

        public void Unbind()
        {
            if (_currentManipulator == null)
            {
                return;
            }

            this.RemoveManipulator(_currentManipulator);
            _currentManipulator = null;
        }

        public FabMenuButton()
        {
            AddToClassList("fab-menu-button");

            var labelContainer = new VisualElement();
            labelContainer.AddToClassList("fab-menu-button__label-container");

            _label = new CustomLabel
            {
                pickingMode = PickingMode.Ignore,
                Weight = CustomLabel.FontWeight.Bold,
                Variant = CustomLabel.TextVariant.Large
            };
            _label.AddToClassList("fab-menu-button__label-text");
            labelContainer.Add(_label);

            var buttonCircle = new VisualElement();
            buttonCircle.AddToClassList("fab-menu-button__circle");

            _iconElement = new VisualElement
            {
                pickingMode = PickingMode.Ignore
            };
            _iconElement.AddToClassList("fab-menu-button__icon");
            buttonCircle.Add(_iconElement);

            Add(labelContainer);
            Add(buttonCircle);
        }
    }
}