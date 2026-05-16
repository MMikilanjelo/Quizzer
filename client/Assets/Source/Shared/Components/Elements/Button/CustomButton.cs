using System;
using Source.Shared.Components.Elements.CustomVisualElement;
using Source.Shared.Components.Elements.Label;
using Source.Shared.Components.Manipulators;
using Source.Shared.Reactive.Commands;
using Source.Shared.Reactive.Disposables;
using UnityEngine;
using UnityEngine.UIElements;

namespace Source.Shared.Components.Elements.Button
{
    [UxmlElement]
    public partial class CustomButton : ReactiveVisualElement
    {
        public enum ButtonVariant
        {
            Primary,
            Secondary,
            Outline,
            Ghost
        }

        public enum ButtonIconPosition
        {
            Leading,
            Trailing
        }

        public enum ButtonSize
        {
            Large,
            Small
        }

        public enum ButtonShape
        {
            Pill,
            Circle
        }

        private readonly CustomLabel _label;
        private readonly VisualElement _iconElement;
        private readonly VisualElement _spinner;
        private readonly IVisualElementScheduledItem _spinnerTask;
        private CommandManipulator _currentManipulator;

        [UxmlAttribute]
        public string Text
        {
            get => _label.text;
            set
            {
                _label.text = value;
                _label.style.display = string.IsNullOrEmpty(value) ? DisplayStyle.None : DisplayStyle.Flex;
                RefreshIconPlacement();
            }
        }

        private ButtonIconPosition _iconPosition = ButtonIconPosition.Leading;

        [UxmlAttribute]
        public ButtonIconPosition IconPosition
        {
            get => _iconPosition;
            set
            {
                _iconPosition = value;
                RefreshIconPlacement();
            }
        }

        public Sprite Icon
        {
            set
            {
                if (value)
                {
                    _iconElement.style.backgroundImage = new StyleBackground(value);
                    _iconElement.style.display = DisplayStyle.Flex;
                }
                else
                {
                    _iconElement.style.backgroundImage = null;
                    _iconElement.style.display = DisplayStyle.None;
                }

                RefreshIconPlacement();
            }
        }

        private ButtonVariant _variant = ButtonVariant.Primary;

        [UxmlAttribute]
        public ButtonVariant Variant
        {
            get => _variant;
            set
            {
                _variant = value;
                RefreshVariantClasses();
            }
        }

        private ButtonSize _size = ButtonSize.Large;

        [UxmlAttribute]
        public ButtonSize Size
        {
            get => _size;
            set
            {
                _size = value;
                RefreshVariantClasses();
            }
        }

        private ButtonShape _shape = ButtonShape.Pill;

        [UxmlAttribute]
        public ButtonShape Shape
        {
            get => _shape;
            set
            {
                _shape = value;
                RefreshVariantClasses();
            }
        }

        public CustomButton()
        {
            AddToClassList("btn");

            _iconElement = new VisualElement { pickingMode = PickingMode.Ignore };
            _iconElement.AddToClassList("btn__icon");
            _iconElement.style.display = DisplayStyle.None;
            Add(_iconElement);

            _label = new CustomLabel { pickingMode = PickingMode.Ignore };
            Add(_label);

            _spinner = new VisualElement { pickingMode = PickingMode.Ignore };
            _spinner.AddToClassList("btn__spinner");
            Add(_spinner);

            _spinnerTask = _spinner.schedule
                .Execute(state => { _spinner.style.rotate = new Rotate(new Angle(state.now * 0.3f % 360f, AngleUnit.Degree)); })
                .Every(16);
            _spinnerTask.Pause();

            RefreshVariantClasses();
            RefreshIconPlacement();

            RegisterCallback<AttachToPanelEvent>(OnAttachedToPanel);

            Disposables.Add(new Disposable(() =>
            {
                UnregisterCallback<CommandStateChangedEvent>(OnStateChanged);
                _spinnerTask?.Pause();
                Unbind();
            }));
        }

        private void RefreshIconPlacement()
        {
            bool hasText = !string.IsNullOrEmpty(Text);

            if (!hasText)
            {
                _iconElement.RemoveFromClassList("btn__icon--leading");
                _iconElement.RemoveFromClassList("btn__icon--trailing");
                _iconElement.AddToClassList("btn__icon--icon-only");
                return;
            }

            _iconElement.RemoveFromClassList("btn__icon--icon-only");

            if (_iconPosition == ButtonIconPosition.Leading)
            {
                _iconElement.PlaceBehind(_label);
                _iconElement.RemoveFromClassList("btn__icon--trailing");
                _iconElement.AddToClassList("btn__icon--leading");
            }
            else
            {
                _iconElement.PlaceInFront(_label);
                _iconElement.RemoveFromClassList("btn__icon--leading");
                _iconElement.AddToClassList("btn__icon--trailing");
            }
        }

        private void OnAttachedToPanel(AttachToPanelEvent evt) =>
            RegisterCallback<CommandStateChangedEvent>(OnStateChanged);


        public IDisposable Bind(ICommand command)
        {
            Unbind();

            _currentManipulator = new CommandManipulator(command);

            this.AddManipulator(_currentManipulator);

            var disposable = new Disposable(Unbind);

            Disposables.Add(disposable);

            return disposable;
        }

        public void Unbind()
        {
            if (_currentManipulator == null) return;

            this.RemoveManipulator(_currentManipulator);

            _currentManipulator = null;
        }

        private void OnStateChanged(CommandStateChangedEvent evt)
        {
            RemoveFromClassList("btn--loading");
            RemoveFromClassList("btn--disabled");

            _spinnerTask.Pause();
            _spinner.style.rotate = new Rotate(0);

            switch (evt.State)
            {
                case CommandState.Executing:
                    AddToClassList("btn--loading");
                    _spinnerTask.Resume();
                    break;
                case CommandState.Disabled:
                    AddToClassList("btn--disabled");
                    break;
            }
        }

        private void RefreshVariantClasses()
        {
            foreach (ButtonVariant v in Enum.GetValues(typeof(ButtonVariant)))
                RemoveFromClassList($"btn--{v.ToString().ToLower()}");

            foreach (ButtonSize s in Enum.GetValues(typeof(ButtonSize)))
                RemoveFromClassList($"btn--size-{s.ToString().ToLower()}");

            foreach (ButtonShape sh in Enum.GetValues(typeof(ButtonShape)))
                RemoveFromClassList($"btn--shape-{sh.ToString().ToLower()}");

            AddToClassList($"btn--{Variant.ToString().ToLower()}");
            AddToClassList($"btn--size-{Size.ToString().ToLower()}");
            AddToClassList($"btn--shape-{Shape.ToString().ToLower()}");
        }
    }
}