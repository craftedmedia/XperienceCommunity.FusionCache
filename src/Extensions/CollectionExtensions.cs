namespace XperienceCommunity.FusionCache.Extensions;

/// <summary>
/// Collection extension methods.
/// </summary>
internal static class CollectionExtensions
{
    /// <summary>
    /// Adds one dictionary to another.
    /// </summary>
    /// <typeparam name="TKey">Key type.</typeparam>
    /// <typeparam name="TValue">Value type.</typeparam>
    /// <param name="source">Source dictionary.</param>
    /// <param name="other">Dictionary to add.</param>
    /// <exception cref="ArgumentNullException"></exception>
    public static void AddRange<TKey, TValue>(this IDictionary<TKey, TValue> source, IDictionary<TKey, TValue> other)
    {
        if (other is null)
        {
            throw new ArgumentNullException(nameof(other));
        }

        foreach (var item in other)
        {
            source.TryAdd(item.Key, item.Value);
        }
    }
}
