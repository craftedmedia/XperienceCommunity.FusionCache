using System.Reflection;

namespace XperienceCommunity.FusionCache.Utilities;

/// <summary>
/// Xperience Admin utility methods.
/// </summary>
internal static class XperienceAdminHelper
{
    /// <summary>
    /// Nasty solution for determining whether the current instance is the detached admin.
    /// </summary>
    /// <returns>A value indicating whether the current instance is running the admin.</returns>
    public static bool IsRunningAdmin()
    {
        try
        {
            // This is pretty awful, but we don't currently have a better way of determining
            // whether the current application is the 'admin' instance when running in detached admin mode. It will have to do for now.
            var asm = Assembly.Load("kentico.xperience.admin.base");

            return asm is not null;
        }
        catch (FileNotFoundException)
        {
            return false;
        }
    }
}
