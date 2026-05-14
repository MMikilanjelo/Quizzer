using System.Threading;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using Source.Features.Auth.Models;
using Source.Shared;
using Source.Shared.Extensions;
using Source.Shared.Services;

namespace Source.Features.Auth.UseCases
{
    public static class RefreshAccessToken
    {
        private sealed record Request
        {
            public string RefreshToken { get; set; }
        }

        public sealed record Response
        {
            [JsonProperty] public string AccessToken { get; set; }
            [JsonProperty] public string RefreshToken { get; set; }
        }

        internal class UseCase : IUseCase<Response>
        {
            private readonly IWebApiService _webApi;
            private readonly ISerializationService _serializer;
            private readonly ITokenRepository _tokenRepository;
            private const string URL = "api/auth/refresh";

            internal UseCase(
                IWebApiService webApi,
                ISerializationService serializer,
                ITokenRepository tokenRepository
            )
            {
                _webApi = webApi;
                _serializer = serializer;
                _tokenRepository = tokenRepository;
            }

            public UniTask<Result<Response>> Execute(CancellationToken cancellationToken = default)
            {
                return _tokenRepository.GetRefreshToken()
                    .Map(token => new Request { RefreshToken = token })
                    .Bind(request => _serializer.Serialize(request))
                    .Bind(json => _webApi.Post(URL, json, cancellationToken: cancellationToken))
                    .Bind(json => _serializer.Deserialize<Response>(json))
                    .Bind(data => _tokenRepository
                        .SaveTokens(data.AccessToken, data.RefreshToken)
                        .Map(() => data)
                    );
            }
        }
    }
}