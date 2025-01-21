namespace XperienceCommunity.FusionCache.OutputCache;

/// <summary>
/// Allows specifying of custom vary by option types.
/// </summary>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = false)]
public class XperienceFusionCacheVaryByOptionTypesAttribute : Attribute
{
    /// <summary>
    /// Gets or sets the vary by option types for the request.
    /// </summary>
    public Type[]? VaryByOptionTypes { get; set; }
}
