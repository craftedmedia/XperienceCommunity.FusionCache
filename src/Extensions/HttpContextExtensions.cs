using Microsoft.AspNetCore.Http;

namespace XperienceCommunity.FusionCache.Caching.Extensions;

/// <summary>
/// Http Context extensions.
/// </summary>
public static class HttpContextExtensions
{
    private const string DummyKeysCtx = "XperienceCommunity.FusionCache.DummyKeys.Ctx";

    /// <summary>
    /// Adds dummy dependency keys to the current <see cref="HttpContext"/>.
    /// </summary>
    /// <param name="httpContext">Current context.</param>
    /// <param name="keys">Collection of keys associated with the request.</param>
    public static void AddCacheDependencies(this HttpContext httpContext, HashSet<string> keys)
    {
        if (keys is null || !keys.Any())
        {
            return;
        }

        if (!httpContext.Items.TryAdd(DummyKeysCtx, keys))
        {
            if (httpContext.Items[DummyKeysCtx] is not HashSet<string> currentKeys)
            {
                currentKeys = new HashSet<string>(keys);
            }
            else
            {
                currentKeys.UnionWith(keys);
            }

            httpContext.Items[DummyKeysCtx] = currentKeys;
        }
    }

    /// <summary>
    /// Gets dummy dependency keys from the current <see cref="HttpContext"/>.
    /// </summary>
    /// <param name="httpContext">Current context.</param>
    /// <returns>Collection of dummy keys associated with the request.</returns>
    internal static HashSet<string> GetContextDummyKeys(this HttpContext httpContext)
    {
        if (httpContext.Items.TryGetValue(DummyKeysCtx, out object? result) && result is HashSet<string> keys)
        {
            return keys;
        }

        return [];
    }
}
