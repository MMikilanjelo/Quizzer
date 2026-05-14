using Source.Features.FAB.Mediator;
using Source.Features.TabBar.Mediator;
using Source.Shared.StateMachine.States;

namespace Source.App.StateMachine.States.MainState.StateMachine.States
{
    public class HomeState :
        ApplicationState,
        IEnterState,
        IExitState
    {
        private readonly ITabBarMediator _tabBarMediator;
        private readonly IFabMediator _fabMediator;

        public HomeState(
            ITabBarMediator tabBarMediator,
            IFabMediator fabMediator
        )
        {
            _tabBarMediator = tabBarMediator;
            _fabMediator = fabMediator;
        }

        public void Enter()
        {
            _tabBarMediator.Show();
        }

        public void Exit()
        {
        }
    }
}