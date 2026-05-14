using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Source.Features.Auth.UseCases;
using Source.Shared;
using Source.Shared.Services;

namespace Source.Features.Auth
{
    public class AuthorizedWebApiService : IAuthorizedWebApiService
    {
        public string EmptyJsonPayload => _webApiService.EmptyJsonPayload;

        private readonly IWebApiService _webApiService;
        private readonly IUseCase<GetAccessToken.Response> _getAccessToken;
        private readonly IUseCase<RefreshAccessToken.Response> _refreshUseCase;

        public AuthorizedWebApiService(
            IWebApiService webApiService,
            IUseCase<GetAccessToken.Response> getAccessToken,
            IUseCase<RefreshAccessToken.Response> refreshUseCase
        )
        {
            _getAccessToken = getAccessToken;
            _refreshUseCase = refreshUseCase;
            _webApiService = webApiService;
        }


        public UniTask<Result<string>> Get(string endpoint, Dictionary<string, string> headers = null, CancellationToken ct = default) =>
            WithAuth(headers, h => _webApiService.Get(endpoint, h, ct), ct);

        public UniTask<Result<string>> Post(string endpoint, string jsonPayload, Dictionary<string, string> headers = null, CancellationToken ct = default) =>
            WithAuth(headers, h => _webApiService.Post(endpoint, jsonPayload, h, ct), ct);

        public UniTask<Result<string>> Put(string endpoint, string jsonPayload, Dictionary<string, string> headers = null, CancellationToken ct = default) =>
            WithAuth(headers, h => _webApiService.Put(endpoint, jsonPayload, h, ct), ct);

        public UniTask<Result<string>> Delete(string endpoint, Dictionary<string, string> headers = null, CancellationToken ct = default) =>
            WithAuth(headers, h => _webApiService.Delete(endpoint, h, ct), ct);

        private async UniTask<Result<string>> WithAuth(
            Dictionary<string, string> customHeaders,
            Func<Dictionary<string, string>, UniTask<Result<string>>> apiCall,
            CancellationToken ct
        )
        {
            var headers = await AddAuthorizationHeader(customHeaders, ct);

            if (!headers.IsSuccess)
            {
                return headers.Error;
            }

            var result = await apiCall(headers.Value);

            if (result.IsSuccess || result.Error.Code != ErrorCodes.Unauthorized)
            {
                return result;
            }

            var refreshResult = await _refreshUseCase.Execute(ct);

            if (refreshResult.IsSuccess)
            {
                var retryHeaders = await AddAuthorizationHeader(customHeaders, ct);

                return await apiCall(retryHeaders.Value);
            }

            return result;
        }

        private async UniTask<Result<Dictionary<string, string>>> AddAuthorizationHeader(Dictionary<string, string> customHeaders, CancellationToken ct)
        {
            var tokenResult = await _getAccessToken.Execute(ct);

            if (!tokenResult.IsSuccess)
            {
                return tokenResult.Error;
            }

            var headers = customHeaders ?? new Dictionary<string, string>();

            headers["Authorization"] = $"Bearer {tokenResult.Value.AccessToken}";

            return headers;
        }
    }
}