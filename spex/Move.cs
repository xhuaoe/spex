using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace spex
{
    public class Move
    {
        [DllImport("move.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.Cdecl)]
        private extern static void SetSpeed();

        [DllImport("move.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.Cdecl)]
        private extern static void SetSteps2Start([MarshalAs(UnmanagedType.I4)]int step);

        [DllImport("move.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.Cdecl, EntryPoint = "Progress")]
        private extern static int prg();

        [DllImport("move.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.Cdecl, EntryPoint = "Stop")]
        private extern static void stop();

        public static void Init()
        {
            SetSpeed();
        }

        public static void MoveStep(double step)
        {
            SetSteps2Start((int)(step * 400 + 0.5));
        }

        public static double Progress()
        {
            return prg() / 400.0;
        }

        public static void Stop()
        {
            stop();
        }
    }
}
