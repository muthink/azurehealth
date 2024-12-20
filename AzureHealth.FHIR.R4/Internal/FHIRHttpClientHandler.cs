using System.Diagnostics;
using System.Net;
using System.Net.Http.Json;
using System.Security.Authentication;
using System.Text.Json;
using Hl7.Fhir.Rest;
using Microsoft.Extensions.Logging;
using static Muthink.AzureHealth.FHIR.R4.BearerToken;
using Task = System.Threading.Tasks.Task;

namespace Muthink.AzureHealth.FHIR.R4;

/// <summary>Authorization message handler wrapper to use OAuth2 settings</summary>
/// <remarks>We only need one of these in the system.</remarks>
internal class FHIRHttpClientHandler : HttpClientHandler
{
    private static readonly JsonSerializerOptions _jsonSerializerOptions = new();
    private readonly FHIRClientCredentials _credentials;

    // In case we need to get the token endpoint
    private readonly FHIREndpoints _endpoints;
    private readonly ILogger _log;
    private readonly SemaphoreSlim _semaphore = new(1, 1);
    private readonly FhirClient _publicClient;

    /// <summary>
    ///     Creates a <see cref="HttpClientHandler" /> that can optionally provide scopes
    ///     and authenticates.
    /// </summary>
    /// <param name="endpoints">Token endpoints</param>
    /// <param name="credentials">How to connect to the FHIR server and get the bearer token.</param>
    /// <param name="log">Logger</param>
    public FHIRHttpClientHandler(FHIRClientCredentials credentials,
        FHIREndpoints endpoints, ILogger log)
    {
        _endpoints = endpoints;
        _credentials = credentials;
        _log = log;
        _publicClient = new FhirClient(credentials.FhirUri);
    }

    /// <summary>
    ///     Generates a new Bearer token from the credentials provided.
    /// </summary>
    /// <param name="cancelToken"></param>
    /// <param name="httpClient"></param>
    /// <returns></returns>
    /// <exception cref="IOException"></exception>
    /// <exception cref="AuthenticationException"></exception>
    public async Task<BearerToken> GenerateBearerTokenAsync(
        CancellationToken cancelToken, HttpClient? httpClient = null)
    {
        // var fhirUrl = settings.FhirServer;
        var clientSecret = _credentials.ClientSecret;
        var client = httpClient ?? new HttpClient();

        var scopeUri = new Uri(_credentials.Scopes);

        var resourceUri = scopeUri.GetLeftPart(UriPartial.Authority);
        // First authorize any request
        var formUrlContent = new List<KeyValuePair<string, string>>
        {
            new("grant_type", "client_credentials"),
            new("client_id", _credentials.ClientId),
            new("client_secret", clientSecret),
            new("resource", resourceUri),
            new("x-bundle-processing-logic", "parallel"),
            new("x-ms-profile-validation", "true")
            //new("scope", uri.PathAndQuery)
        };

        var preAuthorization = new HttpRequestMessage(HttpMethod.Post, _endpoints.Token)
        {
            Content = new FormUrlEncodedContent(formUrlContent)
        };

        try
        {
            var authorizationResult = await client.SendAsync(preAuthorization, cancelToken);

            var response = await authorizationResult.Content.ReadFromJsonAsync<GetAuthorizationResponse>(
                               _jsonSerializerOptions, cancelToken) ??
                           throw new IOException("Could not deserialize Json with token");

            var token = response.Token
                        ?? throw new AuthenticationException(
                            $"Response failed to obtain token (credentials or header was likely bad): Result = {authorizationResult.StatusCode}: {authorizationResult.ReasonPhrase}, {response.Error}");

            return new BearerToken(token);
        }
        catch(Exception exception) when(exception is not AuthenticationException)
        {
            var headers = string.Join(',', formUrlContent.Select(kvp => $"{kvp.Key}={kvp.Value}"));
            throw new AuthenticationException(
                $"Failed to generate bearer token for {_credentials} with header: {headers}", exception);
        }
    }

    private async Task<BearerToken> GetValidBearerToken(CancellationToken cancellationToken)
    {
        await _semaphore.WaitAsync(cancellationToken);
        try
        {
            var token = Token;

            if( token is not null )
            {
                if( !token.IsTokenExpired() )
                {
                    return token;
                }

                _log.LogDebug("Token has expired, {token}...", token.Token.Truncate(50));
            }

            _log.LogDebug("Creating new bearer token from: {uri}", _credentials.OAuthTokenUrl);
            var bearerToken = await GenerateBearerTokenAsync(cancellationToken);
            Token = bearerToken;
            _log.LogDebug("Scope: {scope}, Token: {token}", _credentials.Scopes, bearerToken.Token);

            return bearerToken;
        }
        finally
        {
            _semaphore.Release();
        }
    }

    protected override HttpResponseMessage Send(HttpRequestMessage request, CancellationToken cancellationToken) =>
        SendAsync(request, cancellationToken).GetAwaiter().GetResult();

    /// <inheritdoc cref="HttpClientHandler.SendAsync(HttpRequestMessage,CancellationToken)" />
    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var token = await GetValidBearerToken(cancellationToken);
        request.Headers.Authorization = token.GetHeaderValue();
        Debug.WriteLine($"Request {request.Method} {request.RequestUri}");
        try
        {
            var response = await base.SendAsync(request, cancellationToken);
            return response;
        }
        catch(OperationCanceledException)
        {
            _log.LogInformation("Request {method} cancelled to {uri}", request.Method, request.RequestUri);
            throw;
        }
        catch(HttpRequestException ex)
        {
            _log.LogWarning(ex, "Http Request to {uri} threw HttpRequestException {m}", request.RequestUri, ex.Message);
            if( ex.StatusCode == HttpStatusCode.Unauthorized )
            {
                Token = null;
            }

            throw;
        }
        catch(Exception ex2)
        {
            _log.LogWarning(ex2, "Http Request to {uri} threw exception {m}", request.RequestUri, ex2.Message);
            throw;
        }
    }

    private BearerToken? Token { get; set; }

}