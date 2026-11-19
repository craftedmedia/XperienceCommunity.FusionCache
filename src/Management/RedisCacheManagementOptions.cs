namespace XperienceCommunity.FusionCache.Management;

/// <summary>Options for managing the Redis database used by FusionCache.</summary>
public sealed class RedisCacheManagementOptions
{
    /// <summary>An optional friendly name shown in the administration.</summary>
    public string DisplayName { get; set; } = "Redis cache";

    /// <summary>Exact, case-sensitive keys excluded from direct Redis deletion. Global FusionCache invalidation still applies.</summary>
    public string[] ExcludedKeys { get; set; } = [];

    /// <summary>Case-sensitive exclusions from direct Redis deletion. '*' matches any characters; '?' matches one character.</summary>
    public string[] ExcludedPatterns { get; set; } = [];

    /// <summary>Maximum duration of a purge before it stops issuing more commands.</summary>
    public TimeSpan PurgeTimeout { get; set; } = TimeSpan.FromMinutes(2);
}
