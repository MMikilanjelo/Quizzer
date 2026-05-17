using System;
using Source.App.StateMachine.States.MainState.StateMachine.States;
using Source.Shared.StateMachine;
using VContainer;

namespace Source.App.StateMachine.States.MainState.StateMachine
{
    public class ApplicationStateMachine : StateMachine<ApplicationState>
    {
        public ApplicationStateMachine(
            SignInState signInState,
            TellUsYourInterestsState tellUsYourInterestsState,
            TellUsYourGoalState tellUsYourGoalState,
            TellUsYourProficiencyLevelState tellUsYourProficiencyLevelState,
            HomeState homeState,
            MyProfileState myProfileState,
            MyQuizzesState myQuizzesState,
            CreateQuizState createQuizState,
            ActiveQuizState activeQuizState,
            FinishedQuizState finishedQuizState
        )
        {
            signInState.Bind(this);
            tellUsYourInterestsState.Bind(this);
            tellUsYourGoalState.Bind(this);
            tellUsYourProficiencyLevelState.Bind(this);
            homeState.Bind(this);
            myProfileState.Bind(this);
            myQuizzesState.Bind(this);
            createQuizState.Bind(this);
            activeQuizState.Bind(this);
            finishedQuizState.Bind(this);

            RegisterState(signInState);
            RegisterState(tellUsYourInterestsState);
            RegisterState(tellUsYourGoalState);
            RegisterState(tellUsYourProficiencyLevelState);
            RegisterState(homeState);
            RegisterState(myProfileState);
            RegisterState(myQuizzesState);
            RegisterState(createQuizState);
            RegisterState(activeQuizState);
            RegisterState(finishedQuizState);
        }
    }
}