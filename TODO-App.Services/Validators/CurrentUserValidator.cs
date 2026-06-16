using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using TODO_App.Domain.Interfaces;

namespace TODO_App.Services.Validators;

internal static class CurrentUserValidator
{
    public static int? GetUserId(IHttpContextAccessor httpContextAccessor)
    {
        var claim = httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return claim != null && int.TryParse(claim, out var userId) ? userId : null;
    }

    public static async Task<bool> CategoryBelongsToUserAsync(
        int? categoryId,
        IHttpContextAccessor httpContextAccessor,
        ICategoryRepository categoryRepository,
        CancellationToken cancellationToken)
    {
        if (!categoryId.HasValue)
            return true;

        var userId = GetUserId(httpContextAccessor);
        if (userId == null)
            return false;

        var category = await categoryRepository.GetByIdAsync(userId.Value, categoryId.Value);
        return category != null;
    }
}
