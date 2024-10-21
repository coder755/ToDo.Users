using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using todo.users.model.Auth;

namespace todo.users.Services.Auth.User;

public class AuthUserProvider: IAuthUserProvider
{
    private static readonly JwkCache Cache = new ();
    private readonly CognitoSettings _cognitoSettings;

    public AuthUserProvider(IOptions<CognitoSettings> cognitoSettings)
    {
        _cognitoSettings = cognitoSettings.Value;
    }

    private async Task<TokenValidationParameters> GetTokenValidationParametersAsync()
    {
        var keys = await Cache.GetJwkAsync(_cognitoSettings.Authority);
        return new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = _cognitoSettings.Authority,
            ValidateAudience = true,
            ValidAudience = _cognitoSettings.AppClientId,
            ValidateLifetime = true,
            IssuerSigningKeys = keys,
            ValidateIssuerSigningKey = true
        };
    }

    public async Task ValidateTokenAsync(string token)
    {
        var tokenValidationParameters = await GetTokenValidationParametersAsync();
        var tokenHandler = new JwtSecurityTokenHandler();
        tokenHandler.ValidateToken(token, tokenValidationParameters, out _);
    }

    private static string GetClaimFromToken(string claimId, string token)
    {
        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);
        var claimStr = jwt.Claims.First(claim => claim.Type == claimId).Value;
        return claimStr;
    }
    
    public Guid GetUserId(string token)
    {
        var guidStr = GetClaimFromToken("sub", token);
        var isValidGuid = Guid.TryParse(guidStr, out var userId);
        if (isValidGuid)
        {
            return userId;
        }

        throw new UnauthorizedAccessException();
    }
    
    private class JwkCache
    {
        private List<RsaSecurityKey> _cachedKeys;
        private DateTime _lastFetchTime;
        private static readonly TimeSpan CacheDuration = TimeSpan.FromHours(24);
        private static readonly HttpClient HttpClient = new();

        public async Task<List<RsaSecurityKey>> GetJwkAsync(string authority)
        {
            if (_cachedKeys == null || DateTime.UtcNow - _lastFetchTime > CacheDuration)
            {
                var jwksUri = $"{authority}/.well-known/jwks.json";
                var response = await HttpClient.GetStringAsync(jwksUri);
                var jwks = new JsonWebKeySet(response).Keys; 

                _lastFetchTime = DateTime.UtcNow;
                _cachedKeys = new List<RsaSecurityKey>();
                foreach (var key in jwks)
                {
                    var rsaSecurityKey = CreateRsaSecurityKey(key);
                    _cachedKeys.Add(rsaSecurityKey);
                }
            }

            return _cachedKeys;
        }

        private static RsaSecurityKey CreateRsaSecurityKey(JsonWebKey key)
        {
            var exponent = Base64UrlEncoder.DecodeBytes(key.E);
            var modulus = Base64UrlEncoder.DecodeBytes(key.N);

            var rsa = new RSACryptoServiceProvider();
            rsa.ImportParameters(new RSAParameters
            {
                Exponent = exponent,
                Modulus = modulus
            });

            return new RsaSecurityKey(rsa)
            {
                KeyId = key.KeyId
            };
        }
    }
}