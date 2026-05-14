using Cysharp.Threading.Tasks;
using Source.Shared.Reactive.Events;

namespace Source.Shared.Reactive.Commands
{
    public interface ICommand
    {
        IReadOnlyReactiveEvent<EmptyEvent> Executed { get; }
        IReadOnlyReactiveProperty<CommandState> State { get; }
        UniTask Execute();
        void Dispose();
    }

    public interface ICommand<in T>
    {
        IReadOnlyReactiveEvent<EmptyEvent> Executed { get; }
        IReadOnlyReactiveProperty<CommandState> State { get; }
        UniTask Execute(T payload);
        void Dispose();
    }
}