using Hl7.Fhir.Model;
using Hl7.Fhir.Rest;
using Microsoft.Extensions.Logging;
using Task = System.Threading.Tasks.Task;

namespace Muthink.AzureHealth.FHIR.R4;

internal class FHIRTokenEndpointRefresher : IFHIRTokenEndpointFactory
{
    private CapabilityStatement? _capabilityStatement;

    /// <summary>
    ///     Reads and caches the FHIR capability statement for the server, upon success.
    /// </summary>
    /// <returns></returns>
    public async Task<CapabilityStatement> GetCapabilityStatementAsync(FhirClient fhirClient,
        CancellationToken cancelToken) =>
        _capabilityStatement ??= await fhirClient.CapabilityStatementAsync(SummaryType.False)
                                 ?? throw new IOException(
                                     $"Could not get capability statement for {fhirClient}");

    /// <summary>
    ///     Parses out the token URLs from the capability statement.
    /// </summary>
    /// <param name="fhirUri"></param>
    /// <param name="capabilities"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException" />
    private FHIREndpoints RefreshFromCapabilityStatement(Uri fhirUri, CapabilityStatement capabilities)
    {
        // Extract the end point for token authorization
        var securityComponents = capabilities.Rest.Select(r => r.Security).First();

        // Under the Security is the oauth uris extension
        var oauthUris = securityComponents.Extension
            .First(ex => ex.Url?.EndsWith("oauth-uris", StringComparison.OrdinalIgnoreCase) ?? false);

        var tokenString = oauthUris.GetExtensionValue<FhirUri>("token").Value
                          ?? throw new InvalidOperationException(
                              $"We can't access the FHIR server {capabilities.Url}, since it has no token endpoint");

        var revokeString = oauthUris.GetExtensionValue<FhirUri>("revoke")?.Value;

        var introspectString = oauthUris.GetExtensionValue<FhirUri>("introspect")?.Value;

        var authorizeString = oauthUris.GetExtensionValue<FhirUri>("authorize")?.Value;

        var endpoints = new FHIREndpoints(fhirUri, new Uri(tokenString),
            revokeString is null ? null : new Uri(revokeString),
            introspectString is null ? null : new Uri(introspectString),
            authorizeString is null ? null : new Uri(authorizeString)
            );

        return endpoints;
    }


    /// <summary>
    ///     Get the token endpoint of the FHIR server (not necessarily the same as the SMART-on-FHIR endpoint.)
    /// </summary>
    /// <param name="fhirUri"></param>
    /// <param name="cancelToken"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public async Task<FHIREndpoints> GetFHIREndpointsAsync(Uri fhirUri, CancellationToken cancelToken)
    {
        var publicClient = new FhirClient(fhirUri);
        var capabilities = await GetCapabilityStatementAsync(publicClient, cancelToken);

        cancelToken.ThrowIfCancellationRequested();

        var endpoints = RefreshFromCapabilityStatement(fhirUri, capabilities);
        return endpoints;
    }


    public async Task ValidateCredentialsAsync(FHIRClientCredentials credentials,
        FHIREndpoints endpoints, ILogger logger, CancellationToken cancelToken = default)
    {
        var handler = new FHIRHttpClientHandler(credentials, endpoints, logger);

        // Attempt to obtain bearer token.
        BearerToken? bearerToken;
        try
        {
            bearerToken = await handler.GenerateBearerTokenAsync(cancelToken);
        }
        catch (Exception ex)
        {
            throw new AzureHealthException($"Cannot continue for {credentials.Application}: failed to obtained token = {ex.Message}", ex);
        }


        if (bearerToken is null)
        {
            throw new AzureHealthException($"Bearer token is null for {credentials.Application}");
        }

        logger.LogInformation($"Successfully obtained bearer token for {credentials.Application} = {bearerToken.Token}");
    }
}