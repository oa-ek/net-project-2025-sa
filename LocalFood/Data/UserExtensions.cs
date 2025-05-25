// Extensions/UserExtensions.cs
using System.Security.Claims;

namespace LocalFood.Extensions
{
    public static class UserExtensions
    {
        public static string GetUserId(this ClaimsPrincipal user)
            => user.FindFirstValue(ClaimTypes.NameIdentifier)!;
    }
}
