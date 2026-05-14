using Application.Abstractions.Messaging;
using Application.Users;
using Application.Users.Queries;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Users;

internal sealed class GetOnboardingQuestionnaireEndpoint : IEndpoint<UserEndpointGroup>
{
    public void MapEndpoint(RouteGroupBuilder group) =>
        group.MapGet("onboarding/questionnaire", Handler);

    private static async Task<IResult> Handler(
        IQueryHandler<GetOnboardingQuestionnaire.Query, GetOnboardingQuestionnaire.Response> handler,
        CancellationToken ct
    )
    {
        var result = await handler.Handle(new GetOnboardingQuestionnaire.Query(), ct);

        return result.Match(
            Results.Ok,
            CustomResults.Problem
        );
    }
}