# XperienceCommunity.FusionCache

[![License](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE)

## Overview

`XperienceCommunity.FusionCache` integrates [ZiggyCreatures.FusionCache](https://github.com/ZiggyCreatures/FusionCache) with Xperience by Kentico, providing a true L1 + L2 layered caching solution.

The package includes:

- Cache invalidation via Kentico cache dependency keys
- A custom `FusionCache` backed cache tag helper
- ASP.NET Core Output Caching support
- Content personalization support
- Redis-backed L2 cache support
- A Redis administration application with an overview and exclusion-aware purging

If you're unfamiliar with hybrid caching, read the [gentle introduction to FusionCache](https://github.com/ZiggyCreatures/FusionCache/blob/main/docs/AGentleIntroduction.md).

## Compatibility

### Library version matrix

| Xperience Version | Library Version |
| ----------------- | --------------- |
| 31.5.4+           | 2.x             |
| 30.x              | 1.x             |

Version 2.x contains breaking changes and requires Xperience by Kentico 31.0.0 or later. Projects using Xperience 30.x should use the latest 1.x version.

## Requirements

- [ASP.NET Core 8.0](https://dotnet.microsoft.com/en-us/download)
- [Xperience by Kentico](https://docs.kentico.com)
- A Redis instance to use as your L2 cache

## Installation

Install the `XperienceCommunity.FusionCache` package via NuGet or run:

```powershell
Install-Package XperienceCommunity.FusionCache
```

from the Package Manager Console.

## Quick start

### 1. Configure `appsettings.json`

Add the following section to your `appsettings.json` file:

```json
{
  "XperienceFusionCache": {
    "RedisConnectionString": "REDIS CONNECTION STRING GOES HERE"
  }
}
```

### 2. Register services

Add the following code to your `Program.cs` file:

```csharp
var builder = WebApplication.CreateBuilder(args);

// ...

builder.Services.AddXperienceFusionCache(builder.Configuration);
```

This uses the `RedisConnectionString` configured in `appsettings.json`. The package registers a singleton `IConnectionMultiplexer` if one has not already been registered, then reuses that multiplexer for the FusionCache distributed cache, backplane, and distributed locker.

The registered `IConnectionMultiplexer` can also be injected and reused by your application.

### 3. Add middleware

Include `UseXperienceFusionCache()` before `app.Run()`:

```csharp
app.UseXperienceFusionCache();
```

### 4. Register tag helpers

Include the following in your `_ViewImports.cshtml` file:

```html
@addTagHelper *, XperienceCommunity.FusionCache
```

### 5. Start caching

Use the `FusionCache` tag helper:

```html
<xperience-fusion-cache
    name="home-page-cache"
    cache-dependencies="@(new string[] { "webpageitem|all" })"
    duration="@TimeSpan.FromMinutes(5)">

    @* Cached HTML goes here *@

</xperience-fusion-cache>
```

Or inject `IFusionCache` and provide Kentico cache dependency keys as tags:

```csharp
public class ProductService
{
    private readonly IFusionCache fusionCache;

    public ProductService(IFusionCache fusionCache) =>
        this.fusionCache = fusionCache;

    public async Task<IEnumerable<ProductDTO>> GetProductsAsync()
    {
        return await fusionCache.GetOrSetAsync(
            key: "Products.All",
            factory: async (_, _) => await GetProductsFromSourceAsync(),
            tags:
            [
                CacheHelper.BuildCacheItemName(new[] { ProductItem.CONTENT_TYPE_NAME, "all" })
            ]);
    }
}
```

Or use the output cache policy:

```csharp
[OutputCache(PolicyName = "XperienceFusionCache", Tags = ["webpageitem|all"])]
```

## Configuration

Package configuration is provided through the `XperienceFusionCache` section in `appsettings.json`. Redis connection configuration can be provided either through `appsettings.json` or by passing an `IConnectionMultiplexer` or factory when registering services.

### Redis connection configuration

Redis is required unless `DevMode` is enabled.

The package supports two Redis connection approaches:

1. Configure a Redis connection string in `appsettings.json`
2. Provide an existing `IConnectionMultiplexer` or connection multiplexer factory when registering services

#### Using `RedisConnectionString`

The simplest setup is to configure a Redis connection string in the `XperienceFusionCache` section.

```json
{
  "XperienceFusionCache": {
    "RedisConnectionString": "REDIS CONNECTION STRING GOES HERE"
  }
}
```

Then register the package with:

```csharp
builder.Services.AddXperienceFusionCache(builder.Configuration);
```

When using this approach, the package registers a singleton `IConnectionMultiplexer` if one has not already been registered.

The resolved `IConnectionMultiplexer` is used by the FusionCache distributed cache, backplane, and distributed locker. It can also be injected and reused by your own application services.

```csharp
public class MyRedisService
{
    private readonly IConnectionMultiplexer redis;

    public MyRedisService(IConnectionMultiplexer redis)
    {
        this.redis = redis;
    }
}
```

If your application has already registered an `IConnectionMultiplexer`, the package reuses that existing registration instead of creating another one from `RedisConnectionString`.

#### Using a provided `IConnectionMultiplexer`

If your application already creates and manages a Redis connection multiplexer, pass it to `AddXperienceFusionCache`.

```csharp
using StackExchange.Redis;

var redisConnectionMultiplexer = await ConnectionMultiplexer.ConnectAsync(
    builder.Configuration.GetConnectionString("Redis")!);

builder.Services.AddXperienceFusionCache(
    builder.Configuration,
    redisConnectionMultiplexer);
```

When using this overload, the provided multiplexer is registered as the app-level singleton `IConnectionMultiplexer` if one has not already been registered. It is then shared by the FusionCache distributed cache, backplane, and distributed locker.

The package does not create a multiplexer passed to this overload. The calling application should still treat it as an application-level shared Redis connection.

#### Using a connection multiplexer factory

You can also provide a factory function. This is useful when the multiplexer is already registered in dependency injection.

```csharp
using StackExchange.Redis;

builder.Services.AddSingleton<IConnectionMultiplexer>(_ =>
    ConnectionMultiplexer.Connect(
        builder.Configuration.GetConnectionString("Redis")!));

builder.Services.AddXperienceFusionCache(
    builder.Configuration,
    serviceProvider => serviceProvider.GetRequiredService<IConnectionMultiplexer>());
```

An async factory overload is also available for applications that resolve the multiplexer asynchronously from their own connection provider.

```csharp
builder.Services.AddSingleton<IRedisConnectionMultiplexerProvider, RedisConnectionMultiplexerProvider>();

builder.Services.AddXperienceFusionCache(
    builder.Configuration,
    serviceProvider => serviceProvider
        .GetRequiredService<IRedisConnectionMultiplexerProvider>()
        .GetConnectionMultiplexerAsync());
```

The factory result is shared and reused internally by the package. The factory is not called for every cache operation.

Prefer returning a DI-managed singleton `IConnectionMultiplexer`, or otherwise ensure the returned multiplexer is managed as an application-level shared Redis connection.

#### Choosing a Redis connection approach

For most projects, configuring `RedisConnectionString` is sufficient.

Use a provided `IConnectionMultiplexer` or factory when:

- Your application already manages Redis connections
- Multiple libraries in your application need to share the same Redis multiplexer
- You need custom Redis connection setup that cannot be represented by the package configuration

### FusionCache entry options

#### `DefaultFusionCacheEntryOptions`

You can configure default `FusionCacheEntryOptions` via `appsettings.json`. These are used as the default for all cache entries, although they can be overridden on a per-call basis when using `IFusionCache`.

See the FusionCache documentation for [default entry options](https://github.com/ZiggyCreatures/FusionCache/blob/main/docs/Options.md#defaultentryoptions).

```json
{
  "XperienceFusionCache": {
    "DefaultFusionCacheEntryOptions": {
      "Duration": "00:05:00",
      "DistributedCacheDuration": "00:10:00",
      "IsFailSafeEnabled": true,
      "FailSafeMaxDuration": "02:00:00"
    }
  }
}
```

Any option available on `FusionCacheEntryOptions` is also available to be set here. See the [FusionCacheEntryOptions source](https://github.com/ZiggyCreatures/FusionCache/blob/f3896a5f5b6e21f918009d687520938d322f79f4/src/ZiggyCreatures.FusionCache/FusionCacheEntryOptions.cs).

### Serialization options

#### `DefaultSerializer`

[NewtonsoftJson](https://www.nuget.org/packages/ZiggyCreatures.FusionCache.Serialization.NewtonsoftJson/) is configured as the default serializer for maximum compatibility and ease of use.

You can configure any of the following serializers:

- [SystemTextJson](https://www.nuget.org/packages/ZiggyCreatures.FusionCache.Serialization.SystemTextJson)
- [CysharpMemoryPack](https://www.nuget.org/packages/ZiggyCreatures.FusionCache.Serialization.CysharpMemoryPack)
- [NeueccMessagePack](https://www.nuget.org/packages/ZiggyCreatures.FusionCache.Serialization.NeueccMessagePack)
- [ServiceStackJson](https://www.nuget.org/packages/ZiggyCreatures.FusionCache.Serialization.ServiceStackJson)
- [ProtoBufNet](https://www.nuget.org/packages/ZiggyCreatures.FusionCache.Serialization.ProtoBufNet)

See the [FusionCache serializer performance benchmarks](https://github.com/ZiggyCreatures/FusionCache/pull/349) for more information.

To configure a different serializer, specify the `DefaultSerializer` option:

```json
{
  "XperienceFusionCache": {
    "RedisConnectionString": "...",
    "DefaultSerializer": "NeueccMessagePack"
  }
}
```

`FusionCache` will now use the configured serializer instead of the default. Each serializer has its own benefits, trade-offs, and quirks you should familiarize yourself with before using.

### Event handling options

#### `RegisterEventHandlersOnlyInAdmin`

Controls whether cache invalidation event handlers are only active in the administration application.

```json
{
  "XperienceFusionCache": {
    "RegisterEventHandlersOnlyInAdmin": false
  }
}
```

The default value is `false`.

When enabled, event handlers only run in the admin application. This can reduce duplicate Redis calls and FusionCache backplane operations in separated admin/live-site environments.

Only enable this if your project performs relevant content and object changes exclusively through the admin application. Leave it disabled if changes may happen from the live site, scheduled tasks, integrations, imports, or background services.

### Development mode

#### `DevMode`

The library has a development mode setting which skips reads and writes to the L2 cache for easier local development.

```json
{
  "XperienceFusionCache": {
    "DevMode": true
  }
}
```

### Output cache options

#### `OutputCachePolicyName`

Controls the name of the output cache policy registered by the package.

#### `OutputCacheExpiration`

Controls the default expiration used by the output cache policy.

```json
{
  "XperienceFusionCache": {
    "OutputCachePolicyName": "MyOutputCachePolicy",
    "OutputCacheExpiration": "00:05:00"
  }
}
```

## Usage

### Using `IFusionCache`

Inject `IFusionCache` and use the Get/Set methods, providing `tags` as Kentico cache dependency keys:

```csharp
var products = await this.fusionCache.GetOrSetAsync<IEnumerable<ProductDTO>?>(
    key: "FooWebsite.Products",
    factory: async (ctx, _) =>
    {
        var products = await this.GetProductsAsync();

        if (products is null)
        {
            ctx.Options.Duration = TimeSpan.Zero;
            return null;
        }

        return products;
    },
    tags:
    [
        CacheHelper.BuildCacheItemName(new[] { ProductItem.CONTENT_TYPE_NAME, "all" })
    ]);
```

### Using the tag helper

The package provides a custom cache tag helper backed by `FusionCache`.

```html
<xperience-fusion-cache
    name="home-page-cache"
    cache-dependencies="@(new string[] { "webpageitem|byid|1", "contentitem|bycontenttype|Medio.Clinic" })"
    duration="@TimeSpan.FromMinutes(5)"
    vary-by-option-types="@(new[] { typeof(ContactGroupVaryByOption) })">

    @* Cached HTML goes here *@

</xperience-fusion-cache>
```

#### Tag helper options

| Option               | Description                                                                                                                                                               | Example                                                                                      | Default   |
| -------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | -------------------------------------------------------------------------------------------- | --------- |
| name                 | Required. A unique name for the tag instance.                                                                                                                             | `"product-listing"`                                                                          | `null`    |
| enabled              | A value indicating whether caching is enabled for the tag.                                                                                                                | `true`                                                                                       | `true`    |
| cache-dependencies   | Collection of cache dependencies for the cache entry. The associated cache item is cleared when one of the dependencies is touched by the system.                         | `new string[] { "webpageitem\|byid\|3" }`                                                   | `null`    |
| cacheability-rules   | Collection of custom rules that determine whether the tag inner content can be cached based on whether the `IReadOnlyMemory<char>` pattern was found within the tag HTML. | `Func<ReadOnlyMemory<char>, bool> CacheDisabled = content => content.Span.IndexOf("cache-disabled=\"True\"") <= -1` | `null` |
| duration             | Cache duration.                                                                                                                                                          | `TimeSpan.FromMinutes(5)`                                                                    | 5 minutes |
| vary-by              | Custom vary-by string.                                                                                                                                                   | `$"product-{product.Id}"`                                                                    | `null`    |
| vary-by-header       | Vary the cache by the provided headers.                                                                                                                                   | `"header1,header2"`                                                                          | `null`    |
| vary-by-query        | Vary the cache by the provided query parameters.                                                                                                                          | `"page,filter"`                                                                              | `null`    |
| vary-by-route        | Vary the cache by the provided route parameters.                                                                                                                          | `"lang,id"`                                                                                  | `null`    |
| vary-by-cookie       | Vary the cache by the provided cookie names.                                                                                                                              | `"cookie1,cookie2"`                                                                          | `null`    |
| vary-by-user         | Vary the cache by the current user.                                                                                                                                       | `true`                                                                                       | `false`   |
| vary-by-culture      | Vary the cache by the current request culture.                                                                                                                            | `true`                                                                                       | `false`   |
| vary-by-option-types | `ICacheVaryByOption` implementations to vary the cache by. Useful for content personalization.                                                                            | `new[] { typeof(ContactGroupVaryByOption) }`                                                  | `null`    |

### Using output caching

This package integrates with ASP.NET Core Output Caching middleware via a custom `IOutputCacheStore` and `IOutputCachePolicy` which has been integrated with `FusionCache`.

Reference `XperienceFusionCache` as the policy name when using the `[OutputCache]` attribute and optionally specify cache dependencies via the `Tags` attribute.

```csharp
public class HomePageController : Controller
{
    [OutputCache(PolicyName = "XperienceFusionCache", Tags = ["webpageitem|all"])]
    public async Task<IActionResult> Index()
    {
        // Perform some expensive logic...

        // Associate cache dependencies with the current request
        this.HttpContext.AddCacheDependencies(
            new HashSet<string>
            {
                CacheHelper.BuildCacheItemName(new[] { "webpageitem", "bychannel", "MyWebsite", "bycontenttype", "website.homepage" }),
            });

        return new TemplateResult();
    }
}
```

You can also specify cache dependencies via the `AddCacheDependencies` extension method if they aren't known at runtime.

### Content personalization

The library provides several ways to inject unique vary-by keys into each cache item key, providing compatibility with the widget personalization feature within Xperience.

See the Xperience documentation for [widget personalization](https://docs.kentico.com/business-users/digital-marketing/widget-personalization) and [custom personalization options for output caching](https://docs.kentico.com/developers-and-admins/development/caching/output-caching#implement-custom-personalization-options).

To use this feature, implement custom `ICacheVaryByOption` types and ensure a unique key is returned based on your use case.

```csharp
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

Then pass these types to the `vary-by-option-types` attribute if using the `<xperience-fusion-cache />` tag helper:

```html
<xperience-fusion-cache
    name="my-widget-cache"
    duration="@TimeSpan.FromMinutes(5)"
    vary-by-option-types="@(new[] { typeof(ContactGroupVaryByOption) })">

    @* Cached HTML which should vary by contact group goes here *@

</xperience-fusion-cache>
```

Alternatively, if using controller-level `[OutputCache]`, decorate the action result with `[XperienceFusionCacheVaryByOptionTypes]` and specify your custom `ICacheVaryByOption` types in the constructor:

```csharp
[OutputCache(PolicyName = "XperienceFusionCache", Tags = ["webpageitem|all"])]
[XperienceFusionCacheVaryByOptionTypes(VaryByOptionTypes = [typeof(ContactGroupVaryByOption)])]
public async Task<IActionResult> Index()
{
    // Some expensive logic...

    return new TemplateResult();
}
```

This ensures that your custom vary-by option implementations are considered when constructing a unique cache key for the cache item.

## Cache invalidation

### Built-in invalidation

The package automatically handles cache invalidation for supported Xperience by Kentico objects, including:

- Web pages
- Content items
- Media files
- Settings keys
- Headless items

When supported objects change, the package generates the relevant Kentico cache dependency keys and removes FusionCache entries associated with those tags.

### Extending invalidation for custom object types

If you want to invalidate cache entries when other Xperience objects change, you can register one or more `IGeneralObjectCacheItemsProvider` implementations.

This is useful for objects that inherit from `BaseInfo`, such as custom module classes or other Kentico/Xperience info objects. The provider exposes a collection of `ObjectTypeInfo` instances through the `GeneralObjectInfos` property.

#### Implement `IGeneralObjectCacheItemsProvider`

```csharp
public sealed class BrandCacheItemsProvider : IGeneralObjectCacheItemsProvider
{
    public IEnumerable<ObjectTypeInfo> GeneralObjectInfos =>
    [
        ProductBrandInfo.TYPEINFO
    ];
}
```

#### Register the provider

Register the provider as a singleton in `Program.cs`:

```csharp
builder.Services.AddSingleton<IGeneralObjectCacheItemsProvider, BrandCacheItemsProvider>();
```

You can register multiple providers if you want to split object registrations by feature or project area:

```csharp
builder.Services.AddSingleton<IGeneralObjectCacheItemsProvider, MembershipCacheItemsProvider>();
builder.Services.AddSingleton<IGeneralObjectCacheItemsProvider, CustomModuleCacheItemsProvider>();
```

#### How general object invalidation works

When a registered general object type is inserted, updated, or deleted, the package generates the matching general object dummy cache keys and removes FusionCache entries associated with those tags.

For example, if your provider returns:

```csharp
UserInfo.TYPEINFO
```

then user object changes can invalidate entries tagged with the corresponding general object cache dependencies.

The package only tracks the object class names exposed by the `ObjectTypeInfo` instances returned from registered `IGeneralObjectCacheItemsProvider` implementations. If no providers are registered, general object invalidation is skipped.

See the Xperience documentation for [general object cache dependency keys](https://docs.kentico.com/developers-and-admins/development/caching/cache-dependencies#general-objects).

## Fusion Cache administration module

The Fusion Cache administration screens are included in `XperienceCommunity.FusionCache` and register automatically in the Xperience administration. Keep the existing `AddXperienceFusionCache(...)` registration in the application hosting the administration, including when using a detached admin.

Open **Development → Fusion Cache** at `/admin/fusion-cache` (overview: `/admin/fusion-cache/overview`):

- **Overview** shows the configured friendly name, Redis instance name, connection status, selected database, total key count, primary endpoints, Redis version, memory usage and peak memory in MiB, connected clients, uptime, and snapshot time. Refresh the page for updated figures. Key counts cover the selected database; memory, client counts and uptime are server-wide. Replicas are excluded from totals. Unavailable fields and server statistics are omitted, including when Redis denies `INFO` or `DBSIZE`; genuine zero values remain visible. Connection status is independent of permissions to read optional statistics. See [Redis admin mode and overview fields](#redis-admin-mode-and-overview-fields) for the requirements of each field.
- **Purge Redis** deletes all keys in that database except configured exclusions and additional exclusions entered for the current purge. Existing in-memory entries remain available until expiration.
- **Purge All** uses FusionCache's API to invalidate its entries in both layers across applications sharing this cache's Redis backplane. It does not scan or directly delete Redis keys. Session, data protection and other keys stored outside FusionCache are unaffected.

The Redis purge accepts one exact key or pattern per line. The Fusion Cache purge has no exclusion fields because it invalidates all entries within this FusionCache scope. For either action, type `PURGE`, then confirm the dialog. The Redis purge result reports confirmed deletions and excluded key observations; repeated observations are possible during a scan. The Fusion Cache purge result reports whether invalidation was published.

Grant the application's **View** permission for the overview and **Purge caches** permission for either purge section through Xperience's role management. Purge commands enforce the purge permission on the server. Purge outcomes, scope and the initiating user's ID are recorded in Xperience's event log under `FusionCache.Redis`.

### Redis admin mode and overview fields

The overview works without Redis admin mode. The optional server statistics require `allowAdmin=true` because StackExchange.Redis gates explicit `INFO` calls behind that setting. This is a Redis client setting, separate from the user's Xperience administration permissions.

| Overview field | Requires `allowAdmin=true`? | Source |
| --- | --- | --- |
| Redis name and Redis instance name | No | Application configuration, with Redis host names as the instance-name fallback |
| Connection status and Database | No | The shared Redis connection |
| Total keys in database | No | `DBSIZE` for the selected database on each primary server; Redis must permit this command |
| Primary server endpoints | No | The shared Redis connection |
| Last updated | No | The time the application generated the snapshot |
| Memory used by primary servers | Yes | `INFO`, summed across primary servers when all memory values are available |
| Redis version, Memory, Peak memory, Connected clients, and Uptime under Primary servers | Yes | `INFO` for each primary server |

When admin mode is disabled, the primary server endpoints remain visible, but their optional statistics and the aggregate memory field are hidden. If Redis denies `INFO` or omits a statistic, the corresponding fields are also hidden. The total key count is hidden if any primary server's count is unavailable, including when Redis denies `DBSIZE`.

To enable the optional statistics, add `allowAdmin=true` to the Redis connection string used by FusionCache, for example:

```text
localhost:6379,allowAdmin=true
```

When the package creates the connection, this is the `XperienceFusionCache:RedisConnectionString` setting. When supplying an existing connection multiplexer or a factory to `AddXperienceFusionCache(...)`, enable admin mode on that connection before creating it:

```csharp
var redisOptions = ConfigurationOptions.Parse(redisConnectionString);
redisOptions.AllowAdmin = true;
var connection = await ConnectionMultiplexer.ConnectAsync(redisOptions);
```

The package reuses that connection and does not change its permissions. Changing the package's connection-string setting has no effect when an externally supplied multiplexer is used. Restart the application after changing the connection configuration so the shared multiplexer is recreated.

`allowAdmin=true` does not grant Redis server permissions: the authenticated Redis user must also be allowed to execute `INFO`. `DBSIZE` permission is required for key counts even when admin mode is disabled. See the [StackExchange.Redis configuration reference](https://github.com/StackExchange/StackExchange.Redis/blob/main/docs/Configuration.md), [Redis INFO reference](https://redis.io/docs/latest/commands/info/), and [Redis DBSIZE reference](https://redis.io/docs/latest/commands/dbsize/).

Neither **Purge Redis** nor **Purge All** requires admin mode. Their normal Redis command permissions still apply.

### Persistent exclusions

Use `XperienceFusionCache:RedisManagement` to configure a friendly name and exclusions that apply to **Purge Redis**. Administrators cannot remove these configured protections through the purge form. Additional exclusions entered in the form apply only to that purge and are not saved. These rules are not used by **Purge All**.

```json
{
  "XperienceFusionCache": {
    "RedisConnectionString": "localhost:6379,defaultDatabase=0",
    "RedisManagement": {
      "DisplayName": "Website cache",
      "ExcludedKeys": ["shared-dataprotection-keys"],
      "ExcludedPatterns": ["session:*"],
      "PurgeTimeout": "00:02:00"
    }
  }
}
```

Exclusions match the **actual Redis key**, including any prefixes added by your cache libraries. Matching is case-sensitive and preserves spaces. Exact-key exclusions treat every character literally. Patterns support `*` (zero or more characters) and `?` (one character); all other characters, including brackets and backslashes, are literal. For example, `session:*` excludes keys beginning with `session:`, and `*:settings` excludes keys ending with `:settings`. A pattern of `*` skips all direct Redis deletion. Each combined list of configured and additional exclusions supports up to 200 distinct entries of at most 1024 characters each.

### Purge behavior

**Purge Redis** scans the entire selected Redis database, including keys belonging to other applications if they share it. Other Redis databases are not scanned. Existing FusionCache L1/in-memory entries remain available until expiration, including fail-safe retention where enabled.

**Purge All** calls FusionCache's `ClearAsync(allowFailSafe: false)` without scanning or directly deleting Redis keys. This makes previously cached FusionCache entries unavailable, including fail-safe copies, on the current instance and other connected applications sharing its backplane channel and cache key prefix. Redis entries may remain until expiry or subsequent access, so key counts need not fall immediately. It requires the distributed cache, backplane and tagging to be enabled. It does not clear unrelated `IMemoryCache` instances or separately configured FusionCache caches. See FusionCache's [Clear documentation](https://github.com/ZiggyCreatures/FusionCache/blob/v2.9.0/docs/Clear.md).

The Fusion Cache action waits for backplane publication before reporting success. Remote applications do not acknowledge completion, and disconnected applications may miss the notification. A failure reports that global invalidation could not be confirmed; some applications may already have invalidated their entries.

A Redis purge is not an atomic snapshot. Writers can create or replace entries while it runs, so successful completion does not guarantee an empty database. Avoid concurrent purges across different admin instances. A timeout or connection/permission failure can leave a partially purged database; the result reports confirmed deletions and does not claim success. `PurgeTimeout` applies to both actions, defaults to two minutes and must be greater than zero and at most one hour. Commands already sent are allowed to finish before the operation releases its local lock; proxy/request timeouts may need adjustment for large databases.

With `DevMode` enabled, the overview reports that Redis is disabled and purging is unavailable.

## Contributing

To see the guidelines for contributing to Kentico open source software, see [Kentico's `CONTRIBUTING.md`](https://github.com/Kentico/.github/blob/main/CONTRIBUTING.md) and follow [Kentico's `CODE_OF_CONDUCT`](https://github.com/Kentico/.github/blob/main/CODE_OF_CONDUCT.md).

## License

Distributed under the MIT License. See [`LICENSE.md`](./LICENSE.md) for more information.
