using System.Text.Json;
using System.Text.Json.Serialization;
using Application.Abstractions;
using ErrorOr;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.Google;

namespace Infrastructure.Generators;

public sealed partial class GeminiContentGenerator(
    IChatCompletionService chatService,
    ILogger<GeminiContentGenerator> logger
) : IStructuredContentGenerator
{
    private readonly JsonSerializerOptions _jsonSerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() }
    };

    public async Task<ErrorOr<TResponse>> GenerateAsync<TResponse>(string prompt, CancellationToken cancellationToken)
    {
        try
        {
            var history = new ChatHistory();

            history.AddSystemMessage(
                "You are a strict data processing API. " +
                "Your only output must be raw, valid JSON that strictly conforms to the requested schema. " +
                "Do not wrap the response in markdown blocks (e.g., ```json). " +
                "Do not include any conversational text, explanations, or greetings."
            );

            history.AddUserMessage(prompt);

            var settings = new GeminiPromptExecutionSettings
            {
                ResponseMimeType = "application/json",
                ResponseSchema = typeof(TResponse),
                Temperature = 0.1f
            };

            LogAiGenerationStarted(logger, typeof(TResponse).Name);

            ChatMessageContent result = await chatService.GetChatMessageContentAsync(
                history,
                settings,
                kernel: null,
                cancellationToken: cancellationToken
            );

            string? jsonContent = result.Content;

            if (string.IsNullOrWhiteSpace(jsonContent))
            {
                LogAiReturnedEmptyResponse(logger);
                
                return Error.Failure("AI.EmptyResponse", "The AI returned an empty response.");
            }

            TResponse? responseObj = JsonSerializer.Deserialize<TResponse>(jsonContent, _jsonSerializerOptions);

            if (responseObj is null)
            {
                return Error.Failure("AI.Deserialization", "Failed to map AI JSON response to the requested type.");
            }

            LogAiGenerationCompleted(logger, typeof(TResponse).Name);

            return responseObj;
        }
        catch (JsonException ex)
        {
            LogAiJsonParseError(logger, ex);
            return Error.Validation("AI.ParseError", "AI returned invalid or malformed JSON.");
        }
        catch (Exception ex)
        {
            LogAiUnexpectedError(logger, ex);
            return Error.Unexpected("AI.GenerationError", ex.Message);
        }
    }

    [LoggerMessage(EventId = 101, Level = LogLevel.Information, Message = "AI generation started for type: {ResponseType}")]
    private static partial void LogAiGenerationStarted(ILogger logger, string responseType);

    [LoggerMessage(EventId = 102, Level = LogLevel.Warning, Message = "The AI returned an empty response content.")]
    private static partial void LogAiReturnedEmptyResponse(ILogger logger);

    [LoggerMessage(EventId = 103, Level = LogLevel.Information, Message = "AI generation successfully completed for type: {ResponseType}")]
    private static partial void LogAiGenerationCompleted(ILogger logger, string responseType);

    [LoggerMessage(EventId = 104, Level = LogLevel.Error, Message = "Failed to parse AI JSON response. Raw output might be malformed.")]
    private static partial void LogAiJsonParseError(ILogger logger, Exception ex);

    [LoggerMessage(EventId = 105, Level = LogLevel.Error, Message = "AI generation failed due to an unexpected error.")]
    private static partial void LogAiUnexpectedError(ILogger logger, Exception ex);
}
