using Microsoft.AspNetCore.Authorization;

namespace Assetgaze.Backend.Features.Users
{
    // This handler works with a Requirement and the Resource type (Guid for accountId)
    public class HasAccountPermissionHandler : AuthorizationHandler<HasAccountPermissionRequirement, Guid>
    {
        protected override Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            HasAccountPermissionRequirement requirement,
            Guid resourceAccountId) // The AccountId from the request URL
        {
            // Find all "account_permission" claims in the user's JWT
            var permissionClaims = context.User.FindAll("account_permission");

            // Check if the user has a claim that matches the accountId they are trying to access
            if (permissionClaims.Any(c => c.Value.Equals(resourceAccountId.ToString(), StringComparison.OrdinalIgnoreCase)))
            {
                // Success! The user has permission.
                context.Succeed(requirement);
            }
            else
            {
                // The user does not have permission for this specific account.
                context.Fail();
            }

            return Task.CompletedTask;
        }
    }

    // This is a simple marker requirement for our policy
    public class HasAccountPermissionRequirement : IAuthorizationRequirement { }
}