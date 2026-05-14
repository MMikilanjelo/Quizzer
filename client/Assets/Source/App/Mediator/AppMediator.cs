using Source.Shared;
using Source.Shared.Reactive.Events;

namespace Source.App.Mediator
{
    public class AppMediator : IAppMediator
    {
        public IReactiveEvent<Error> TechnicalErrorOccured { get; } = new ReactiveEvent<Error>();
    }
}

