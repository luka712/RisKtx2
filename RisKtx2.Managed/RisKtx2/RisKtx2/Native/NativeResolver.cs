using System.Reflection;
using System.Runtime.InteropServices;

namespace RisKtx2.Native
{
    public static class NativeResolver
    {
        private const string BaseName = "ris_ktx2";

        private static bool _isSetup = false;

        // Call this once at startup before any native calls
        public static void Setup()
        {
            if (_isSetup)
            {
                return; // already set up
            }
            _isSetup = true;
            NativeLibrary.SetDllImportResolver(typeof(NativeResolver).Assembly, Resolve);
        }

        private static IntPtr Resolve(
            string libraryName, 
            Assembly assembly,
            DllImportSearchPath? searchPath)
        {
            if (libraryName != BaseName)
                return IntPtr.Zero; // only handle our library

            string rid = GetRuntimeIdentifier();
            string ext = OperatingSystem.IsWindows() ? ".dll" :
                         OperatingSystem.IsMacOS() ? ".dylib" : ".so";


            string path = Path.Combine(AppContext.BaseDirectory, "runtimes", rid, "native", BaseName + ext);

            if (!File.Exists(path))
            {
                path = Path.Combine(AppContext.BaseDirectory, "runtimes", rid, "native", "lib" + BaseName + ext);
            }

            if (!File.Exists(path))
            {
                throw new DllNotFoundException($"Native library not found: {path}");
            }

            return NativeLibrary.Load(path);
        }

        private static string GetRuntimeIdentifier()
        {
            string os = OperatingSystem.IsWindows() ? "win" :
                        OperatingSystem.IsLinux() ? "linux" :
                        OperatingSystem.IsMacOS() ? "osx" : throw new PlatformNotSupportedException();

            string arch = RuntimeInformation.ProcessArchitecture switch
            {
                Architecture.X64 => "x64",
                Architecture.X86 => "x86",
                Architecture.Arm64 => "arm64",
                Architecture.Arm => "arm",
                _ => "unknown"
            };

            return $"{os}-{arch}";
        }
    }
}
