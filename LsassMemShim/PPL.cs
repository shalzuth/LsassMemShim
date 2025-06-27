using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace LsassMemShim
{
    public class PPL
    {
        public enum ProcessProtectionLevels : uint
        {
            PROTECTION_LEVEL_WINTCB_LIGHT = 0x00000000,
            PROTECTION_LEVEL_WINDOWS = 0x00000001,
            PROTECTION_LEVEL_WINDOWS_LIGHT = 0x00000002,
            PROTECTION_LEVEL_ANTIMALWARE_LIGHT = 0x00000003,
            PROTECTION_LEVEL_LSA_LIGHT = 0x00000004,
            // The following protection levels are supplied for testing only (no win32 callers need these).
            PROTECTION_LEVEL_WINTCB = 0x00000005,
            PROTECTION_LEVEL_CODEGEN_LIGHT = 0x00000006,
            PROTECTION_LEVEL_AUTHENTICODE = 0x00000007,
            PROTECTION_LEVEL_PPL_APP = 0x00000008,
            PROTECTION_LEVEL_SAME = 0xFFFFFFFF,            //
            // The following is only used as a value for ProtectionLevel when querying ProcessProtectionLevelInfo in GetProcessInformation.
            PROTECTION_LEVEL_NONE = 0xFFFFFFFE
        }
        const int ProcessProtectionLevelInfo = 7; // From PROCESS_INFORMATION_CLASS

        [DllImport("kernel32")] static extern bool GetProcessInformation(IntPtr hProcess, int processInformationClass, out uint processInformation, int processInformationSize);
        [DllImport("kernel32")] static extern IntPtr GetCurrentProcess();
        public static ProcessProtectionLevels GetProcessProtectionLevel()
        {
            var res = GetProcessInformation(GetCurrentProcess(), ProcessProtectionLevelInfo, out uint level, sizeof(uint));
            return (ProcessProtectionLevels)level;
        }
    }
}
