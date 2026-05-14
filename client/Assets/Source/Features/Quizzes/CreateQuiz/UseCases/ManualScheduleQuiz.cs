using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using Source.Features.Quizzes.CreateQuiz.Models;
using Source.Shared;
using Source.Shared.Extensions;
using Source.Shared.Services;

namespace Source.Features.Quizzes.CreateQuiz.UseCases
{
    public static class ManualScheduleQuiz
    {
        public sealed record Response;

        private sealed record Request
        {
            public string TopicId { get; set; }
            public int QuestionCount { get; set; }
            public string DifficultyLevelId { get; set; }
        }

        internal class UseCase : IUseCase<Response>
        {
            private readonly IAuthorizedWebApiService _webApi;
            private readonly IQuizCreationRepository _quizCreationRepository;
            private readonly ISerializationService _serializer;
            private const string URL = "/api/quizzes/schedule/manual";

            internal UseCase(
                IAuthorizedWebApiService webApi,
                IQuizCreationRepository quizCreationRepository,
                ISerializationService serializer
            )
            {
                _webApi = webApi;
                _quizCreationRepository = quizCreationRepository;
                _serializer = serializer;
            }

            public UniTask<Result<Response>> Execute(CancellationToken cancellationToken)
            {
                return _serializer
                    .Serialize(new Request
                    {
                        TopicId = _quizCreationRepository.Get().SelectedDomain,
                        QuestionCount = _quizCreationRepository.Get().RequestedQuestions,
                        DifficultyLevelId = _quizCreationRepository.Get().SelectedDifficulty
                    })
                    .Bind(serializedRequest => _webApi.Post(URL, serializedRequest, cancellationToken: cancellationToken))
                    .Map(_ => new Response());
            }
        }
    }
}