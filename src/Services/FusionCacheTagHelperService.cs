using System.Collections.Concurrent;
using System.Text;
using System.Text.Encodings.Web;

using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.Logging;

using XperienceCommunity.FusionCache.Caching.TagHelpers;
using XperienceCommunity.FusionCache.TagHelpers;

using ZiggyCreatures.Caching.Fusion;

namespace XperienceCommunity.FusionCache.Caching.Services;

/// <summary>
/// Fusion cache tag helper service.
/// </summary>
public partial class FusionCacheTagHelperService
{
    private static readonly string csrfTokenID = "__RequestVerificationToken";
    private readonly ILogger<FusionCacheTagHelperService> logger;
    private readonly IFusionCache fusionCache;
    private readonly HtmlEncoder htmlEncoder;
    private readonly ConcurrentDictionary<FusionCacheTagKey, Task<HtmlString>> workers;

    /// <summary>
    /// Initializes a new instance of the <see cref="FusionCacheTagHelperService"/> class.
    /// </summary>
    /// <param name="logger">The <see cref="ILogger{TCategoryName}"/>.</param>
    /// <param name="fusionCache">The <see cref="IFusionCache"/> instance.</param>
    /// <param name="htmlEncoder">The <see cref="HtmlEncoder"/> used to encode cache content.</param>
    public FusionCacheTagHelperService(
        ILogger<FusionCacheTagHelperService> logger,
        IFusionCache fusionCache,
        HtmlEncoder htmlEncoder)
    {
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(fusionCache);
        ArgumentNullException.ThrowIfNull(htmlEncoder);

        this.logger = logger;
        this.fusionCache = fusionCache;
        this.htmlEncoder = htmlEncoder;

        workers = new ConcurrentDictionary<FusionCacheTagKey, Task<HtmlString>>();
    }

    /// <summary>
    /// Processes the html content of a distributed cache tag helper.
    /// </summary>
    /// <param name="tagHelperOutput">The <see cref="TagHelperOutput" />.</param>
    /// <param name="key">The key in the storage.</param>
    /// <param name="options">Cache options.</param>
    /// <returns>A cached or new content for the cache tag helper.</returns>
    public async Task<HtmlString> ProcessContentAsync(
        TagHelperOutput tagHelperOutput,
        FusionCacheTagKey key,
        XperienceFusionCacheTagHelperOptions options)
    {
        HtmlString? content = null;

        while (content == null)
        {
            // Is there any request already processing the value?
            if (!workers.TryGetValue(key, out var result))
            {
                // There is a small race condition here between TryGetValue and TryAdd that might cause the
                // content to be computed more than once. We don't care about this race as the probability of
                // happening is very small and the impact is not critical.
                var tcs = new TaskCompletionSource<HtmlString>(creationOptions: TaskCreationOptions.RunContinuationsAsynchronously);

                workers.TryAdd(key, tcs.Task);

                // Get a hashed key based on vary by params etc...
                string? storageKey = key.GenerateHashedKey();

                try
                {
                    var cacheRequest = await fusionCache.TryGetAsync<HtmlString>(storageKey);

                    if (!cacheRequest.HasValue)
                    {
                        // The value is not cached, we need to render the tag helper output
                        content = await GetTagHelperContent(tagHelperOutput);

                        if (!IsCacheable(content.ToString().AsMemory(), options.CacheabilityRules))
                        {
                            return content;
                        }

                        await fusionCache.SetAsync(storageKey, content, opts => opts.SetDuration(options.Duration), tags: options.CacheDependencies);

                        return content;
                    }
                    else
                    {
                        content = cacheRequest.Value;
                    }
                }
                catch (Exception exc)
                {
                    content = null;
                    Log.DistributedFormatterDeserializationException(logger, storageKey, exc);

                    throw;
                }
                finally
                {
                    // Remove the worker task before setting the result.
                    // If the result is null, other threads would potentially
                    // acquire it otherwise.
                    workers.TryRemove(key, out _);

                    // Failsafe, if the content could not be retreived/deserialized
                    content ??= await GetTagHelperContent(tagHelperOutput);

                    // Notify all other awaiters to render the content
                    tcs.TrySetResult(content!);
                }
            }
            else
            {
                content = await result;
            }
        }

        return content;
    }

    private async Task<HtmlString> GetTagHelperContent(TagHelperOutput output)
    {
        var processedContent = await output.GetChildContentAsync();

        var stringBuilder = new StringBuilder();
        using (var writer = new StringWriter(stringBuilder))
        {
            processedContent.WriteTo(writer, htmlEncoder);
        }

        return new HtmlString(stringBuilder.ToString());
    }

    private static bool IsCacheable(ReadOnlyMemory<char> value, IEnumerable<Func<ReadOnlyMemory<char>, bool>>? cacheabilityRules = null)
    {
        bool cacheable = value.Span.IndexOf(csrfTokenID) <= -1;

        if (!cacheable || cacheabilityRules is null || !cacheabilityRules.Any())
        {
            return cacheable;
        }

        foreach (var rule in cacheabilityRules)
        {
            cacheable = rule(value);

            if (!cacheable)
            {
                return false;
            }
        }

        return true;
    }

    private static partial class Log
    {
        [LoggerMessage(1, LogLevel.Error, "Couldn't deserialize cached value for key {Key}.", EventName = "DistributedFormatterDeserializationException")]
        public static partial void DistributedFormatterDeserializationException(ILogger logger, string key, Exception exception);
    }
}
