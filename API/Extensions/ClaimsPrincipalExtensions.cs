using System.Security.Claims;


namespace API.Extensions
{
    public static class ClaimsPrincipalExtensions
    {
        public static string GetEmail(this ClaimsPrincipal User) => User.FindFirst(ClaimTypes.Name)?.Value;
        public static string GetUserId(this ClaimsPrincipal User) => User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    }
}
