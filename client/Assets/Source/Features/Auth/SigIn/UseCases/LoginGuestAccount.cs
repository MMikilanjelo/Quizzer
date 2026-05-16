using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using Source.Features.Auth.Models;
using Source.Shared;
using Source.Shared.Extensions;
using Source.Shared.Services;
using UnityEngine.Device;

namespace Source.Features.Auth.SigIn.UseCases
{
    public static class LoginGuestAccount
    {
        public static class ErrorCodes
        {
            public const string UserNotRegistered = "User.NotFound";
        }

        private sealed record Request
        {
            public string GuestId { get; set; }
        }

        public sealed record Response
        {
            [JsonProperty] public string AccessToken { get; set; }
            [JsonProperty] public string RefreshToken { get; set; }
            [JsonProperty] public List<string> RequiredActions { get; set; }

            public bool IsOnboardingCompletionRequired => RequiredActions.Contains("CompleteOnboarding");
        }

        internal class UseCase : IUseCase<Response>
        {
            private readonly IWebApiService _webApi;
            private readonly ISerializationService _serializer;
            private readonly ITokensStore _tokensStore;
            private const string URL = "api/users/guests/login";

            internal UseCase(
                IWebApiService webApi,
                ISerializationService serializer,
                ITokensStore tokensStore
            )
            {
                _webApi = webApi;
                _serializer = serializer;
                _tokensStore = tokensStore;
            }

            public async UniTask<Result<Response>> Execute(CancellationToken cancellationToken = default)
            {
                return await _serializer
                    .Serialize(new Request
                    {
                        GuestId = SystemInfo.deviceUniqueIdentifier,
                    })
                    .Bind(serializedRequest => _webApi.Post(URL, serializedRequest, cancellationToken: cancellationToken))
                    .Bind(serializedResponse => _serializer.Deserialize<Response>(serializedResponse))
                    .Bind(response => _tokensStore
                        .SaveTokens(response.AccessToken, response.RefreshToken)
                        .Map(() => response)
                    );
            }
        }
    }
}