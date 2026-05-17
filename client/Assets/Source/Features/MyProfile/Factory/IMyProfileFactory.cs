using Cysharp.Threading.Tasks;
using Source.Features.MyProfile.Components;
using Source.Features.MyProfile.ViewModels;
using Source.Shared;

namespace Source.Features.MyProfile.Factory
{
    public interface IMyProfileFactory
    {
        UniTask<Result<MyProfileScreen>> CreateMyProfileScreen(IMyProfileScreenViewModel viewModel);
    }
}