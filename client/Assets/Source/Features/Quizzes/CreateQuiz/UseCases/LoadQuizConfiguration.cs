using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using Source.Features.Quizzes.CreateQuiz.Models;
using Source.Shared;
using Source.Shared.Extensions;
using Source.Shared.Services;

namespace Source.Features.Quizzes.CreateQuiz.UseCases
{
    public class LoadQuizConfiguration
    {
        public sealed record Response;

        private sealed record ResponseModel
        {
            [JsonProperty] public List<string> Difficulties { get; set; }
            [JsonProperty] public List<string> Domains { get; set; }
            [JsonProperty] public int MaxQuestions { get; set; }
            [JsonProperty] public int MinQuestions { get; set; }
        }

        internal class UseCase : IUseCase<Response>
        {
            private readonly IAuthorizedWebApiService _webApi;
            private readonly ISerializationService _serializer;
            private readonly IQuizCreationStore _quizCreationStore;
            private const string URL = "/api/quizzes/configuration";

            internal UseCase(
                IAuthorizedWebApiService webApi,
                ISerializationService serializer,
                IQuizCreationStore quizCreationStore
            )
            {
                _webApi = webApi;
                _serializer = serializer;
                _quizCreationStore = quizCreationStore;
            }

            public UniTask<Result<Response>> Execute(CancellationToken cancellationToken)
            {
                return _webApi
                    .Get(URL, cancellationToken: cancellationToken)
                    .Bind(serializedResponse => _serializer.Deserialize<ResponseModel>(serializedResponse))
                    .Map(response =>
                    {
                        var quizCreationModel = new QuizCreationModel
                        {
                            Configuration = new QuizConfigurationModel
                            {
                                Difficulties = response.Difficulties,
                                Domains = response.Domains,
                                MaxQuestions = response.MaxQuestions,
                                MinQuestions = response.MinQuestions
                            }
                        };

                        _quizCreationStore.Save(quizCreationModel);
                        return new Response();
                    });
            }
        }
    }
}