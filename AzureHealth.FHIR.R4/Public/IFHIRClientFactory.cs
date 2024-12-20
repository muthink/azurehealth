
using Microsoft.Extensions.Logging;

namespace Muthink.AzureHealth.FHIR.R4;

/// <summary>
/// Provides a FHIR client factory
/// </summary>
public interface IFHIRClientFactory
{
    /// <summary>
    /// Creates a public FHIR client endpoint.
    /// </summary>
    /// <param name="credentials"></param>
    /// <param name="endpoints"></param>
    /// <returns>An authorized client.</returns>
    AzureFhirClient CreateAuthFhirClient(FHIRClientCredentials credentials, FHIREndpoints endpoints);

    /// <summary>
    /// Validate the credentials against the given FHIR server.
    /// </summary>
    /// <param name="credentials"></param>
    /// <param name="endpoints"></param>
    /// <param name="logger"></param>
    /// <param name="cancelToken"></param>
    /// <returns></returns>
    Task ValidateCredentialsAsync(FHIRClientCredentials credentials,
        FHIREndpoints endpoints, ILogger logger, CancellationToken cancelToken = default);
}
