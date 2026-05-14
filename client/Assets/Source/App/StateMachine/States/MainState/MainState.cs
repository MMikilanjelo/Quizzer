using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Source.App.Mediator;
using Source.App.StateMachine.States.MainState.StateMachine;
using Source.App.StateMachine.States.MainState.StateMachine.States;
using Source.Features.FAB.Mediator;
using Source.Features.FAB.ViewModels;
using Source.Features.TabBar.Mediator;
using Source.Features.TabBar.Models;
using Source.Features.TabBar.ViewModels;
using Source.Features.TechnicalDialogs.ErrorDialog;
using Source.Features.TechnicalDialogs.Mediator;
using Source.Shared;
using Source.Shared.Icons;
using Source.Shared.Reactive.Commands;
using Source.Shared.Services;
using Source.Shared.StateMachine.States;
using Source.Shared.UIStack.Mediator;
using UnityEngine;

namespace Source.App.StateMachine.States.MainState
{
    public class MainState :
        GlobalState,
        IEnterState
    {
        private readonly IAssetProviderService _assetProviderService;

        private readonly ITabBarMediator _tabBarMediator;
        private readonly ApplicationStateMachine _applicationStateMachine;
        private readonly IFabMediator _fabMediator;
        private readonly IAppMediator _appMediator;
        private readonly ITechnicalDialogsMediator _technicalDialogsMediator;
        private readonly IUIStackMediator _uiStackMediator;

        public MainState(
            ApplicationStateMachine applicationStateMachine,
            ITabBarMediator tabBarMediator,
            IAssetProviderService assetProviderService,
            IFabMediator fabMediator,
            IAppMediator appMediator,
            ITechnicalDialogsMediator technicalDialogsMediator,
            IUIStackMediator uiStackMediator
        )
        {
            _tabBarMediator = tabBarMediator;
            _assetProviderService = assetProviderService;
            _applicationStateMachine = applicationStateMachine;
            _fabMediator = fabMediator;
            _appMediator = appMediator;
            _technicalDialogsMediator = technicalDialogsMediator;
            _uiStackMediator = uiStackMediator;
        }

        public void Enter()
        {
            _appMediator.TechnicalErrorOccured.Subscribe(OnTechnicalErrorOccured);

            _tabBarMediator.Initialize();
            _fabMediator.Initialize();

            PrepareTabBar();

            _applicationStateMachine.Enter<SignInState>();
        }

        private void PrepareTabBar()
        {
            _tabBarMediator.Set(new List<TabBarItemViewModel>
            {
                new(new TabItemModel(nameof(HomeState), Icons.Home), true),
                new(new TabItemModel(nameof(MyQuizzesState), Icons.SealQuestion)),
                new(new TabItemModel(nameof(ProfileState), Icons.UserCircle))
            });
            _tabBarMediator.TabSelectionChanged.Subscribe(OnTabSelectionChanged);
        }

        private void OnTabSelectionChanged(TabBarItemViewModel model)
        {
            if (!model.IsSelected.Value)
            {
                return;
            }

            switch (model.Model.Id)
            {
                case nameof(HomeState):
                    _applicationStateMachine.Enter<HomeState>();
                    break;
                case nameof(MyQuizzesState):
                    _applicationStateMachine.Enter<MyQuizzesState>();
                    break;
                case nameof(ProfileState):
                    _applicationStateMachine.Enter<ProfileState>();
                    break;
            }
        }

        private void OnTechnicalErrorOccured(Error error)
        {
            _technicalDialogsMediator.CreateErrorDialog(new ErrorDialogViewModel(
                SyncCommand.Create(_uiStackMediator.PopDialog),
                ErrorModel.From(error)
            ));
        }
    }
}