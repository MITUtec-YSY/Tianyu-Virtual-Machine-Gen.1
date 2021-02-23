using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SkyVM;
using SkyVM.InterfaceModule;
using SkyVM.IOModule;
using SkyVM.CoreModule.Complexs;

namespace 虚拟机测试_数据交互
{
    /// <summary>
    /// 用于输出模块的回调接口
    /// </summary>
    interface FormShow
    {
        void Show(string str);
    }
    /// <summary>
    /// 虚拟机实现类
    /// </summary>
    class VisualMachine : VMController
    {
        private readonly FormShow Show;

        public VisualMachine(FormShow show) : base(new NetControl(), new ComplexAU())
        {
            Show = show;
        }

        /// <summary>
        /// 设备控制器运行时中断回调接口
        /// </summary>
        /// <param name="exception"></param>
        /// <param name="device_id"></param>
        /// <param name="device"></param>
        public override void Interrupt(DeviceException_Type exception, int device_id, IOBase device)
        {
            Show.Show("设备控制器：" + exception.ToString() + " " + device_id);
        }
        /// <summary>
        /// 网络控制器运行时中断回调接口
        /// </summary>
        /// <param name="exception"></param>
        /// <param name="connection_id"></param>
        /// <param name="ip"></param>
        /// <param name="port"></param>
        public override void Interrupt(NetException_Type exception, int connection_id, string ip, int port)
        {
            Show.Show("网络控制器：" + exception.ToString() + " " + connection_id + " " + ip + " " + port);
        }
        /// <summary>
        /// 网络控制器运行时失败回调接口
        /// </summary>
        /// <param name="process"></param>
        public override void NetCreateFailed(int process)
        {
            Show.Show("程序：" + process + " 网络连接失败");
        }
        /// <summary>
        /// 运行时显示中断回调接口
        /// </summary>
        /// <param name="str"></param>
        public override void Print(string str)
        {
            Show.Show(str);
        }
        /// <summary>
        /// 运行时输入中断回调接口
        /// </summary>
        /// <param name="size"></param>
        /// <returns></returns>
        public override byte[] Scan(long size)
        {
            char[] vs = new char[size];
            for (int i = 0; i < vs.Length; i++)
                vs[i] = (char)Console.Read();
            return Encoding.Unicode.GetBytes(new string(vs));
        }
        /// <summary>
        /// 虚拟机结束调用接口
        /// </summary>
        public override void ShutDown()
        {
            Console.WriteLine();
            Show.Show("虚拟机结束");
            Console.ReadLine();
            System.Diagnostics.Process.GetCurrentProcess().Close();
        }

        /// <summary>
        /// 网络控制器实现类
        /// </summary>
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
        /// <summary>
        /// 网络连接对象
        /// </summary>
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
}
