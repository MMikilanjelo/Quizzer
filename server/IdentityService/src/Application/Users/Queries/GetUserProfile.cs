using Application.Abstractions.Messaging;
using Application.Users.Views;
using ErrorOr;
using Marten;

namespace Application.Users.Queries;

public static class GetUserProfile
{
    public sealed record Query(string UserId) : IQuery<Response>;
    public sealed record Response(UserProfileView Profile);

    internal sealed class Handler(IQuerySession session) : IQueryHandler<Query, Response>
    {
        public async Task<ErrorOr<Response>> Handle(Query query, CancellationToken cancellationToken)
        {
            var profile = await session.LoadAsync<UserProfileView>(query.UserId, cancellationToken);

            if (profile is null)
            {
                return Error.NotFound("User.ProfileNotFound", "User profile was not found.");
            }

            return new Response(profile);
        }
    }
}