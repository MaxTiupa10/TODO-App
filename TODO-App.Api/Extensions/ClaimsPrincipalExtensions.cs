using System.Security.Claims;

namespace TODO_App.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static int GetUserId(this ClaimsPrincipal user)
    {
        var claim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (claim == null || !int.TryParse(claim, out var userId))
            throw new UnauthorizedAccessException("User is not authenticated.");
        return userId;
    }
}
