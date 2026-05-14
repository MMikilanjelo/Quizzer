using Source.Shared;
using Source.Shared.Reactive.Events;

namespace Source.App.Mediator
{
    public interface IAppMediator
    {
        IReactiveEvent<Error> TechnicalErrorOccured { get; }
    }
}