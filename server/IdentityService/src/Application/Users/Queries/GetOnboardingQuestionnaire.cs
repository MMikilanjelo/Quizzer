using Application.Abstractions.Messaging;
using Domain.Users;
using ErrorOr;

namespace Application.Users.Queries;

public static class GetOnboardingQuestionnaire
{
    public sealed record Response(
        List<string> Goals,
        List<string> Interests,
        List<string> Proficiencies
    );

    public sealed record Query : IQuery<Response>;

    internal sealed class Handler : IQueryHandler<Query, Response>
    {
        public Task<ErrorOr<Response>> Handle(
            Query query,
            CancellationToken cancellationToken
        )
        {
            var response = new Response(
                Goals:Enum.GetValues<Goal>().Select(x => x.ToString()).ToList(),
                Interests:Enum.GetValues<Interest>().Select(x => x.ToString()).ToList(),
                Proficiencies: Enum.GetValues<Proficiency>().Select(x => x.ToString()).ToList()
            );

            return Task.FromResult(ErrorOrFactory.From(response));
        }
    }
}