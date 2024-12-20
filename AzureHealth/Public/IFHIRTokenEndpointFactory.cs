
namespace Muthink.AzureHealth;

/// <summary>
/// Provides a FHIR client factory
/// </summary>
public interface IFHIRTokenEndpointFactory
{
    /// <summary>
    /// Queries the server to find out which endpoints are available.
    /// </summary>
    /// <param name="fhirUri">The existing FHIR server</param>
    /// <param name="cancelToken">Allows caller to cancel before completion</param>
    /// <returns>A refresh set of FHIR endpoints</returns>
    Task<FHIREndpoints> GetFHIREndpointsAsync(Uri fhirUri, CancellationToken cancelToken);
}
