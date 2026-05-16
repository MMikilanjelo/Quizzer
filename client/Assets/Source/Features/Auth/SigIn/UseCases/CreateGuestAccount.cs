using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Source.Shared;
using Source.Shared.Extensions;
using Source.Shared.Services;
using UnityEngine.Device;

namespace Source.Features.Auth.SigIn.UseCases
{
    public static class CreateGuestAccount
    {
        public static class ErrorCodes
        {
            public const string AlreadyRegister = "User.DeviceAlreadyLinked";
        }

        private sealed record Request
        {
            public string GuestId { get; set; }
        }

        public sealed record Response;

        internal class UseCase : IUseCase<Response>
        {
            private readonly IWebApiService _webApi;
            private readonly ISerializationService _serializer;
            private const string URL = "/api/users/guests";

            internal UseCase(
                IWebApiService webApi,
                ISerializationService serializer
            )
            {
                _webApi = webApi;
                _serializer = serializer;
            }

            public UniTask<Result<Response>> Execute(CancellationToken cancellationToken)
            {
                return _serializer
                    .Serialize(new Request
                    {
                        GuestId = SystemInfo.deviceUniqueIdentifier
                    })
                    .Bind(serializedRequest => _webApi.Post(URL, serializedRequest, cancellationToken: cancellationToken))
                    .Map(_ => new Response());
            }
        }
    }
}