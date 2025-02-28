# XperienceCommunity.FusionCache
[![License](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE)

## Description
This package integrates with the popular Hybrid Caching library known as [ZiggyCreatures.FusionCache](https://github.com/ZiggyCreatures/FusionCache) providing a true L1 + L2 layered caching solution within Xperience by Kentico.

It provides some useful utilities such as cache invalidation via Kentico cache dependencies, custom `FusionCache` backed cache tag helper and support for output caching, with content personalization handled out of the box.

If you're unfamiliar with Hybrid Caching, I would recommend reading the gentle intro over at: https://github.com/ZiggyCreatures/FusionCache/blob/main/docs/AGentleIntroduction.md

### Library Version Matrix

| Xperience Version | Library Version |
| ----------------- | --------------- |
| >= 30.0.0         | 1.0.0           |

### Dependencies

- [ASP.NET Core 8.0](https://dotnet.microsoft.com/en-us/download)
- [Xperience by Kentico](https://docs.kentico.com)

### Other requirements

A Redis instance to use as your L2 cache.

### Package Installation

Install the `XperienceCommunity.FusionCache` package via nuget or run:

```
Install-Package XperienceCommunity.FusionCache
```
From package manager console.

## Quick Start

### Configuration

Include the following section within your `appsettings.json` file:

```
 "XperienceFusionCache": {
   "RedisConnectionString": "REDIS CONNECTION STRING GOES HERE"
 }
```

### Register services

Add the following code to your `Program.cs` file:

```
var builder = WebApplication.CreateBuilder(args);

// ...

builder.Services.AddXperienceFusionCache(builder.Configuration);
```

And include `UseXperienceFusionCache()` before `app.Run()`:

```
app.UseXperienceFusionCache();
```

### Update _ViewImports.cshtml

Include the following in your `_ViewImports.cshtml` file:

```
@addTagHelper *, XperienceCommunity.FusionCache
```

### Output caching

Use either:

- Tag helper
    - `<xperience-fusion-cache />`
- Output cache policy
    - `[OutputCache(PolicyName = "XperienceFusionCache", Tags = ["webpageitem|all"])]`

### Services

Inject `IFusionCache` and use the Get/Set methods, providing `tags` as Kentico cache dependency keys:

```
var products = await this.fusionCache.GetOrSetAsync<IEnumerable<ProductDTO>?>(
            key: "FooWebsite.Products",
            factory: async (ctx, _) =>
            {
                var products = await this.GetproductsAsync();

                if (products is null)
                {
                    ctx.Options.Duration = TimeSpan.Zero;
                    return null;
                }

                return products;
            },
            tags: [CacheHelper.BuildCacheItemName(new[] { ProductItem.CONTENT_TYPE_NAME, "all" })]);
```


And that should be enough to get going! Read on for more info.

## Full Instructions

### Default Cache options

You can choose to configure some default `FusionCacheEntryOptions` via `appsettings.json` config. These will be used as the default for all cache entries, although they can be overridden on a per-call basis when using `IFusionCache`. See: [https://github.com/ZiggyCreatures/FusionCache/blob/main/docs/Options.md](https://github.com/ZiggyCreatures/FusionCache/blob/main/docs/Options.md#defaultentryoptions)

Example:

```
"XperienceFusionCache": {
  "DefaultFusionCacheEntryOptions": {
    "Duration": "00:05:00", // 5 mins,
    "DistributedCacheDuration": "00:10:00", // 10 mins,
    "IsFailSafeEnabled": true,
    "FailSafeMaxDuration": "02:00:00" // 2 hours
    // Etc...
  }
}
```

Any option available on the [FusionCacheEntryOptions](https://github.com/ZiggyCreatures/FusionCache/blob/f3896a5f5b6e21f918009d687520938d322f79f4/src/ZiggyCreatures.FusionCache/FusionCacheEntryOptions.cs) is also available to be set here.

### Configuring Serialization
[NewtonsoftJson](https://www.nuget.org/packages/ZiggyCreatures.FusionCache.Serialization.NewtonsoftJson/) is configured as the default serializer for maximum compatibility and ease of use, however it's possible to configure any of the following serializers:

- [SystemTextJson](https://www.nuget.org/packages/ZiggyCreatures.FusionCache.Serialization.SystemTextJson)
- [CysharpMemoryPack](https://www.nuget.org/packages/ZiggyCreatures.FusionCache.Serialization.CysharpMemoryPack)
- [NeueccMessagePack](https://www.nuget.org/packages/ZiggyCreatures.FusionCache.Serialization.NeueccMessagePack)
- [ServiceStackJson](https://www.nuget.org/packages/ZiggyCreatures.FusionCache.Serialization.ServiceStackJson)
- [ProtoBufNet](https://www.nuget.org/packages/ZiggyCreatures.FusionCache.Serialization.ProtoBufNet)

See https://github.com/ZiggyCreatures/FusionCache/pull/349 for performance benchmarks for each of these serializers.

To configure a different serializer, simply specify the `DefaultSerializer` in options:
```
"XperienceFusionCache": {
  "RedisConnectionString": "...",
  "DefaultSerializer": "NeueccMessagePack" // OR 'ServiceStackJson' etc...
},
```
`FusionCache` will now use the configured serializer instead of the default. Each serializer has its pros, cons and individual quirks you should familiarize yourself with before using.


### Fusion Cache Tag Helper

The package provides a custom cache tag helper backed by `FusionCache`.

To use it, include the tag helper in your view:

```
<xperience-fusion-cache
    name="home-page-cache"
    cache-dependencies="@(new string[] { "webpageitem|byid|1", "contentitem|bycontenttype|Medio.Clinic" })"
    duration="@TimeSpan.FromMinutes(5)"
    vary-by-option-types="@(new[] { typeof(ContactGroupVaryByOption) })">

@* Cached HTML goes here *@

</xperience-fusion-cache>
```

See below, for a full list of options:

| Option               | Description                                                                                                                                                                 | Example                                                           | Default    |
| -------------------- | --------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | ----------------------------------------------------------------- | ---------- |
| name                 | Required. A unique name for the tag instance.                                                                                                                             | `"product-listing"`                                                 | `null`     |
| enabled              | A value indicating whether caching is enabled for the tag.                                                                                                                  | `true`                                                            | `true`     |
| cache-dependencies   | Collection of cache dependencies for the cache entry. The associated cache item will be cleared when one of the dependencies is touched by the system.                     | `new string[] { "webpageitem\|byid\|3" }`                         | `null`     |
| cacheability-rules   | Collection of custom rules that determine whether the tag inner content can be cached based on whether the `IReadOnlyMemory<char>` pattern was found within the tags HTML. | `Func<ReadOnlyMemory<char>, bool> CacheDisabled = (content) => content.Span.IndexOf("cache-disabled=\"True\"") <= -1` | `null`     |
| duration             | Cache duration.                                                                                                                                                              | `TimeSpan.FromMinutes(5)`                                         | 5 minutes  |
| vary-by              | Custom vary by string.                                                                                                                                                     | `$"product-{product.Id}"`                                         | `null`     |
| vary-by-header       | Vary the cache by the provided header(s).                                                                                                                                  | `"header1,header2"`                                               | `null`     |
| vary-by-query        | Vary the cache by the provided query parameter(s).                                                                                                                         | `"page,filter"`                                                   | `null`     |
| vary-by-route        | Vary the cache by the provided route parameter(s).                                                                                                                         | `"lang,id"`                                                       | `null`     |
| vary-by-cookie       | Vary the cache by the provided cookie name(s).                                                                                                                             | `"cookie1,cookie2"`                                               | `null`     |
| vary-by-user         | Vary the cache by the current user.                                                                                                                                        | `true`                                                            | `false`    |
| vary-by-culture      | Vary the cache by the current request culture.                                                                                                                             | `true`                                                            | `false`    |
| vary-by-option-types | `ICacheVaryByOption` implementations to vary the cache by. Useful for content personalization.                                                                             | `new[] { typeof(ContactGroupVaryByOption) }`                      | `null`     |




### Output cache

This package integrates with the NET Core Output Caching middleware via a custom `IOutputCacheStore` and `IOutputCachePolicy` which has been integrated with `FusionCache`.

Just reference `XperienceFusionCache` as the policy name when using the `[OutputCache]` attribute and optionally specify cache dependencies via the `Tags` attribute.

Example usage:

```
public class HomePageController : Controller
{
    [OutputCache(PolicyName = "XperienceFusionCache", Tags = ["webpageitem|all"])]
    public async Task<IActionResult> Index()
    {
        // Perform some expensive logic...

        // Associate cache dependencies with the current request
        this.HttpContext.AddCacheDependencies(
            new HashSet<string>() {
                CacheHelper.BuildCacheItemName(new[] { "webpageitem", "bychannel", "MyWebsite", "bycontenttype", "website.homepage" }),
            });

        return new TemplateResult();
    }
}

```

You can also specify cache dependencies via the `AddCacheDependencies` extension method (see example above) if they aren't known at runtime.

Policy defaults can be customized via `appsettings.json`:

```
"XperienceFusionCache": {
  // ...
  "OutputCachePolicyName": "MyOutputCachePolicy",
  "OutputCacheExpiration": "00:05:00"
}
```
### Content Personalization
The library provides several ways to inject unique vary-by keys into each cache items key entry, granting compatibility with the widget personalization feature within Xperience:
https://docs.kentico.com/business-users/digital-marketing/widget-personalization

To utilize this feature, simply implement your custom `ICacheVaryByOption` types, ensuring a unique key is returned based on your own use case:
https://docs.kentico.com/developers-and-admins/development/caching/output-caching#implement-custom-personalization-options

Complete example:
```
public class ContactGroupVaryByOption : ICacheVaryByOption
{
    public string GetKey()
    {
        var contact = ContactManagementContext.GetCurrentContact();

        if (contact?.ContactGroups is null || !contact.ContactGroups.Any())
        {
            return string.Empty;
        }

        var contactGroups = contact.ContactGroups
            .OrderBy(x => x.ContactGroupName)
            .Select(y => y.ContactGroupName);

        return string.Join("||", ["VaryByContactGroup", .. contactGroups]);
    }
}
```

Then pass these types to the `vary-by-option-types` attribute, if using the `<xperience-fusion-cache />` tag helper, e.g:

```
<xperience-fusion-cache
    name="my-widget-cache"
    duration="@TimeSpan.FromMinutes(5)"
    vary-by-option-types="@(new[] { typeof(ContactGroupVaryByOption) })">

@* Cached HTML which should vary by contact group goes here *@

</xperience-fusion-cache>
```

Or alternatively, if using controller level `[OutputCache]`, decorate the action result with `[XperienceFusionCacheVaryByOptionTypes]` and specify your custom `ICacheVaryByOption` types in the constructor, e.g:
```
[OutputCache(PolicyName = "XperienceFusionCache", Tags = ["webpageitem|all"])]
[XperienceFusionCacheVaryByOptionTypes(VaryByOptionTypes = [typeof(ContactGroupVaryByOption)])]
public async Task<IActionResult> Index()
{
    // Some expensive logic...

    return new TemplateResult();
}
```

This ensures that your custom vary by option implementations are considered when constructing a unique cache key for the cache item.


### Extending cache invalidation for custom object types

Cache invalidation of standard Kentico objects (pages, content items, media etc...) is handled out of the box but if you want invalidation for general object types (those that inherit from `BaseInfo`) then you should implement the `IGeneralObjectCacheItemsProvider` type and place it somewhere within your application root.

Example:

```
public class GeneralObjectsCacheItemsProvider : IGeneralObjectCacheItemsProvider
{
    public IEnumerable<ObjectTypeInfo> GeneralObjectInfos => new List<ObjectTypeInfo>()
    {
        SomeCustomTypeInfo.TYPEINFO,
        UserInfo.TYPEINFO,
        //...
    };
}
```

This will ensure cache invalidation based on the 'General objects' dummy cache keys for the listed types: https://docs.kentico.com/developers-and-admins/development/caching/cache-dependencies#general-objects

## Contributing

To see the guidelines for Contributing to Kentico open source software, please see [Kentico's `CONTRIBUTING.md`](https://github.com/Kentico/.github/blob/main/CONTRIBUTING.md) for more information and follow the [Kentico's `CODE_OF_CONDUCT`](https://github.com/Kentico/.github/blob/main/CODE_OF_CONDUCT.md).

## License

Distributed under the MIT License. See [`LICENSE.md`](./LICENSE.md) for more information.

