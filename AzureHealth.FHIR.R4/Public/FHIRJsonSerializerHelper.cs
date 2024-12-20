using System.Text;
using System.Text.Json;
using Hl7.Fhir.Model;
using Hl7.Fhir.Serialization;

namespace Muthink.AzureHealth.FHIR.R4;

/// <summary>
///     New-line delimited Json serialize helper
/// </summary>
public static class FHIRJsonSerializerHelper
{
    private static readonly FhirJsonPocoDeserializer _deserializer = new();
    private static readonly FhirJsonSerializer _serializerNonPretty = new(new SerializerSettings { Pretty = false });
    private static readonly FhirJsonSerializer _serializerPretty = new(new SerializerSettings { Pretty = true });

    /// <summary>
    ///     Deserialize to resource
    /// </summary>
    /// <typeparam name="TResource">The type of resource</typeparam>
    /// <param name="json">JSON string</param>
    /// <returns>
    ///     <typeparamref name="TResource" />
    /// </returns>
    public static TResource DeserializeToResource<TResource>(string json)
        where TResource : Base
    {
        var asUtf8 = Encoding.UTF8.GetBytes(json);
        var reader = new Utf8JsonReader(asUtf8);
        return _deserializer.DeserializeObject<TResource>(ref reader);
    }

    /// <summary>
    ///     Serializes bundles into a <see cref="StreamWriter" />
    /// </summary>
    /// <param name="bundle">The bundle to serialize</param>
    /// <param name="streamWriter">Where to output the stream data</param>
    public static void SerializeNDJson(this Bundle bundle, StreamWriter streamWriter)
    {
        foreach(var entry in bundle.Entry)
        {
            var line = _serializerNonPretty.SerializeToString(entry.Resource);
            streamWriter.WriteLine(line);
        }
    }

    /// <summary>
    ///     Serialize to string.
    /// </summary>
    /// <param name="resource"></param>
    /// <param name="isPretty"></param>
    /// <returns></returns>
    public static string SerializeToString(this Resource resource, bool isPretty = false) =>
        isPretty
            ? _serializerPretty.SerializeToString(resource)
            : _serializerNonPretty.SerializeToString(resource);
}