using Microsoft.AspNetCore.Authorization;
using RestaurantAPI.Entities;
using System.Security.Claims;

namespace RestaurantAPI.Authorization
{
    public class ResourceOperationRequirementHandler : AuthorizationHandler<ResourceOperationRequirment, Restaurant>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, ResourceOperationRequirment requirement, Restaurant restaurant)
        {
            if (requirement.ResourceOperation == ResourceOperation.Read || requirement.ResourceOperation == ResourceOperation.Create)
            {
                context.Succeed(requirement);
            }

            var userId = context.User.FindFirst(c => c.Type == ClaimTypes.NameIdentifier).Value;

            if (restaurant.CreatedById == int.Parse(userId))
            {
                context.Succeed(requirement);
            }

            return Task.CompletedTask;
        }
    }
}
