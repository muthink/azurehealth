using Hl7.Fhir.Rest;

namespace Muthink.AzureHealth.FHIR.R4;

/// <summary>
/// FHIR client, which wraps <see cref="FhirClient"/>, for the additional Azure endpoints.
/// </summary>
public class AzureFhirClient : FhirClient
{
    public AzureFhirClient(Uri endpoint, FhirClientSettings? settings = null, HttpMessageHandler? messageHandler = null)
        : base(endpoint, settings, messageHandler)
    {
    }

    public AzureFhirClient(Uri endpoint, HttpClient httpClient, FhirClientSettings? settings = null)
        : base(endpoint, httpClient, settings)
    {
    }

    public AzureFhirClient(string endpoint, FhirClientSettings? settings = null, HttpMessageHandler? messageHandler = null)
        : base(endpoint, settings, messageHandler)
    {
    }

    public AzureFhirClient(string endpoint, HttpClient httpClient, FhirClientSettings? settings = null)
        : base(endpoint, httpClient, settings)
    {
    }


}
