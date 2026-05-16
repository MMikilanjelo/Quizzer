using System.Threading;
using Cysharp.Threading.Tasks;
using Source.Features.Quizzes.ActiveQuiz.Models;
using Source.Shared;
using Source.Shared.Extensions;
using Source.Shared.Services;

namespace Source.Features.Quizzes.ActiveQuiz.UseCases
{
    public class SubmitAnswer
    {
        public sealed record Request
        {
            public string Id { get; set; }
            public string QuestionId { get; set; }
            public int SelectedIndex { get; set; }
        }

        public sealed record Response;

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
                var endpoint = $"{URL}{request.Id}/answer";

                return _serializer
                    .Serialize(request)
                    .Bind(jsonPayload => _webApi.Post(endpoint, jsonPayload, cancellationToken: cancellationToken))
                    .Tap(_ =>
                    {
                        var activeQuiz = _activeQuizRepository.Get();

                        activeQuiz.RecordAnswer(request.QuestionId);
                    })
                    .Map(_ => new Response());
            }
        }
    }
}