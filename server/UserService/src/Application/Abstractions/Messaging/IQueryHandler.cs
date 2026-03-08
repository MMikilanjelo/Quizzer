using ErrorOr;

namespace Application.Abstractions.Messaging;

public interface IQueryHandler<in TQuery, TResponse> where TQuery : IQuery<TResponse>
{
    Task<ErrorOr<TResponse>> Handle(TQuery query, CancellationToken cancellationToken);
}