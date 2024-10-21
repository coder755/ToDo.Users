using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace todo.users.Services.Auth.Header;

public class AuthHeaderProvider : ControllerBase, IAuthHeaderProvider
{
    private readonly IHttpContextAccessor _contextAccessor;
    
    public AuthHeaderProvider(IHttpContextAccessor contextAccessor)
    {
        _contextAccessor = contextAccessor;
    }

    public Guid GetUserId()
    {
        var guidStr = GetClaimFromBearerToken("sub");
        var isValidGuid = Guid.TryParse(guidStr, out var userId);
        return isValidGuid ? userId: Guid.Empty;
    }

    public string GetIdentityToken()
    {
        var authHeader = GetAuthHeader();
        if (string.IsNullOrWhiteSpace(authHeader) || !authHeader.Contains(JwtBearerDefaults.AuthenticationScheme,
                StringComparison.OrdinalIgnoreCase)) return "";
        var identityToken = authHeader.Trim().Substring(JwtBearerDefaults.AuthenticationScheme.Length).Trim();
        return identityToken;
    }
    
    private string GetClaimFromBearerToken(string claimId)
    {
        var identityToken = GetIdentityToken();
        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(identityToken);
        var claimStr = jwt.Claims.First(claim => claim.Type == claimId).Value;
        return claimStr;
    }
    
    private string GetAuthHeader()
    {
        return _contextAccessor.HttpContext?.Request.Headers["Authorization"].ToString();
    }
}