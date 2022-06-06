using System;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace LsassMemShim
{
    public static class Pipe
    {
        static String PipeName = "lsasspipe";
        [DllImport("kernel32")] static extern IntPtr OpenProcess(int dwDesiredAccess, bool bInheritHandle, int dwProcessId);
        [DllImport("kernel32")] static extern Int32 ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, [Out] byte[] lpBuffer, int dwSize, out IntPtr lpNumberOfBytesRead);
        [DllImport("kernel32")] static extern Boolean WriteProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, Byte[] buffer, Int32 nSize, out Int32 lpNumberOfBytesWritten);
        enum Command : Byte
        {
            OpenProcess,
            ReadProcessMemory,
            WriteProcessMemory,
            Close
        }
        static IntPtr handle = IntPtr.Zero;
        public static void StartPipe()
        {
            Task.Factory.StartNew(() =>
            {
                var server = new NamedPipeServerStream(PipeName, PipeDirection.InOut, 1, PipeTransmissionMode.Byte);
                while (true)
                {
                    server.WaitForConnection();
                    while (true)
                    {
                        var cmd = new Byte[1];
                        server.Read(cmd, 0, 1);
                        if ((Command)cmd[0] == Command.OpenProcess)
                        {
                            var buf = new Byte[0x100];
                            var procNameLength = server.Read(buf, 0, buf.Length);
                            var procName = Encoding.UTF8.GetString(buf, 0, procNameLength);
                            var procs = Process.GetProcessesByName(procName);
                            server.Write(BitConverter.GetBytes(procs.Length), 0, 4);
                            foreach (var p in procs)
                            {
                                handle = OpenProcess(0x479, true, p.Id);
                                server.Write(BitConverter.GetBytes((UInt64)p.MainModule.BaseAddress), 0, 8);
                                break;
                            }
                        }
                        if ((Command)cmd[0] == Command.ReadProcessMemory)
                        {
                            var buf = new Byte[0x100];
                            server.Read(buf, 0, buf.Length);
                            var addr = (IntPtr)BitConverter.ToUInt64(buf, 0);
                            server.Read(buf, 0, buf.Length);
                            var len = BitConverter.ToInt32(buf, 0);
                            buf = new Byte[len];
                            ReadProcessMemory(handle, addr, buf, len, out _);
                            server.Write(buf, 0, len);
                        }
                        if ((Command)cmd[0] == Command.WriteProcessMemory)
                        {
                            var buf = new Byte[0x100];
                            server.Read(buf, 0, buf.Length);
                            var addr = (IntPtr)BitConverter.ToUInt64(buf, 0);
                            server.Read(buf, 0, buf.Length);
                            var len = BitConverter.ToInt32(buf, 0);
                            buf = new Byte[len];
                            server.Read(buf, 0, buf.Length);
                            WriteProcessMemory(handle, addr, buf, len, out _);
                        }
                        if ((Command)cmd[0] == Command.Close)
                        {
                            break;
                        }
                    }
                    server.Disconnect();
                }
            });
        }
        static NamedPipeClientStream client;
        static NamedPipeClientStream Client
        {
            get
            {
                if (client == null)
                {
                    client = new NamedPipeClientStream(".", PipeName, PipeDirection.InOut);
                    client.Connect(1000);
                }
                return client;
            }
        }
        public static Boolean LsassModuleLoaded()
        {
            if (File.Exists(AppDomain.CurrentDomain.FriendlyName.Replace(".exe", ".dll")))
            {
                try
                {
                    File.Delete(AppDomain.CurrentDomain.FriendlyName.Replace(".exe", ".dll"));
                }
                catch
                {
                    return true;
                }
            }
            return false;
        }
        public static void OpenProcess(String procName, out UInt64 baseAddr)
        {
            Client.Write(new Byte[1] { (Byte)Command.OpenProcess }, 0, 1);
            Client.Write(Encoding.UTF8.GetBytes(procName), 0, procName.Length);
            var buf = new Byte[0x100];
            var procCountSize = Client.Read(buf, 0, buf.Length);
            var procCount = BitConverter.ToUInt64(buf, 0);
            if (procCount > 0)
            {
                var addrSize = Client.Read(buf, 0, buf.Length);
                baseAddr = BitConverter.ToUInt64(buf, 0);
            }
            else baseAddr = 0;
        }
        public static Byte[] ReadProcessMemory(UInt64 address, Int32 length)
        {
            Client.Write(new Byte[1] { (Byte)Command.ReadProcessMemory }, 0, 1);
            Client.Write(BitConverter.GetBytes(address), 0, 8);
            Client.Write(BitConverter.GetBytes(length), 0, 4);
            var buf = new Byte[length];
            var handleSize = Client.Read(buf, 0, buf.Length);
            return buf;
        }
        public static void WriteProcessMemory(UInt64 address, Byte[] value)
        {
            Client.Write(new Byte[1] { (Byte)Command.WriteProcessMemory }, 0, 1);
            Client.Write(BitConverter.GetBytes(address), 0, 8);
            Client.Write(BitConverter.GetBytes(value.Length), 0, 4);
            Client.Write(value, 0, value.Length);
        }
        public static void Close()
        {
            Client.Write(new Byte[1] { (Byte)Command.Close }, 0, 1);
            Client.Dispose();
        }
    }
}
