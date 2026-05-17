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

        public sealed record Request
        {
            public string DomainId { get; set; }
            public int QuestionCount { get; set; }
            public string DifficultyLevel { get; set; }
        }

        internal class UseCase : IUseCase<Request, Response>
        {
            private readonly IAuthorizedWebApiService _webApi;
            private readonly IQuizCreationStore _quizCreationStore;
            private readonly ISerializationService _serializer;
            private const string URL = "/api/quizzes/schedule/manual";

            internal UseCase(
                IAuthorizedWebApiService webApi,
                IQuizCreationStore quizCreationStore,
                ISerializationService serializer
            )
            {
                _webApi = webApi;
                _quizCreationStore = quizCreationStore;
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