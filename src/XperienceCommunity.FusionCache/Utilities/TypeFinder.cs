using System.Reflection;

namespace XperienceCommunity.FusionCache.Caching.Utilities;

/// <summary>
/// Type finder utility methods.
/// </summary>
internal static class TypeFinder
{
    /// <summary>
    /// Finds implementations of the given interface within the provided assembly.
    /// </summary>
    /// <typeparam name="TInterface">Interface type to search for.</typeparam>
    /// <param name="asm">Assembly to search within.</param>
    /// <returns>Collection of types which implement the given interface.</returns>
    public static IEnumerable<Type?> GetImplementationsOf<TInterface>(Assembly? asm)
    {
        if (asm is null)
        {
            return Enumerable.Empty<Type>();
        }

        var type = typeof(TInterface);

        return GetAssemblyTypes(asm).Where(type.IsAssignableFrom).Where(t => t is not null && !t.Equals(type)).ToList();
    }

    private static IEnumerable<Type?> GetAssemblyTypes(Assembly asm)
    {
        try
        {
            return asm.GetTypes();
        }
        catch (ReflectionTypeLoadException e)
        {
            return e.Types.Where(t => t != null);
        }
    }
}
