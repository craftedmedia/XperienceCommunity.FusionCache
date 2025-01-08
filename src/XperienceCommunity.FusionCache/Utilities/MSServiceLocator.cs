using CommonServiceLocator;

using Microsoft.Extensions.DependencyInjection;

namespace XperienceCommunity.FusionCache.Caching.Utilities;

/// <summary>
/// Custom service locator for use with CSL.
/// </summary>
internal class MSServiceLocator : ServiceLocatorImplBase
{
    private readonly IServiceProvider serviceProvider;

    /// <summary>
    /// Initializes a new instance of the <see cref="MSServiceLocator"/> class.
    /// </summary>
    /// <param name="serviceProvider">Instance of <see cref="IServiceProvider"/>.</param>
    public MSServiceLocator(IServiceProvider serviceProvider) => this.serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));

    /// <summary>
    /// Returns a single instance of the specified service type.
    /// </summary>
    /// <param name="serviceType">Service type.</param>
    /// <param name="key">Key. Not used.</param>
    /// <returns>Service instance.</returns>
    protected override object? DoGetInstance(Type serviceType, string key) => serviceProvider.GetService(serviceType);

    /// <summary>
    /// Returns all instances of the specified service type.
    /// </summary>
    /// <param name="serviceType">Service type.</param>
    /// <returns>Services of the specified type.</returns>
    protected override IEnumerable<object?> DoGetAllInstances(Type serviceType) => serviceProvider.GetServices(serviceType);
}
