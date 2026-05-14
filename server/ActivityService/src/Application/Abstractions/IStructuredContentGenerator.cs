using ErrorOr;

namespace Application.Abstractions;

public interface IStructuredContentGenerator
{
   Task<ErrorOr<TResponse>> GenerateAsync<TResponse>(string prompt, CancellationToken cancellationToken);
}