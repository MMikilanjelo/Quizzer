using Source.Shared.Components;
using Source.Shared.Reactive.Events;
using UnityEngine;

namespace Source.Features.Quizzes.MyQuizzes.ViewModels
{
    public class NoQuizzesViewModel
    {
        public IReadOnlyReactiveProperty<SpriteAtlasIconModel> Icon => _icon;
        public IReadOnlyReactiveProperty<string> Title => _title;
        public IReadOnlyReactiveProperty<string> Message => _message;

        private readonly ReactiveProperty<SpriteAtlasIconModel> _icon;
        private readonly ReactiveProperty<string> _title;
        private readonly ReactiveProperty<string> _message;

        public NoQuizzesViewModel(
            SpriteAtlasIconModel icon,
            string title,
            string message
        )
        {
            _icon = new ReactiveProperty<SpriteAtlasIconModel>(icon);
            _title = new ReactiveProperty<string>(title);
            _message = new ReactiveProperty<string>(message);
        }

        public void Update(SpriteAtlasIconModel icon, string title, string message)
        {
            _icon.Value = icon;
            _title.Value = title;
            _message.Value = message;
        }

        public void Dispose()
        {
            _icon.Dispose();
            _title.Dispose();
            _message.Dispose();
        }
    }
}