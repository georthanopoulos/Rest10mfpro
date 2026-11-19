using Microsoft.AspNetCore.Authorization;
using SchoolApp.Security.ResourceAuthorization;
using System.Security.Claims;

namespace SchoolApp.Security;

public class OwnerOrCapabilityHandler : AuthorizationHandler<OwnerOrCapabilityRequirement, int>
{
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context,
        OwnerOrCapabilityRequirement requirement, int ownerUserId)
    {
        bool hasCapability = context.User.HasClaim("Capability", requirement.Capability);

        bool isOwner = int.TryParse(context.User.FindFirstValue(ClaimTypes.NameIdentifier), out int currentUserId)
                       && currentUserId == ownerUserId;

        if (isOwner || hasCapability)
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}