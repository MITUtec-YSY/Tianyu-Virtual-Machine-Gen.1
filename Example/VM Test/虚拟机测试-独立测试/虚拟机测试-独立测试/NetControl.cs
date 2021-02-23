using SkyVM.InterfaceModule;
using SkyVM.IOModule;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace 虚拟机测试_独立测试
{
    class NetControl : INetConnect, INetInterrupt
    {
        public INetConnection CreateConnect(string ip_address, int ip_port, IP_Type ip_type, Protocol_Type protocol_type, Connection_Model connection_model)
        {
            Console.WriteLine("创建连接：{0} {1} {2} {3} {4}", ip_address, ip_port, ip_type.ToString(), protocol_type.ToString(), connection_model.ToString());
            return new Net(this);
        }

        public void Interrupt(NetException_Type exception, int connection_id)
        {
            Console.WriteLine("网络异常：" + exception.ToString() + " " + connection_id);
        }
    }
}
