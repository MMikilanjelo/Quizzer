using System.Threading;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using Source.Features.Auth.Models;
using Source.Shared;
using Source.Shared.Extensions;
using Source.Shared.Services;

namespace Source.Features.Auth.UseCases
{
    public static class GetAccessToken
    {
        public sealed record Response
        {
            public string AccessToken { get; set; }
        }

        internal class UseCase : IUseCase<Response>
        {
            private readonly ITokenRepository _tokenRepository;

            internal UseCase(ITokenRepository tokenRepository)
            {
                _tokenRepository = tokenRepository;
            }

            public UniTask<Result<Response>> Execute(CancellationToken cancellationToken = default)
            {
                var result = _tokenRepository
                    .GetAccessToken()
                    .Map(token => new Response { AccessToken = token });

                return UniTask.FromResult(result);
            }
        }
    }
}