using System.Collections.Generic;
using Source.Features.Onboarding.TellUsYourGoal.Models;
using Source.Features.Onboarding.TellUsYourInterests.Models;
using Source.Features.Onboarding.TellUsYourProficiency.Models;

namespace Source.Features.Onboarding.Models
{
    public interface IOnboardingRepository
    {
        OnboardingModel Get();
        void SaveGoals(List<YourGoalModel> goals);
        void SaveProficiencies(List<ProficiencyModel> proficiencies);
        void SaveInterests(List<YourInterestModel> interests);
        void SaveSelectedGoals(List<YourGoalModel> selectedGoals);
        void SaveSelectedInterests(List<YourInterestModel> selectedInterests);
        void Clear();
    }
}