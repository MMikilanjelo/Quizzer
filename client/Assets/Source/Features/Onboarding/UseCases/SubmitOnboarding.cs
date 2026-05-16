using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Source.Shared;
using Source.Shared.Extensions;
using Source.Shared.Services;

namespace Source.Features.Onboarding.UseCases
{
    public static class SubmitOnboarding
    {
        public sealed record Response;

        public sealed record Request
        {
            public List<string> Goals { get; set; }
            public List<string> Interests { get; set; }
            public string Proficiency { get; set; }
        }

        public class UseCase : IUseCase<Request, Response>
        {
            private readonly IWebApiService _webApi;
            private readonly ISerializationService _serializer;
            private const string URL = "/api/users/me/onboarding";

            internal UseCase(
                IAuthorizedWebApiService webApi,
                ISerializationService serializer
            )
            {
                _webApi = webApi;
                _serializer = serializer;
            }

            public UniTask<Result<Response>> Execute(Request request, CancellationToken cancellationToken)
            {
                return _serializer
                    .Serialize(request)
                    .Bind(serializedRequest => _webApi.Post(URL, serializedRequest, cancellationToken: cancellationToken))
                    .Map(_ => new Response());
            }
        }
    }
}