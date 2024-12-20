namespace Muthink.AzureHealth;

/// <summary>
/// Exception from the AzureHealth sub-system.
/// </summary>
[Serializable]
public class AzureHealthException : SystemException
{
    public AzureHealthException(string message) : base(message)
    {
    }

    public AzureHealthException(string message, Exception innerException) : base(message, innerException)
    {

    }
}
