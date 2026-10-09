namespace RAGA.Application.Common.Constants;

public static class AppClaimTypes
{
    public const string ObjectId =
        "http://schemas.microsoft.com/identity/claims/objectidentifier";

    public const string TenantId =
        "http://schemas.microsoft.com/identity/claims/tenantid";

    // Standard OIDC claims that are not remapped by the default
    // inbound claim type map (given_name/family_name are remapped
    // to ClaimTypes.GivenName/ClaimTypes.Surname, so both forms are checked).
    public const string Name = "name";

    public const string GivenName = "given_name";

    public const string FamilyName = "family_name";

    public const string PreferredUsername = "preferred_username";
}