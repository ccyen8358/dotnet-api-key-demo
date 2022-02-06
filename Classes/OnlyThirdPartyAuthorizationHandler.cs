using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

public class OnlyThirdPartyAuthorizationHandler : AuthorizationHandler<OnlyThirdPartyRequirement>
{
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, OnlyThirdPartyRequirement requirement)
    {
        if (context.User.IsInRole(Roles.ThirdParty))
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}