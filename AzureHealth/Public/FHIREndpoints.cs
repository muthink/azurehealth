namespace Muthink.AzureHealth;

/// <summary>
///     A <see cref="Uri" />, which contains the FHIR token endpoint
/// </summary>
/// <param name="Public">The public FHIR Uri</param>
/// <param name="Token">The public Token Uri</param>
/// <param name="Revoke">Optional Revoke Uri</param>
/// <param name="Introspect">Optional Introspection Uri</param>
/// <param name="Authorize">Optional Authorize Uri</param>
public record FHIREndpoints(
    Uri Public,
    Uri Token,
    Uri? Revoke = null,
    Uri? Introspect = null,
    Uri? Authorize = null);