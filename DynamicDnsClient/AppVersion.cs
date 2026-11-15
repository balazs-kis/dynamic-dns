using System.Reflection;

namespace DynamicDnsClient;

public static class AppVersion
{
    /// <summary>
    /// The version of the running application, e.g. "2.2.0+3a1418ad54bedca215d9a8b008f93319929650fb"
    /// (release version from the git tag, plus the commit it was built from).
    /// Reading it at runtime also keeps it in Native AOT builds, which would otherwise trim it away.
    /// </summary>
    public static string Current { get; } =
        typeof(AppVersion).Assembly
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?
            .InformationalVersion ?? "unknown";
}
