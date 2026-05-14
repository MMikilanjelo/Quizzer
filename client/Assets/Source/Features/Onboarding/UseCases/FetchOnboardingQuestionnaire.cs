using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using Source.Features.Onboarding.Models;
using Source.Features.Onboarding.TellUsYourGoal.Models;
using Source.Features.Onboarding.TellUsYourInterests.Models;
using Source.Features.Onboarding.TellUsYourProficiency.Models;
using Source.Shared;
using Source.Shared.Extensions;
using Source.Shared.Services;

namespace Source.Features.Onboarding.UseCases
{
    public static class FetchOnboardingQuestionnaire
    {
        public sealed record Response;

        private sealed record ResponseModel
        {
            [JsonProperty] public List<string> Goals { get; set; }
            [JsonProperty] public List<string> Interests { get; set; }
            [JsonProperty] public List<string> Proficiencies { get; set; }
        }

        internal class UseCase : IUseCase<Response>
        {
            private readonly IWebApiService _webApi;
            private readonly ISerializationService _serializer;
            private readonly IOnboardingRepository _onboardingRepository;
            private const string URL = "/api/users/onboarding/questionnaire";

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
                return _webApi
                    .Get(URL, cancellationToken: cancellationToken)
                    .Bind(serializedResponse => _serializer.Deserialize<ResponseModel>(serializedResponse))
                    .Map(response =>
                    {
                        var goals = response.Goals
                            .Select(g => new YourGoalModel(g, g))
                            .ToList();

                        var interests = response.Interests
                            .Select(i => new YourInterestModel(i, i))
                            .ToList();

                        var proficiencies = response.Proficiencies
                            .Select(i => new ProficiencyModel(i, i))
                            .ToList();

                        _onboardingRepository.SaveInterests(interests);
                        _onboardingRepository.SaveGoals(goals);
                        _onboardingRepository.SaveProficiencies(proficiencies);

                        return new Response();
                    });
            }
        }
    }
}