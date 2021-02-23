using SkyVM.InterfaceModule;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace 虚拟机测试_独立测试
{
    class Device : IOBase
    {
        public Device(IDeviceControlInterrupt interrupt) : base(interrupt)
        {
            Property = new DeviceProperty
            {
                DeviceID = "0x1111",
                DeviceName = "测试设备",
                DeviceDescription = ""
            };

        }

        public new byte[] Get()
        {
            return Encoding.Unicode.GetBytes("测试数据-输入");
        }

        public new void Set(byte[] data)
        {
            Console.WriteLine(Encoding.Unicode.GetString(data));
        }

        public override void Pause()
        {
            Console.WriteLine("暂停设备" + Property.DeviceID);
        }

        public override void Reset()
        {
            Console.WriteLine("复位设备" + Property.DeviceID);
        }

        public override void Run()
        {
            Console.WriteLine("启动设备" + Property.DeviceID);
        }

        public override void Stop()
        {
            Console.WriteLine("关闭设备" + Property.DeviceID);
        }
    }
}
