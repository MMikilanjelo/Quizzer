namespace Source.Features.Onboarding.Models
{
    public interface IOnboardingStore
    {
        OnboardingModel Get();
        void Save(OnboardingModel newData);
        void Clear();
    }

    internal class OnboardingStore : IOnboardingStore
    {
        private OnboardingModel _data;

        public OnboardingModel Get() =>
            _data;

        public void Save(OnboardingModel newData) =>
            _data = newData;

        public void Clear() =>
            _data = null;
    }
}