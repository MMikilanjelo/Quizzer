using Source.Features.MyProfile.ViewModels;
using Source.Shared.Components.Elements.Label;
using Source.Shared.Components.Skeleton;
using UnityEngine.UIElements;

namespace Source.Features.MyProfile.Components
{
    public class MasterySkillRowSkeleton : VisualElement
    {
        public MasterySkillRowSkeleton()
        {
            AddToClassList("my-profile__skill-row");
            var header = new VisualElement();
            header.AddToClassList("my-profile__skill-header");

            var nameSkeleton = new Skeleton { Variant = Skeleton.SkeletonVariant.Small, style = { width = Length.Percent(40) } };
            var pctSkeleton = new Skeleton { Variant = Skeleton.SkeletonVariant.Small, style = { width = 40 } };
            header.Add(nameSkeleton);
            header.Add(pctSkeleton);

            var trackSkeleton = new Skeleton { Variant = Skeleton.SkeletonVariant.Block };
            trackSkeleton.AddToClassList("my-profile__skill-track");

            Add(header);
            Add(trackSkeleton);
        }
    }

    public class MasterySkillRow : VisualElement
    {
        public MasterySkillRow(ConceptMasteryLevelViewModel concept, bool isFocusArea)
        {
            AddToClassList("my-profile__skill-row");

            var header = new VisualElement();
            header.AddToClassList("my-profile__skill-header");

            var nameLabel = new CustomLabel
            {
                text = concept.ConceptId,
                Variant = CustomLabel.TextVariant.Regular, 
                Weight = CustomLabel.FontWeight.Bold
            };
            nameLabel.AddToClassList("my-profile__skill-name");

            var pctLabel = new CustomLabel
            {
                text = $"{concept.MasteryPercentage}%",
                Variant = CustomLabel.TextVariant.Regular,
                Weight = CustomLabel.FontWeight.Bold
            };
            pctLabel.AddToClassList("my-profile__skill-pct");

            header.Add(nameLabel);
            header.Add(pctLabel);

            var track = new VisualElement();
            track.AddToClassList("my-profile__skill-track");

            var fill = new VisualElement();
            fill.AddToClassList("my-profile__skill-fill");
            if (isFocusArea) fill.AddToClassList("my-profile__skill-fill--warning");
            fill.style.width = Length.Percent(concept.MasteryPercentage);

            track.Add(fill);
            Add(header);
            Add(track);
        }
    }
}