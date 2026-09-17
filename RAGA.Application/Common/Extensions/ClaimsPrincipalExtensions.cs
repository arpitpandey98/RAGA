using System;
using System.Security.Claims;


namespace RAGA.Extensions
{
    public static class ClaimsPrincipalExtensions
    {
        public static string GetUserObjectId(this ClaimsPrincipal user)
        {
            return user.FindFirst("http://schemas.microsoft.com/identity/claims/objectidentifier")?.Value       
                   ?? throw new InvalidOperationException(
                       "User Object ID claim was not found.");
        }
    }
}