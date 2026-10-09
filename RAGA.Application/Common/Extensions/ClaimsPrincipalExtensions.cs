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

        public static string GetTenantId(this ClaimsPrincipal user)
        {
            return user.FindFirst(AppClaimTypes.TenantId)?.Value
                   ?? throw new InvalidOperationException(
                       "Tenant ID claim was not found.");
        }

        /// <summary>
        /// Returns the authenticated person's display name from the
        /// validated token claims, or null if none is available.
        /// Order: name, given_name + family_name, preferred_username.
        /// </summary>
        public static string? GetUserDisplayName(this ClaimsPrincipal user)
        {
            var name = GetClaimValue(user, AppClaimTypes.Name);

            if (name != null)
            {
                return name;
            }

            var givenName = GetClaimValue(
                user, AppClaimTypes.GivenName, ClaimTypes.GivenName);

            var familyName = GetClaimValue(
                user, AppClaimTypes.FamilyName, ClaimTypes.Surname);

            var fullName = string.Join(
                " ",
                new[] { givenName, familyName }.Where(x => x != null));

            if (fullName.Length > 0)
            {
                return fullName;
            }

            return GetClaimValue(user, AppClaimTypes.PreferredUsername);
        }

        private static string? GetClaimValue(
            ClaimsPrincipal user,
            params string[] claimTypes)
        {
            foreach (var claimType in claimTypes)
            {
                var value = user.FindFirst(claimType)?.Value?.Trim();

                // Entra External ID emits "unknown" when the user
                // has no display name set.
                if (!string.IsNullOrEmpty(value) &&
                    !string.Equals(value, "unknown", StringComparison.OrdinalIgnoreCase))
                {
                    return value;
                }
            }

            return null;
        }
    }
}