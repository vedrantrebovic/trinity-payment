using System.Security.Claims;
using OpenIddict.Abstractions;

namespace Trinity.PaymentProvider.API.Shared.Extensions;

public static class PrincipalExtensions
{
    private static string GetClaim(this System.Security.Principal.IPrincipal user, string claim,
        string defaultValue = null)
    {
        ClaimsIdentity claims = (ClaimsIdentity)user.Identity;
        if (claims != null && claims.HasClaim(p => p.Type == claim))
            return claims.Claims.First(p => p.Type == claim).Value;
        return defaultValue;
    }

    public static bool IsNotInRole(this System.Security.Principal.IPrincipal user, string role)
    {
        return !user.IsInRole(role);
    }

    public static IList<string> GetRoles(this System.Security.Principal.IPrincipal user)
    {
        IList<string> result = new List<string>();

        if (user.Identity is ClaimsIdentity claimsIdentity)
        {
            foreach (var claim in claimsIdentity.GetClaims(ClaimTypes.Role))
            {
                if(!result.Contains(claim))
                    result.Add(claim);
            }

            foreach (var claim in claimsIdentity.GetClaims(OpenIddictConstants.Claims.Role))
            {
                if (!result.Contains(claim))
                    result.Add(claim);
            }
        }
        
        return result;
    }
}

