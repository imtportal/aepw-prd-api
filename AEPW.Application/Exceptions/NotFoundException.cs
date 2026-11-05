namespace AEPW.Application.Exceptions;

public sealed class NotFoundException : Exception
{
    public string ResourceName { get; }
    public object ResourceKey  { get; }

    public NotFoundException(string resourceName, object resourceKey)
        : base($"{resourceName} with identifier '{resourceKey}' was not found.")
    {
        ResourceName = resourceName;
        ResourceKey  = resourceKey;
    }

    /// <summary>Use when a pre-formatted message reads better than the resource/key template above.</summary>
    public NotFoundException(string message)
        : base(message)
    {
        ResourceName = string.Empty;
        ResourceKey  = string.Empty;
    }
}
