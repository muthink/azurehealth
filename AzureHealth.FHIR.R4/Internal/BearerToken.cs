using System.Net.Http.Headers;
using System.Text.Json.Serialization;
using Microsoft.IdentityModel.JsonWebTokens;

namespace Muthink.AzureHealth.FHIR.R4;

/// <summary>
///     Token combined with expiration
/// </summary>
/// <param name="Token">The bearer token</param>
public record BearerToken(string Token)
{
    private JsonWebToken? _jwtSecurityToken;

    /// <summary>
    ///     Get for appending to requests
    /// </summary>
    /// <returns>The header value</returns>
    public AuthenticationHeaderValue GetHeaderValue()
        => new("Bearer", Token);

    public bool IsTokenExpired()
    {
        var token = JwtToken;
        if( token == null )
        {
            return true;
        }

        var tokenExpiryDate = token.ValidTo;

        // If there is no valid `exp` claim then `ValidTo` returns DateTime.MinValue
        if( tokenExpiryDate == DateTime.MinValue )
        {
            return true;
        }

        // If the token is in the past then you can't use it
        if( tokenExpiryDate < DateTime.UtcNow )
        {
            return true;
        }

        return false;
    }

    /// <summary>
    ///     Show this token as a JwtToken
    /// </summary>
    public JsonWebToken? JwtToken
    {
        get
        {
            if( _jwtSecurityToken is null )
            {
                var handler = new JsonWebTokenHandler();
                var token = handler.ReadToken(Token) as JsonWebToken;
                _jwtSecurityToken = token;
                return token;
            }

            return _jwtSecurityToken;
        }
    }


    internal class GetAuthorizationResponse
    {
        /// <summary>
        ///     If an error occurred, the explanation is here.
        /// </summary>
        [JsonPropertyName("error")]
        public string? Error { get; set; }

        /// <summary>
        /// Token, a string, returned
        /// </summary>
        [JsonPropertyName("access_token")] public string? Token { get; set; }
    }
}