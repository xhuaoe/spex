using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace spex
{
    public class Ccd
    {
        [DllImport("ccd.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.Cdecl)]
        private extern static bool init();

        [DllImport("ccd.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.Cdecl)]
        private extern static void uninit();

        [DllImport("ccd.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.Cdecl)]
        private extern static Int16 errorCode();

        [DllImport("ccd.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.Cdecl)]
        private extern static Int16 getTemp();

        [DllImport("ccd.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.Cdecl)]
        private extern static void setTemp(Int16 temp);

        [DllImport("ccd.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.Cdecl)]
        private extern static UInt32 setup(UInt32 exp_time, Int16 circ_buff_size, bool spec);

        [DllImport("ccd.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.Cdecl)]
        private extern static bool getFrame(UInt16[] output);

        [DllImport("ccd.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.Cdecl)]
        private extern static void stop();

        public static bool Init()
        {
            return init();
        }

        public static void Uninit()
        {
            uninit();
        }

        public static Int16 ErrorCode()
        {
            return errorCode();
        }

        public static Int16 GetTemp()
        {
            return getTemp();
        }

        public static void SetTemp(Int16 temp)
        {
            setTemp(temp);
        }

        public static UInt32 Setup(bool spec, UInt32 expTime = 100, Int16 buffSize = 5)
        {
            return setup(expTime, buffSize, spec);
        }

        public static bool GetFrame(UInt16[] output)
        {
            return getFrame(output);
        }

        public static void Stop()
        {
            stop();
        }

        public static int ExpoTime = 100;
        public static double Temp = -70;
        public static int ROIFrom = 0;
        public static int ROITo = DataProcessing.Width;
    }
}
