using System.Security.Claims;


namespace API.Extensions
{
    public static class ClaimsPrincipalExtensions
    {
        public static string GetEmail(this ClaimsPrincipal User) => User.FindFirst(ClaimTypes.Name)?.Value;
        public static int GetUserId(this ClaimsPrincipal User) => int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
    }
}
