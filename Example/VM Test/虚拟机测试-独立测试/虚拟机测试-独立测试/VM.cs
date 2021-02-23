using SkyVM.CoreModule.Complexs;
using SkyVM.ExModule;
using SkyVM.InterfaceModule;
using SkyVM.IOModule;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace 虚拟机测试_独立测试
{
    class VM : IApplication, IDeviceControlInterrupt, INetControlInterrupt
    {
        void IApplication.NetCreateFailed(int process)
        {
            Console.WriteLine("程序：" + process + " 网络连接失败");
        }

        void IApplication.Print(string str)
        {
            Console.WriteLine(str);
        }

        byte[] IApplication.Scan(long size)
        {
            char[] vs = new char[size];
            for (int i = 0; i < vs.Length; i++)
                vs[i] = (char)Console.Read();
            return Encoding.Unicode.GetBytes(new string(vs));
        }

        void IApplication.ShutDown()
        {
            Console.WriteLine();
            Console.WriteLine("虚拟机结束");
            Console.ReadLine();
            System.Diagnostics.Process.GetCurrentProcess().Close();
        }

        void IDeviceControlInterrupt.Interrupt(DeviceException_Type exception, int device_id, IOBase device)
        {
            Console.WriteLine("设备控制器：" + exception.ToString() + " " + device_id);
        }

        void INetControlInterrupt.Interrupt(NetException_Type exception, int connection_id, string ip, int port)
        {
            Console.WriteLine("网络控制器：" + exception.ToString() + " " + connection_id + " " + ip + " " + port);
        }

        public static long MemorySize = 1024 * 1024 * 8;
        public static int MemoryAline = 16;
        public static int MemoryOffset = 16;
        public static int MemoryMinSize = 32;
        public static int CacheSize = 3;
        public static int CoreCount = 0;
        public static int StackSize = 100;
    }
}
