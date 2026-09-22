using RAGA.Application.Common.Constants;
using System;
using System.Security.Claims;

namespace RAGA.Application.Common.Extensions
{
    public static class ClaimsPrincipalExtensions
    {
        public static string GetUserObjectId(this ClaimsPrincipal user)
        {
            return user.FindFirst(AppClaimTypes.ObjectId)?.Value
                   ?? throw new InvalidOperationException(
                       "User Object ID claim was not found.");
        }
    }
}