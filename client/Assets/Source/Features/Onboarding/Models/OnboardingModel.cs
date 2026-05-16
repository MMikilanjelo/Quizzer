using System;
using System.Collections.Generic;

namespace Source.Features.Onboarding.Models
{
    public record OnboardingModel
    {
        public int TotalSteps => 3;
        public int CurrentIndex { get; private set; }
        public int CurrentStep => CurrentIndex + 1;
        public IReadOnlyList<YourInterestModel> InterestModels { get; set; } = Array.Empty<YourInterestModel>();
        public IReadOnlyList<YourGoalModel> GoalModels { get; set; } = Array.Empty<YourGoalModel>();
        public IReadOnlyList<ProficiencyModel> ProficiencyModels { get; set; } = Array.Empty<ProficiencyModel>();
        public IReadOnlyList<YourGoalModel> SelectedGoals { get; set; } = Array.Empty<YourGoalModel>();
        public IReadOnlyList<YourInterestModel> SelectedInterests { get; set; } = Array.Empty<YourInterestModel>();

        public void AdvanceStep()
        {
            if (CurrentIndex < TotalSteps - 1)
            {
                CurrentIndex++;
            }
        }

        public void RevertStep()
        {
            if (CurrentIndex > 0)
            {
                CurrentIndex--;
            }
        }
    }

    public record ProficiencyModel(string Id, string Name)
    {
        public string Id { get; } = Id;
        public string Name { get; } = Name;
    }

    public record YourGoalModel(string Id, string Name)
    {
        public string Id { get; } = Id;
        public string Name { get; } = Name;
    }
    
    public record YourInterestModel(string Id, string Name)
    {
        public string Id { get; } = Id;
        public string Name { get; } = Name;
    }
}