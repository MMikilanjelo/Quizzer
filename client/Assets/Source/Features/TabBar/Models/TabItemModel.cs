using Source.Shared.Components;
using UnityEngine;

namespace Source.Features.TabBar.Models
{
    public record TabItemModel(string Id, SpriteAtlasIconModel Icon)
    {
        public string Id { get; } = Id;
        public SpriteAtlasIconModel Icon { get; } = Icon;
    }
}