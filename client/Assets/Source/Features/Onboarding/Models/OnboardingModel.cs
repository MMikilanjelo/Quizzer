using System;
using System.Collections.Generic;
using Source.Features.Onboarding.TellUsYourGoal.Models;
using Source.Features.Onboarding.TellUsYourInterests.Models;
using Source.Features.Onboarding.TellUsYourProficiency.Models;

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
}