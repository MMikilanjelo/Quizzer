using System.Threading;
using Cysharp.Threading.Tasks;
using Source.Features.Quizzes.MyQuizzes.Models;
using Source.Shared;
using Source.Shared.Extensions;
using Source.Shared.Services;

namespace Source.Features.Quizzes.MyQuizzes.UseCases
{
    public static class FetchQuizzes
    {
        public sealed record Request
        {
            public QuizFilter Filter { get; set; }
            public int Page { get; set; }
            public int PageSize { get; set; }

            public string ToQueryString() =>
                $"?filter={Filter.ToString()}&page={Page}&pageSize={PageSize}";
        }

        public sealed record Response
        {
            public PageResponse<QuizModel> Quizzes { get; set; }
        }

        public sealed record QuizResponseModel
        {
            public string Id { get; set; }
            public string Topic { get; set; }
            public string Status { get; set; }
            public int QuestionCount { get; set; }
            public int AnsweredCount { get; set; }
        }

        internal class UseCase : IUseCase<Request, Response>
        {
            private readonly IAuthorizedWebApiService _webApi;
            private readonly ISerializationService _serializer;
            private const string URL = "/api/quizzes/me";

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
                var endpoint = $"{URL}{request.ToQueryString()}";
                
                return await _webApi
                    .Get(endpoint, cancellationToken: cancellationToken)
                    .Bind(serializedResponse => _serializer.Deserialize<Response>(serializedResponse))
                    .Map(response => response);
            }
        }
    }
}