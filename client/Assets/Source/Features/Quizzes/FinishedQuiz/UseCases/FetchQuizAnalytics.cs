using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using Source.Shared;
using Source.Shared.Extensions;
using Source.Shared.Services;

namespace Source.Features.Quizzes.FinishedQuiz.UseCases
{
    public static class FetchQuizAnalytics
    {
        public sealed record Request
        {
            public string Id { get; set; }
        }

        public sealed record Response
        {
            public QuizAnalyticsModel Analytics { get; set; }
        }

        public sealed record QuizAnalyticsModel
        {
            public string Id { get; set; }
            public string Name { get; set; }
            public List<string> Topics { get; set; }
            public int TotalQuestions { get; set; }
            public int AnsweredCount { get; set; }
            public int CorrectCount { get; set; }
            public int IncorrectCount { get; set; }
            public List<ConceptMasteryDeltaModel> MasteryChanges { get; set; }
        }

        public sealed record ConceptMasteryDeltaModel
        {
            public string ConceptId { get; set; }
            public double StartingMastery { get; set; }
            public double EndingMastery { get; set; }
            public int Attempts { get; set; }
        }

        private sealed record ResponseModel
        {
            [JsonProperty] public AnalyticsResponseModel Analytics { get; set; }
        }

        private sealed record AnalyticsResponseModel
        {
            [JsonProperty] public string Id { get; set; }
            [JsonProperty] public string UserId { get; set; }
            [JsonProperty] public string Name { get; set; }
            [JsonProperty] public List<string> Topics { get; set; }
            [JsonProperty] public string Status { get; set; }
            [JsonProperty] public DateTime CreatedAt { get; set; }
            [JsonProperty] public int TotalQuestions { get; set; }
            [JsonProperty] public int AnsweredCount { get; set; }
            [JsonProperty] public int CorrectCount { get; set; }
            [JsonProperty] public int IncorrectCount { get; set; }
            [JsonProperty] public List<MasteryDeltaResponseModel> MasteryChanges { get; set; }
        }

        private sealed record MasteryDeltaResponseModel
        {
            [JsonProperty] public string ConceptId { get; set; }
            [JsonProperty] public double StartingMastery { get; set; }
            [JsonProperty] public double EndingMastery { get; set; }
            [JsonProperty] public int Attempts { get; set; }
        }

        internal class UseCase : IUseCase<Request, Response>
        {
            private readonly IAuthorizedWebApiService _webApi;
            private readonly ISerializationService _serializer;
            private const string URL = "/api/quizzes/";

            internal UseCase(
                IAuthorizedWebApiService webApi,
                ISerializationService serializer
            )
            {
                _webApi = webApi;
                _serializer = serializer;
            }

            public UniTask<Result<Response>> Execute(Request request, CancellationToken cancellationToken = default)
            {
                var endpoint = $"{URL}{request.Id}/analytics";

                return _webApi
                    .Get(endpoint, cancellationToken: cancellationToken)
                    .Bind(serializedResponse => _serializer.Deserialize<ResponseModel>(serializedResponse))
                    .Map(response =>
                    {
                        var analyticsData = new QuizAnalyticsModel
                        {
                            Id = response.Analytics.Id,
                            Name = response.Analytics.Name,
                            Topics = response.Analytics.Topics ?? new List<string>(),
                            TotalQuestions = response.Analytics.TotalQuestions,
                            AnsweredCount = response.Analytics.AnsweredCount,
                            CorrectCount = response.Analytics.CorrectCount,
                            IncorrectCount = response.Analytics.IncorrectCount,
                            MasteryChanges = response.Analytics.MasteryChanges?.Select(m => new ConceptMasteryDeltaModel
                            {
                                ConceptId = m.ConceptId,
                                StartingMastery = m.StartingMastery,
                                EndingMastery = m.EndingMastery,
                                Attempts = m.Attempts
                            }).ToList() ?? new List<ConceptMasteryDeltaModel>()
                        };

                        return new Response { Analytics = analyticsData };
                    });
            }
        }
    }
}