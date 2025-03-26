using System.Runtime.InteropServices;

namespace UDPipeBindings;

/// <summary>
/// Slouží k načtení nativní knihovny pro UDPipe podle platformy.
/// </summary>
public static class UdPipeLoader
{
    private static IntPtr _nativeLibHandle;

    public static void LoadNativeLibrary()
    {
        // Získání základní cesty (adresář výstupu)

        var basePath = AppDomain.CurrentDomain.BaseDirectory;

        // Určení názvu knihovny podle platformy
        string libName;

        // Určení názvu knihovny podle platformy
        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            libName = "libudpipe_csharp.dylib";
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            libName = "udpipe_csharp.dll";
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            libName = "libudpipe_csharp.so";
        }
        else
        {
            throw new PlatformNotSupportedException("Unsupported platform.");
        }

        var libPath = Path.Combine(basePath, libName);

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