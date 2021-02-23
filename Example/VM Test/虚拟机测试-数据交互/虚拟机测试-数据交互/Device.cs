using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SkyVM.InterfaceModule;

namespace 虚拟机测试_数据交互
{
    /// <summary>
    /// IO设备实现类：用于模拟虚拟机IO设备
    /// </summary>
    class Device : IOBase
    {
        public Device(IDeviceControlInterrupt interrupt) : base(interrupt)
        {
            Property = new DeviceProperty
            {
                DeviceID = "123456789",
                DeviceName = "TEST",
                DeviceDescription = "Test Divice"
            };
        }

        public void Send(string str)
        {
            byte[] vs = new byte[Encoding.Unicode.GetByteCount(str) + 4];
            byte[] temp = BitConverter.GetBytes((vs.Length - 4) / 2 + 1);
            for (int i = 0; i < temp.Length; i++)
                vs[i] = temp[i];
            temp = Encoding.Unicode.GetBytes(str);
            for (int i = 0; i < temp.Length; i++)
                vs[i + 4] = temp[i];
            OutPutQueue.Enqueue(vs);
        }
        public string Receive()
        {
            if (InPutQueue.Count > 0)
            {
                return Encoding.Unicode.GetString(InPutQueue.Dequeue());
            }
            return null;
        }

        public override void Pause()
        {
            throw new NotImplementedException();
        }

        public override void Reset()
        {
            throw new NotImplementedException();
        }

        public override void Run()
        {
            throw new NotImplementedException();
        }

        public override void Stop()
        {
            throw new NotImplementedException();
        }
    }
}
