using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using Source.Features.Onboarding.Models;
using Source.Shared;
using Source.Shared.Extensions;
using Source.Shared.Services;

namespace Source.Features.Onboarding.UseCases
{
    public static class SubmitOnboarding
    {
        public sealed record Response;

        internal sealed record Request
        {
            [JsonProperty] public List<string> Goals { get; set; }
            [JsonProperty] public List<string> Interests { get; set; }
            [JsonProperty] public string Proficiency { get; set; }
        }

        public class UseCase : IUseCase<Response>
        {
            private readonly IWebApiService _webApi;
            private readonly ISerializationService _serializer;
            private readonly IOnboardingRepository _onboardingRepository;
            private const string URL = "/api/users/me/onboarding";

            internal UseCase(
                IAuthorizedWebApiService webApi,
                ISerializationService serializer,
                IOnboardingRepository onboardingRepository
            )
            {
                _webApi = webApi;
                _serializer = serializer;
                _onboardingRepository = onboardingRepository;
            }

            public UniTask<Result<Response>> Execute(CancellationToken cancellationToken)
            {
                var mockRequest = new Request
                {
                    Goals = new List<string>() { "prepare_for_exam", "career_boost" },
                    Interests = new List<string>() { "data_science", "programming" },
                    Proficiency = "advanced"
                };

                return _serializer
                    .Serialize(mockRequest)
                    .Bind(serializedRequest => _webApi.Post(URL, serializedRequest, cancellationToken: cancellationToken))
                    .Map(_ => new Response());
            }
        }
    }
}