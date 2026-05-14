using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using Source.Features.Quizzes.CreateQuiz.Models;
using Source.Shared;
using Source.Shared.Extensions;
using Source.Shared.Services;
using UnityEditor.PackageManager.Requests;

namespace Source.Features.Quizzes.CreateQuiz.UseCases
{
    public static class SmartScheduleQuiz
    {
        public sealed record Response;

        internal class UseCase : IUseCase<Response>
        {
            private readonly IAuthorizedWebApiService _webApi;
            private const string URL = "/api/quizzes/schedule/smart";

            internal UseCase(IAuthorizedWebApiService webApi)
            {
                _webApi = webApi;
            }

            public UniTask<Result<Response>> Execute(CancellationToken cancellationToken)
            {
                return _webApi
                    .Post(URL, _webApi.EmptyJsonPayload, cancellationToken: cancellationToken)
                    .Map(_ => new Response());
            }
        }
    }
}