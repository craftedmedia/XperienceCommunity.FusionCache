using Kentico.PageBuilder.Web.Mvc;

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace XperienceCommunity.FusionCache.Services;

/// <summary>
/// Service for retrieving cache vary by option values.
/// </summary>
public class CacheVaryByOptionService
{
    private readonly IHttpContextAccessor httpContextAccessor;

    /// <summary>
    /// Initializes a new instance of the <see cref="IHttpContextAccessor"/> class.
    /// </summary>
    /// <param name="httpContextAccessor">Instance of <see cref="IHttpContextAccessor"/>.</param>
    public CacheVaryByOptionService(IHttpContextAccessor httpContextAccessor) => this.httpContextAccessor = httpContextAccessor;

    /// <summary>
    /// Gets <see cref="ICacheVaryByOption"/> instances.
    /// </summary>
    /// <param name="types">Vary by types.</param>
    /// <returns>Collection of <see cref="ICacheVaryByOption"/>.</returns>
    public IEnumerable<ICacheVaryByOption> GetVaryByOptions(Type[]? types)
    {
        if (types is null)
        {
            yield break;
        }

        var services = httpContextAccessor?.HttpContext?.RequestServices;

        if (services is null)
        {
            yield break;
        }

        foreach (var type in types.OrderBy(x => x.FullName))
        {
            yield return (ICacheVaryByOption)ActivatorUtilities.CreateInstance(services, type);
        }
    }

    /// <summary>
    /// Gets a <see cref="Dictionary{TKey, TValue}"/> for the provided vary by option types.
    /// </summary>
    /// <param name="types">Vary by types.</param>
    /// <returns><see cref="Dictionary{TKey, TValue}"/>.</returns>
    public IDictionary<string, string>? GetVaryByOptionsDictionary(Type[]? types)
    {
        if (types is null)
        {
            return null;
        }

        var services = httpContextAccessor?.HttpContext?.RequestServices;

        if (services is null)
        {
            return null;
        }

        var dict = new Dictionary<string, string>();

        foreach (var type in types)
        {
            var varyByOption = (ICacheVaryByOption)ActivatorUtilities.CreateInstance(services, type);

            string? keyValue = varyByOption?.GetKey();

            if (string.IsNullOrEmpty(keyValue))
            {
                continue;
            }

            dict.TryAdd(type.Name, keyValue);
        }

        return dict;
    }
}
