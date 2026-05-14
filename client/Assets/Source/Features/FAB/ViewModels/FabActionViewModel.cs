using Source.Shared.Components;
using Source.Shared.Reactive.Commands;
using UnityEngine;

namespace Source.Features.FAB.ViewModels
{
    public class FabActionViewModel
    {
        public string Title { get; }
        public SpriteAtlasIconModel Icon { get; }
        public ICommand ExecuteCommand { get; }

        public FabActionViewModel(
            string title,
            SpriteAtlasIconModel icon,
            ICommand executeCommand
        )
        {
            Title = title;
            Icon = icon;
            ExecuteCommand = executeCommand;
        }
    }
}