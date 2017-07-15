using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace spex
{
    public class Shutter
    {
        const ushort port = 0x37a;

        [DllImport("Shutter.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.Cdecl, EntryPoint = "close")]
        private extern static void _close([MarshalAs(UnmanagedType.U2)]ushort port);

        [DllImport("Shutter.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.Cdecl, EntryPoint = "open")]
        private extern static void _open([MarshalAs(UnmanagedType.U2)]ushort port);

        public static void Close()
        {
            _close(port);
        }

        public static void Open()
        {
            _open(port);
        }
    }
}
