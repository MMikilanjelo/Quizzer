using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using Source.Features.Quizzes.ActiveQuiz.Models;
using Source.Shared;
using Source.Shared.Extensions;
using Source.Shared.Services;

namespace Source.Features.Quizzes.ActiveQuiz.UseCases
{
    public static class LoadActiveQuiz
    {
        public sealed record Request
        {
            public string Id { get; set; }
        }

        public sealed record Response;

        private sealed record ResponseModel
        {
            [JsonProperty] public QuizResponseModel Quiz { get; set; }
        }

        private sealed record QuizResponseModel
        {
            [JsonProperty] public string Id { get; set; }
            [JsonProperty] public string UserId { get; set; }
            [JsonProperty] public List<string> Topics { get; set; }
            [JsonProperty] public string Name { get; set; }
            [JsonProperty] public string Status { get; set; }
            [JsonProperty] public List<QuestionModel> Questions { get; set; }
            [JsonProperty] public List<string> AnsweredQuestionIds { get; set; }
            [JsonProperty] public string CurrentQuestionId { get; set; }
            [JsonProperty] public int TotalQuestions { get; set; }
            [JsonProperty] public int AnsweredCount { get; set; }
        }

        private sealed record QuestionModel
        {
            [JsonProperty] public string Id { get; set; }
            [JsonProperty] public string ConceptId { get; set; }
            [JsonProperty] public string Text { get; set; }
            [JsonProperty] public List<string> Options { get; set; }
        }

        internal class UseCase : IUseCase<Request, Response>
        {
            private readonly IAuthorizedWebApiService _webApi;
            private readonly ISerializationService _serializer;
            private readonly IActiveQuizRepository _activeQuizRepository;
            private const string URL = "/api/quizzes/";

            internal UseCase(
                IAuthorizedWebApiService webApi,
                ISerializationService serializer,
                IActiveQuizRepository activeQuizRepository
            )
            {
                _webApi = webApi;
                _serializer = serializer;
                _activeQuizRepository = activeQuizRepository;
            }

            public UniTask<Result<Response>> Execute(Request request, CancellationToken cancellationToken = default)
            {
                var endpoint = $"{URL}{request.Id}";

                return _webApi
                    .Get(endpoint, cancellationToken: cancellationToken)
                    .Bind(serializedResponse => _serializer.Deserialize<ResponseModel>(serializedResponse))
                    .Map(response =>
                    {
                        var activeQuiz = new ActiveQuizModel
                        {
                            Id = response.Quiz.Id,
                            Name = response.Quiz.Name,
                            Topics = response.Quiz.Topics,
                            Questions = response.Quiz.Questions.Select(q => new ActiveQuizQuestionModel
                            {
                                Id = q.Id,
                                Text = q.Text,
                                Options = q.Options
                            }).ToList(),
                            AnsweredQuestionIds = response.Quiz.AnsweredQuestionIds.ToHashSet()
                        };

                        _activeQuizRepository.Save(activeQuiz);

                        return new Response();
                    });
            }
        }
    }
}