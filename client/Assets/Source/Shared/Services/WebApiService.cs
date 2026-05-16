using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Text;
using System.Threading;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine.Networking;

namespace Source.Shared.Services
{
    public interface IAuthorizedWebApiService : IWebApiService
    {
    }

    public interface IWebApiService
    {
        string EmptyJsonPayload { get; }
        UniTask<Result<string>> Get(string endpoint, Dictionary<string, string> headers = null, CancellationToken cancellationToken = default);
        UniTask<Result<string>> Post(string endpoint, string jsonPayload, Dictionary<string, string> headers = null, CancellationToken cancellationToken = default);
        UniTask<Result<string>> Put(string endpoint, string jsonPayload, Dictionary<string, string> headers = null, CancellationToken cancellationToken = default);
        UniTask<Result<string>> Delete(string endpoint, Dictionary<string, string> headers = null, CancellationToken cancellationToken = default);
    }

    public class WebApiService : IWebApiService
    {
        public string EmptyJsonPayload => "{}";

        private readonly Uri _baseAddress;
        private readonly int _timeoutSeconds;
        private readonly ILoggingService _loggingService;
        private readonly ISerializationService _serializationService;

        public WebApiService(ILoggingService loggingService, ISerializationService serializationService)
        {
            _loggingService = loggingService;
            _serializationService = serializationService;
            _baseAddress = new Uri("https://dev-api.quizzer.it.com/");
            _timeoutSeconds = 15;
        }


        public UniTask<Result<string>> Get(string endpoint, Dictionary<string, string> headers = null, CancellationToken cancellationToken = default)
        {
            _loggingService.Info("Sending GET request to {Endpoint}", endpoint);

            var uwr = UnityWebRequest.Get(new Uri(_baseAddress, endpoint));
            SetCustomHeaders(uwr, headers);

            return SendRequest(uwr, endpoint, cancellationToken);
        }

        public UniTask<Result<string>> Post(string endpoint, string jsonPayload, Dictionary<string, string> headers = null, CancellationToken cancellationToken = default)
        {
            _loggingService.Info("Sending POST request to {Endpoint}", endpoint);

            var uwr = new UnityWebRequest(new Uri(_baseAddress, endpoint), UnityWebRequest.kHttpVerbPOST);
            AttachJsonPayload(uwr, jsonPayload);
            SetCustomHeaders(uwr, headers);

            return SendRequest(uwr, endpoint, cancellationToken);
        }

        public UniTask<Result<string>> Put(string endpoint, string jsonPayload, Dictionary<string, string> headers = null, CancellationToken cancellationToken = default)
        {
            _loggingService.Info("Sending PUT request to {Endpoint}", endpoint);

            var uwr = new UnityWebRequest(new Uri(_baseAddress, endpoint), UnityWebRequest.kHttpVerbPUT);
            AttachJsonPayload(uwr, jsonPayload);
            SetCustomHeaders(uwr, headers);

            return SendRequest(uwr, endpoint, cancellationToken);
        }

        public UniTask<Result<string>> Delete(string endpoint, Dictionary<string, string> headers = null, CancellationToken cancellationToken = default)
        {
            _loggingService.Info("Sending DELETE request to {Endpoint}", endpoint);

            var uwr = UnityWebRequest.Delete(new Uri(_baseAddress, endpoint));
            uwr.downloadHandler = new DownloadHandlerBuffer();
            SetCustomHeaders(uwr, headers);

            return SendRequest(uwr, endpoint, cancellationToken);
        }

        private async UniTask<Result<string>> SendRequest(UnityWebRequest uwr, string endpoint, CancellationToken cancellationToken)
        {
            using (uwr)
            {
                try
                {
                    await uwr
                        .SendWebRequest()
                        .WithCancellation(cancellationToken)
                        .Timeout(TimeSpan.FromSeconds(_timeoutSeconds));

                    return uwr.downloadHandler.text;
                }
                catch (Exception ex)
                {
                    return MapExceptionToError(ex, uwr.method, endpoint);
                }
            }
        }

        private Result<string> MapExceptionToError(Exception ex, string method, string endpoint)
        {
            switch (ex)
            {
                case OperationCanceledException:
                    _loggingService.Warning("{Method} request to {Endpoint} was cancelled.", method, endpoint);
                    return Result<string>.Failure(Error.Cancelled);

                case TimeoutException:
                    _loggingService.Warning("{Method} request to {Endpoint} timed out.", method, endpoint);
                    return Result<string>.Failure(Error.Connectivity(ErrorCodes.NetworkTimeout));

                case not UnityWebRequestException:
                    _loggingService.Error(ex, "Non-web exception in {Method} {Endpoint}: {Message}", method, endpoint, ex.Message);
                    return Result<string>.Failure(Error.Connectivity(ErrorCodes.NetworkUnknown));
            }

            var webEx = (UnityWebRequestException)ex;

            if (webEx.Result == UnityWebRequest.Result.ConnectionError)
            {
                return Result<string>.Failure(Error.Connectivity(ErrorCodes.NetworkConnection));
            }

            return webEx.ResponseCode switch
            {
                401 => Result<string>.Failure(ParseServerError(webEx, ErrorCodes.Unauthorized)),
                403 => Result<string>.Failure(ParseServerError(webEx, ErrorCodes.Forbidden)),
                404 => Result<string>.Failure(ParseServerError(webEx, ErrorCodes.ResourceNotFound)),
                409 => Result<string>.Failure(ParseServerError(webEx, ErrorCodes.ServerConflict)),
                400 or 500 => Result<string>.Failure(ParseServerError(webEx, ErrorCodes.ServerError)),
                _ => Result<string>.Failure(Error.Remote(ErrorCodes.HttpError((int)webEx.ResponseCode)))
            };
        }

        private Error ParseServerError(UnityWebRequestException ex, string defaultErrorCode)
        {
            if (string.IsNullOrEmpty(ex.Text))
            {
                return Error.Remote(defaultErrorCode);
            }

            var result = _serializationService.Deserialize<ProblemDetails>(ex.Text);

            if (result.IsFailure || result.Value.Errors is not { Length: > 0 })
            {
                return Error.Remote(defaultErrorCode);
            }

            return Error.Remote(result.Value.Errors[0].Code);
        }

        private static void AttachJsonPayload(UnityWebRequest uwr, string jsonPayload)
        {
            if (string.IsNullOrEmpty(jsonPayload))
            {
                return;
            }

            var jsonToSend = Encoding.UTF8.GetBytes(jsonPayload);
            uwr.uploadHandler = new UploadHandlerRaw(jsonToSend);
            uwr.downloadHandler = new DownloadHandlerBuffer();
            uwr.SetRequestHeader("Content-Type", "application/json");
        }

        private static void SetCustomHeaders(UnityWebRequest uwr, Dictionary<string, string> headers)
        {
            if (headers == null)
            {
                return;
            }

            foreach (var header in headers)
            {
                uwr.SetRequestHeader(header.Key, header.Value);
            }
        }

        private record ProblemDetails
        {
            [JsonProperty] public ApiError[] Errors { get; set; }
        }

        [Serializable]
        private record ApiError
        {
            [JsonProperty] public string Code { get; set; }
            [JsonProperty] public string Message { get; set; }
        }
    }
}