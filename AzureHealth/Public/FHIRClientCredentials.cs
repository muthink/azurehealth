namespace Muthink.AzureHealth;

/// <summary>
///     The FHIR Client arguments required for an authenticated FHIR client,
///     which uses OAuth to authenticate.
/// </summary>
/// <param name="Application">The unique name of this client</param>
/// <param name="ClientId">The client id, which is a GUID for microsoft</param>
/// <param name="ClientSecret">The client secret</param>
/// <param name="FhirUri">The FHIR URL</param>
/// <param name="Scopes">Scopes used when authenticating this</param>
/// <param name="HeaderValues">Optional Header values</param>
public record FHIRClientCredentials(string Application, string ClientId,
    string ClientSecret, Uri FhirUri, string Scopes, HttpHeaderNameValue[]? HeaderValues)
{
    public override int GetHashCode()
    {
        var hashCode = new HashCode();
        hashCode.Add(Application, StringComparer.OrdinalIgnoreCase);
        hashCode.Add(ClientId, StringComparer.OrdinalIgnoreCase);
        hashCode.Add(ClientSecret, StringComparer.OrdinalIgnoreCase);
        hashCode.Add(FhirUri);
        hashCode.Add(Scopes, StringComparer.OrdinalIgnoreCase);
        if( HeaderValues is not null && HeaderValues.Length > 0)
        {
            foreach(var nameValuePair in HeaderValues)
            {
                hashCode.Add(nameValuePair);
            }
        }
        return hashCode.ToHashCode();
    }

    /// <summary>
    ///     The OAuth token URL. If not provided, will obtain it from
    ///     FHIR server.
    /// </summary>
    public Uri? OAuthTokenUrl { get; set; }

    public virtual bool Equals(FHIRClientCredentials? other)
    {
        if (ReferenceEquals(null, other))
        {
            return false;
        }

        if (ReferenceEquals(this, other))
        {
            return true;
        }

        return string.Equals(Application, other.Application, StringComparison.OrdinalIgnoreCase)
               && string.Equals(ClientId, other.ClientId, StringComparison.OrdinalIgnoreCase)
               && string.Equals(ClientSecret, other.ClientSecret, StringComparison.OrdinalIgnoreCase)
               && FhirUri.Equals(other.FhirUri)
               && string.Equals(Scopes, other.Scopes, StringComparison.OrdinalIgnoreCase);
    }
}