# XperienceCommunity.FusionCache
[![License](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE)

## Description
This package integrates with the popular Hybrid Caching library known as [ZiggyCreatures.FusionCache](https://github.com/ZiggyCreatures/FusionCache) allowing for L1 + L2 caching within Xperience.
It provides some useful utilities such as cache invalidation via Kentico cache dependencies, custom `FusionCache` backed cache tag helper and support for output caching.

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

And that should be enough to get going! Read on for a more info.

## Full Instructions

### Default Cache options

You can choose to configure some default `FusionCacheEntryOptions` via `appsettings.json` config. These will be used as the default for all cache entries, although they can be overridden directly when using any of the methods within `IFusionCache`.

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

### Fusion Cache Tag Helper

The package provides a custom cache tag helper backed by `FusionCache`.

To use it, add the following include to your `_ViewImports.cshtml` file:

```
@addTagHelper *, XperienceCommunity.FusionCache
```

And include the tag helper in your view:

```
<xperience-fusion-cache
    name="home-page-cache"
    cache-dependencies="@(new string[] { "webpageitem|byid|1", "contentitem|bycontenttype|Medio.Clinic" })"
    duration="@TimeSpan.FromMinutes(5)">

@* Cached HTML goes here *@

</xperience-fusion-cache>
```

The tag helper supports Kentico cache dependencies and all standard `vary-by-*` attributes as well as a `duration` property, however there is **no** support for `expires-sliding`, `expires-on`, and `expires-after` since `FusionCache` only supports `Duration`.

### Output cache

This package integrates with the Asp NET Core Output Caching middleware via a custom `IOutputCacheStore` and `IOutputCachePolicy` which has been integrated with `FusionCache`.

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

### Extending cache invalidation for custom object types

Cache invalidation of standard Kentico objects (pages, content items, media etc...) is handled out of the box but if you want invalidation for general object types (those that inherit from `BaseInfo`, to be precise) then you should implement the `IGeneralObjectCacheItemsProvider` type and place it somewhere within your application root.

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
