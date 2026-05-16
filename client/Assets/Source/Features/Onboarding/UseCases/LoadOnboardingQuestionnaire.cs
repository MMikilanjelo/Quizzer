using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using Source.Features.Onboarding.Models;
using Source.Shared;
using Source.Shared.Extensions;
using Source.Shared.Services;

namespace Source.Features.Onboarding.UseCases
{
    public static class LoadOnboardingQuestionnaire
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
            private readonly IOnboardingStore _onboardingStore;
            private const string URL = "/api/users/onboarding/questionnaire";

            internal UseCase(
                IAuthorizedWebApiService webApi,
                ISerializationService serializer,
                IOnboardingStore onboardingStore
            )
            {
                _webApi = webApi;
                _serializer = serializer;
                _onboardingStore = onboardingStore;
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

                        _onboardingStore.Save(new OnboardingModel
                        {
                            InterestModels = interests,
                            GoalModels = goals,
                            ProficiencyModels = proficiencies
                        });

                        return new Response();
                    });
            }
        }
    }
}