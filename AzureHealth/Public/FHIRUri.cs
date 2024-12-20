namespace Muthink.AzureHealth;

/// <summary>
/// The full public endpoint of the FHIR Server
/// </summary>
/// <param name="uri"></param>
public class FHIRUri(Uri uri) : Uri(uri.ToString())
{
    /// <summary>
    ///     Well-known SMART OAuth public configuration <see cref="Uri"/>
    /// </summary>
    /// <returns>Url = [FHIR server URL]/.well-known/smart-configuration</returns>
    public Uri GetSmartWellKnownConfigurationUri(Uri fhirUri) =>
        new(fhirUri,
            new Uri(".well-known/smart-configuration", UriKind.Relative));
}
