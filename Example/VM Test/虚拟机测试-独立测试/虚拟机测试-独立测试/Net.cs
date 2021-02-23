using SkyVM.InterfaceModule;
using SkyVM.IOModule;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace 虚拟机测试_独立测试
{
    class Net : INetConnection
    {
        public Net(INetInterrupt interrupt) : base(interrupt)
        {

        }

        public override short GetPort(Port_Type port_type)
        {
            return 0;
        }

        public override void Pause()
        {
            Console.WriteLine("暂停网络");
        }

        public override byte[] Receive()
        {
            return new byte[2];
        }

        public override void Reset()
        {
            Console.WriteLine("复位");
        }

        public override void Send(byte[] data)
        {
            Console.WriteLine("发送：" + Encoding.Unicode.GetString(data));
        }

        public override void Stop()
        {
            Console.WriteLine("停止");
        }
    }
}
