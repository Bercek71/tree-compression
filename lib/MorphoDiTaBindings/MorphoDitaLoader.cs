using System.Runtime.InteropServices;
using System.IO;
using System.Net.Mime;

namespace MorphoDita;

public static class MorphoDitaLoader
{
    private static IntPtr _nativeLibHandle;

    public static void LoadNativeLibrary()
    {
        // Získání základní cesty (adresář výstupu)

        var basePath = AppDomain.CurrentDomain.BaseDirectory;

        // Určení názvu knihovny podle platformy
        string libName;

        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            libName = "libmorphodita_csharp.dylib";
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            libName = "morphodita_csharp.dll";
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            libName = "libmorphodita_csharp.so";
        }
        else
        {
            throw new PlatformNotSupportedException("Unsupported platform.");
        }

        var libPath = Path.Combine(basePath, libName);

        Console.WriteLine(libPath);
        // Kontrola existence souboru
        if (!File.Exists(libPath))
        {
            throw new FileNotFoundException($"Native library not found: {libPath}");
        }
        
        // Načtení knihovny
        _nativeLibHandle = NativeLibrary.Load(libPath);
        if (_nativeLibHandle == IntPtr.Zero)
        {
            throw new Exception($"Failed to load {libName}");
        }
    }
}