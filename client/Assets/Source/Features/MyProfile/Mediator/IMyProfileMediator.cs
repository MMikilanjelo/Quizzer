using Cysharp.Threading.Tasks;
using Source.Features.MyProfile.ViewModels;
using Source.Shared;

namespace Source.Features.MyProfile.Mediator
{
    public interface IMyProfileMediator
    {
        UniTask<Result> CreateMyProfileScreen(IMyProfileScreenViewModel viewModel);
    }
}