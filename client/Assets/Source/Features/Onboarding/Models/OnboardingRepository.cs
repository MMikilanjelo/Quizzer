using System.Collections.Generic;
using Source.Features.Onboarding.TellUsYourGoal.Models;
using Source.Features.Onboarding.TellUsYourInterests.Models;
using Source.Features.Onboarding.TellUsYourProficiency.Models;

namespace Source.Features.Onboarding.Models
{
    internal class OnboardingRepository : IOnboardingRepository
    {
        private OnboardingModel _data = new();

        public OnboardingModel Get() =>
            _data;

        public void SaveGoals(List<YourGoalModel> goals) =>
            _data = _data with { GoalModels = goals };

        public void SaveProficiencies(List<ProficiencyModel> proficiencies) =>
            _data = _data with { ProficiencyModels = proficiencies };

        public void SaveInterests(List<YourInterestModel> interests) =>
            _data = _data with { InterestModels = interests };

        public void Clear() =>
            _data = new OnboardingModel();

        public void SaveSelectedGoals(List<YourGoalModel> selectedGoals) =>
            _data = _data with { SelectedGoals = selectedGoals };

        public void SaveSelectedInterests(List<YourInterestModel> selectedInterests) =>
            _data = _data with { SelectedInterests = selectedInterests };
    }
}