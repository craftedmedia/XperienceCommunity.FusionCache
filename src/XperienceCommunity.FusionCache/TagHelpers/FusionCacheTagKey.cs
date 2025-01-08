using System.Globalization;
using System.Security.Cryptography;
using System.Text;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.TagHelpers.Cache;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Primitives;

using XperienceCommunity.FusionCache.Caching.TagHelpers;

namespace XperienceCommunity.FusionCache.TagHelpers;
/// <summary>
/// Cache tag key, modified and based on the <see cref="CacheTagKey"/> type.
/// </summary>
public class FusionCacheTagKey : IEquatable<FusionCacheTagKey>
{
    private const string CacheKeyTokenSeparator = "||";
    private const string VaryByName = "VaryBy";
    private const string VaryByHeaderName = "VaryByHeader";
    private const string VaryByQueryName = "VaryByQuery";
    private const string VaryByRouteName = "VaryByRoute";
    private const string VaryByCookieName = "VaryByCookie";
    private const string VaryByUserName = "VaryByUser";
    private const string VaryByCulture = "VaryByCulture";

    private static readonly char[] attributeSeparator = new[] { ',' };
    private static readonly Func<IRequestCookieCollection, string, string> cookieAccessor = (c, key) => c[key]!;
    private static readonly Func<IHeaderDictionary, string, string> headerAccessor = (c, key) => c[key]!;
    private static readonly Func<IQueryCollection, string, string> queryAccessor = (c, key) => c[key]!;
    private static readonly Func<RouteValueDictionary, string, string> routeValueAccessor = (c, key) => Convert.ToString(c[key], CultureInfo.InvariantCulture)!;

    private readonly XperienceFusionCacheTagHelper tagHelper;
    private readonly TagHelperContext context;
    private readonly string prefix;
    private readonly string? varyBy;
    private readonly TimeSpan? duration;
    private readonly IList<KeyValuePair<string, string>> headers;
    private readonly IList<KeyValuePair<string, string>> queries;
    private readonly IList<KeyValuePair<string, string>> routeValues;
    private readonly IList<KeyValuePair<string, string>> cookies;
    private readonly bool varyByUser;
    private readonly bool varyByCulture;
    private readonly string username = string.Empty;
    private readonly CultureInfo? requestCulture;
    private readonly CultureInfo? requestUICulture;

    private string? generatedKey;
    private int? hashcode;

    /// <summary>
    /// Initializes a new instance of the <see cref="FusionCacheTagKey"/> class.
    /// </summary>
    /// <param name="tagHelper">Instance of the <see cref="XperienceFusionCacheTagHelper"/>.</param>
    /// <param name="context">Instance of the <see cref="TagHelperContext"/>.</param>
    public FusionCacheTagKey(XperienceFusionCacheTagHelper tagHelper, TagHelperContext context)
    {
        Key = tagHelper.Name;
        prefix = nameof(XperienceFusionCacheTagHelper);

        this.tagHelper = tagHelper;
        this.context = context;

        var httpContext = tagHelper.ViewContext.HttpContext;
        var request = httpContext.Request;

        duration = tagHelper.Duration;
        varyBy = tagHelper.VaryBy;
        cookies = ExtractCollection(tagHelper.VaryByCookie, request.Cookies, cookieAccessor);
        headers = ExtractCollection(tagHelper.VaryByHeader, request.Headers, headerAccessor);
        queries = ExtractCollection(tagHelper.VaryByQuery, request.Query, queryAccessor);
        routeValues = ExtractCollection(tagHelper.VaryByRoute, tagHelper.ViewContext.RouteData.Values, routeValueAccessor);
        varyByUser = tagHelper.VaryByUser;
        varyByCulture = tagHelper.VaryByCulture;

        if (varyByUser)
        {
            username = httpContext.User?.Identity?.Name ?? string.Empty;
        }

        if (varyByCulture)
        {
            requestCulture = CultureInfo.CurrentCulture;
            requestUICulture = CultureInfo.CurrentUICulture;
        }
    }

    /// <summary>
    /// Gets the cache tag key.
    /// </summary>
    internal string Key { get; }

    /// <summary>
    /// Creates a <see cref="string"/> representation of the key.
    /// </summary>
    /// <returns>A <see cref="string"/> uniquely representing the key.</returns>
    public string GenerateKey()
    {
        // Caching as the key is immutable and it can be called multiple times during a request.
        if (generatedKey != null)
        {
            return generatedKey;
        }

        var builder = new StringBuilder(prefix);
        builder
            .Append(CacheKeyTokenSeparator)
            .Append(Key);

        if (!string.IsNullOrEmpty(varyBy))
        {
            builder
                .Append(CacheKeyTokenSeparator)
                .Append(VaryByName)
                .Append(CacheKeyTokenSeparator)
                .Append(varyBy);
        }

        AddStringCollection(builder, VaryByCookieName, cookies);
        AddStringCollection(builder, VaryByHeaderName, headers);
        AddStringCollection(builder, VaryByQueryName, queries);
        AddStringCollection(builder, VaryByRouteName, routeValues);

        if (varyByUser)
        {
            builder
                .Append(CacheKeyTokenSeparator)
                .Append(VaryByUserName)
                .Append(CacheKeyTokenSeparator)
                .Append(username);
        }

        if (varyByCulture)
        {
            builder
                .Append(CacheKeyTokenSeparator)
                .Append(VaryByCulture)
                .Append(CacheKeyTokenSeparator)
                .Append(requestCulture)
                .Append(CacheKeyTokenSeparator)
                .Append(requestUICulture);
        }

        generatedKey = builder.ToString();

        return generatedKey;
    }

