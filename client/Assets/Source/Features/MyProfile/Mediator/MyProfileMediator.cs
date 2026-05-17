using Cysharp.Threading.Tasks;
using Source.Features.MyProfile.Factory;
using Source.Features.MyProfile.ViewModels;
using Source.Shared;
using Source.Shared.Extensions;
using Source.Shared.UIStack.Mediator;

namespace Source.Features.MyProfile.Mediator
{
    public class MyProfileMediator : IMyProfileMediator
    {
        private readonly IMyProfileFactory _factory;
        private readonly IUIStackMediator _stackMediator;

        public MyProfileMediator(IMyProfileFactory factory, IUIStackMediator stackMediator)
        {
            _factory = factory;
            _stackMediator = stackMediator;
        }

        public async UniTask<Result> CreateMyProfileScreen(IMyProfileScreenViewModel viewModel)
        {
            return await _factory
                .CreateMyProfileScreen(viewModel)
                .Tap(view => _stackMediator.Push(view));
        }
    }
}