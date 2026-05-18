using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using Source.Shared;
using Source.Shared.Extensions;
using Source.Shared.Services;

namespace Source.Features.MyProfile.UseCase
{
    public static class FetchMyProfile
    {
        public sealed record Request;

        public sealed record Response
        {
            public UserDashboardModel Dashboard { get; set; }
        }

        public sealed record UserDashboardModel
        {
            public string Id { get; set; }
            public float AverageScore { get; set; }
            public int TotalQuizzes { get; set; }
            public int PerfectQuizzes { get; set; }
            public int StreakDays { get; set; }
            public DateTime? LastQuizDate { get; set; }
            public List<TopicMasteryLevelModel> TopStrengths { get; set; } = new();
            public List<TopicMasteryLevelModel> FocusAreas { get; set; } = new();
        }

        public sealed record TopicMasteryLevelModel
        {
            public string TopicId { get; set; }
            public int MasteryPercentage { get; set; }
        }

        private sealed record ResponseModel
        {
            [JsonProperty] public string Id { get; set; }
            [JsonProperty] public float AverageScore { get; set; }
            [JsonProperty] public int TotalQuizzes { get; set; }
            [JsonProperty] public int PerfectQuizzes { get; set; }
            [JsonProperty] public int StreakDays { get; set; }
            [JsonProperty] public DateTime? LastQuizDate { get; set; }
            [JsonProperty] public List<TopicMasteryResponseModel> TopStrengths { get; set; }
            [JsonProperty] public List<TopicMasteryResponseModel> FocusAreas { get; set; }
        }

        private sealed record TopicMasteryResponseModel
        {
            [JsonProperty] public string TopicId { get; set; }
            [JsonProperty] public int MasteryPercentage { get; set; }
        }

        internal class UseCase : IUseCase<Request, Response>
        {
            private readonly IAuthorizedWebApiService _webApi;
            private readonly ISerializationService _serializer;

            private const string URL = "/api/users/me/dashboard";

            internal UseCase(
                IAuthorizedWebApiService webApi,
                ISerializationService serializer
            )
            {
                _webApi = webApi;
                _serializer = serializer;
            }

            public async UniTask<Result<Response>> Execute(Request request, CancellationToken cancellationToken = default)
            {
                return await _webApi
                    .Get(URL, cancellationToken: cancellationToken)
                    .Bind(serializedResponse => _serializer.Deserialize<ResponseModel>(serializedResponse))
                    .Map(response =>
                    {
                        var dashboardData = new UserDashboardModel
                        {
                            Id = response.Id,
                            AverageScore = response.AverageScore,
                            TotalQuizzes = response.TotalQuizzes,
                            PerfectQuizzes = response.PerfectQuizzes,
                            StreakDays = response.StreakDays,
                            LastQuizDate = response.LastQuizDate,

                            TopStrengths = response.TopStrengths?.Select(t => new TopicMasteryLevelModel
                            {
                                TopicId = t.TopicId,
                                MasteryPercentage = t.MasteryPercentage
                            }).ToList() ?? new List<TopicMasteryLevelModel>(),

                            FocusAreas = response.FocusAreas?.Select(t => new TopicMasteryLevelModel
                            {
                                TopicId = t.TopicId, 
                                MasteryPercentage = t.MasteryPercentage
                            }).ToList() ?? new List<TopicMasteryLevelModel>()
                        };

                        return new Response { Dashboard = dashboardData };
                    });
            }
        }
    }
}