    /// <summary>
    /// Creates a hashed value of the key.
    /// </summary>
    /// <returns>A cryptographic hash of the key.</returns>
    public string GenerateHashedKey()
    {
        string key = GenerateKey();

        // The key is typically too long to be useful, so we use a cryptographic hash
        // as the actual key (better randomization and key distribution, so small vary
        // values will generate dramatically different keys).
        byte[] contentBytes = Encoding.UTF8.GetBytes(key);
        byte[] hashedBytes = SHA256.HashData(contentBytes);

        return Convert.ToBase64String(hashedBytes);
    }

    /// <inheritdoc />
    public override bool Equals(object? obj)
    {
        if (obj is FusionCacheTagKey other)
        {
            return Equals(other);
        }

        return false;
    }

    /// <inheritdoc />
    public bool Equals(FusionCacheTagKey? other)
    {
        if (other is null)
        {
            return false;
        }

        return string.Equals(other.Key, Key, StringComparison.Ordinal) &&
            other.duration == duration &&
            string.Equals(other.varyBy, varyBy, StringComparison.Ordinal) &&
            AreSame(cookies, other.cookies) &&
            AreSame(headers, other.headers) &&
            AreSame(queries, other.queries) &&
            AreSame(routeValues, other.routeValues) &&
            (varyByUser == other.varyByUser &&
                (!varyByUser || string.Equals(other.username, username, StringComparison.Ordinal))) &&
            CultureEquals();

        bool CultureEquals()
        {
            if (varyByCulture != other.varyByCulture)
            {
                return false;
            }

            if (!varyByCulture || requestCulture is null || requestUICulture is null)
            {
                // Neither has culture set.
                return true;
            }

            return requestCulture.Equals(other.requestCulture) &&
                requestUICulture.Equals(other.requestUICulture);
        }
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
        // The hashcode is intentionally not using the computed
        // stringified key in order to prevent string allocations
        // in the common case where it's not explicitly required.

        // Caching as the key is immutable and it can be called
        // multiple times during a request.
        if (hashcode.HasValue)
        {
            return hashcode.Value;
        }

        var hashCode = default(HashCode);

        hashCode.Add(Key, StringComparer.Ordinal);
        hashCode.Add(duration);
        hashCode.Add(varyBy, StringComparer.Ordinal);
        hashCode.Add(username, StringComparer.Ordinal);
        hashCode.Add(requestCulture);
        hashCode.Add(requestUICulture);

        CombineCollectionHashCode(ref hashCode, VaryByCookieName, cookies);
        CombineCollectionHashCode(ref hashCode, VaryByHeaderName, headers);
        CombineCollectionHashCode(ref hashCode, VaryByQueryName, queries);
        CombineCollectionHashCode(ref hashCode, VaryByRouteName, routeValues);

        hashcode = hashCode.ToHashCode();

        return hashcode.Value;
    }

    private static IList<KeyValuePair<string, string>> ExtractCollection<TSourceCollection>(
        string? keys,
        TSourceCollection collection,
        Func<TSourceCollection, string, string> accessor)
    {
        if (string.IsNullOrEmpty(keys))
        {
            return null!;
        }

        var tokenizer = new StringTokenizer(keys, attributeSeparator);

        var result = new List<KeyValuePair<string, string>>();

        foreach (var item in tokenizer)
        {
            var trimmedValue = item.Trim();

            if (trimmedValue.Length != 0)
            {
                string value = accessor(collection, trimmedValue.Value!);

                result.Add(new KeyValuePair<string, string>(trimmedValue.Value!, value ?? string.Empty));
            }
        }

        return result;
    }

    private static void AddStringCollection(
        StringBuilder builder,
        string collectionName,
        IList<KeyValuePair<string, string>> values)
    {
        if (values == null || values.Count == 0)
        {
            return;
        }

        // keyName(param1=value1|param2=value2)
        builder
            .Append(CacheKeyTokenSeparator)
            .Append(collectionName)
            .Append('(');

        for (int i = 0; i < values.Count; i++)
        {
            var item = values[i];

            if (i > 0)
            {
                builder.Append(CacheKeyTokenSeparator);
            }

            builder
                .Append(item.Key)
                .Append(CacheKeyTokenSeparator)
                .Append(item.Value);
        }

        builder.Append(')');
    }

    private static void CombineCollectionHashCode(
        ref HashCode hashCode,
        string collectionName,
        IList<KeyValuePair<string, string>> values)
    {
        if (values != null)
        {
            hashCode.Add(collectionName, StringComparer.Ordinal);

            for (int i = 0; i < values.Count; i++)
            {
                var item = values[i];
                hashCode.Add(item.Key);
                hashCode.Add(item.Value);
            }
        }
    }

    private static bool AreSame(IList<KeyValuePair<string, string>> values1, IList<KeyValuePair<string, string>> values2)
    {
        if (values1 == values2)
        {
            return true;
        }

        if (values1 == null || values2 == null || values1.Count != values2.Count)
        {
            return false;
        }

        for (int i = 0; i < values1.Count; i++)
        {
            if (!string.Equals(values1[i].Key, values2[i].Key, StringComparison.Ordinal) ||
                !string.Equals(values1[i].Value, values2[i].Value, StringComparison.Ordinal))
            {
                return false;
            }
        }

        return true;
    }
}
