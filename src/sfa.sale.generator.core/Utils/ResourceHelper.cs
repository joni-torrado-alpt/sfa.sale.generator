using System.Reflection;
namespace sfa.sale.generator.core;

/// <summary>
/// Helper class to read embedded resources in assembly.
/// </summary>
public static class ResourceHelper
{
    /// <summary>
    /// Read embedded resource as Stream.
    /// </summary>
    /// <param name="assembly"></param>
    /// <param name="folder"></param>
    /// <param name="fileName"></param>
    /// <returns></returns>
    public static Stream? ReadResource(Assembly assembly, string folder, string fileName)
    {
        string resourcePath;
        var assemblyName = assembly.GetName().Name;
        if (folder != null)
            resourcePath = $"{assemblyName}.{folder}.{fileName}";
        else
            resourcePath = $"{assemblyName}.{fileName}";

        return assembly.GetManifestResourceStream(resourcePath);
    }

    /// <summary>
    /// Read embedded resource as String.
    /// </summary>
    /// <param name="assembly"></param>
    /// <param name="folder"></param>
    /// <param name="fileName"></param>
    /// <returns></returns>
    public static string? ReadResourceAsString(Assembly assembly, string folder, string fileName)
    {
        using var stream = ReadResource(assembly, folder, fileName);
        if (stream == null) return null;
        using var streamReader = new StreamReader(stream);
        return streamReader.ReadToEnd();
    }
}