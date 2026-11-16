using Microsoft.AspNetCore.Authorization;

namespace SchoolApp.Security.ResourceAuthorization;

public class OwnerOrCapabilityRequirement : IAuthorizationRequirement
{
    public string Capability { get; }

    public OwnerOrCapabilityRequirement(string capability)
    {
        Capability = capability;
    }
}