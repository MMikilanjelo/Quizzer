using Source.Shared.Reactive.Commands;

namespace Source.Features.Auth.SigIn.ViewModels
{
    public interface ISignInScreenViewModel
    {
        ICommand SignUpCommand { get; }
    }
}