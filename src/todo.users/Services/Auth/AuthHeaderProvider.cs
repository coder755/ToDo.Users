using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace todo.users.Services.Auth;

public class AuthHeaderProvider : ControllerBase, IAuthHeaderProvider
{
    private readonly IHttpContextAccessor _contextAccessor;
    private const string BearerTokenPrefix = "Bearer ";
    
    public AuthHeaderProvider(IHttpContextAccessor contextAccessor)
    {
        _contextAccessor = contextAccessor;
    }

    private string GetAuthHeader()
    {
        return _contextAccessor.HttpContext?.Request.Headers["Authorization"].ToString();
    }
    
    public Guid GetUserId()
    {
        var authHeader = GetAuthHeader();
        if (authHeader != null && !string.IsNullOrWhiteSpace(authHeader) & authHeader.Contains(JwtBearerDefaults.AuthenticationScheme,
                StringComparison.OrdinalIgnoreCase))
        {
            var identityToken = authHeader.Trim().Substring(JwtBearerDefaults.AuthenticationScheme.Length).Trim();
            var jwt = new JwtSecurityTokenHandler().ReadJwtToken(identityToken);
            var guidStr = jwt.Claims.First(claim => claim.Type == "sub").Value;
            return Guid.Parse(guidStr);
        }

        return Guid.Empty;
    }

    public string GetAuthHeaderValue()
    {
        if (_contextAccessor.HttpContext == null) return "";
        var authHeaderValue = _contextAccessor.HttpContext.Request.Headers.Authorization.ToString();
        var token = authHeaderValue.Remove(0, BearerTokenPrefix.Length - 1);
        
        return token;

    }
}