using System;
using System.Collections.Generic;
using System.Text;
using SkyVM.InterfaceModule;

namespace SkyVM.IOModule
{
    /// <summary>
    /// 网络控制器
    /// </summary>
    public class NetUnit : INetControl
    {  
        /// <summary>
        /// 空闲连接号表
        /// </summary>
        public SortedList<int, ushort> FreeConnNumberTable { get; }
        /// <summary>
        /// 网络连接表
        /// </summary>
        public Dictionary<int, Net_Parameter> ConnectionTable { get; }
        private readonly INetConnect NetControl;
        

        /// <summary>
        /// 网络组件构造函数
        /// </summary>
        /// <param name="net_control">网络连接底层接口实例</param>
        /// <param name="interrupt">网络控制器中断接口</param>
        public NetUnit(INetConnect net_control, INetControlInterrupt interrupt) : base(interrupt)
        {
            ConnectionTable = new Dictionary<int, Net_Parameter>();
            FreeConnNumberTable = new SortedList<int, ushort>()
            {
                {0, (ushort)short.MaxValue }
            };
            SetConnectID(0);
            NetControl = net_control;
        }
        /// <summary>
        /// 建立网络连接
        /// </summary>
        /// <param name="ip_address">对端IP地址</param>
        /// <param name="ip_port">对端IP端口</param>
        /// <param name="ip_type">IP地址类型</param>
        /// <param name="protocol_type">连接协议类型</param>
        /// <param name="connection_model">连接模式</param>
        public override int CreateConnection(string ip_address, int ip_port, IP_Type ip_type, Protocol_Type protocol_type, Connection_Model connection_model)
        {
            //创建value对象
            int id = GetConnectID();
            if (-1 != id)
                ConnectionTable.Add(id, new Net_Parameter
                {
                    ip_address = ip_address,
                    ip_port = ip_port,
                    ip_type = ip_type,
                    protocol_type = protocol_type,
                    connection_model = connection_model,
                    //调用底层接口创建一个连接的实例
                    Connection = NetControl.CreateConnect(ip_address, ip_port, ip_type, protocol_type, connection_model)
                });
            return id;
        }
        /// <summary>
        /// 获取网络连接端口号
        /// </summary>
        /// <param name="connection_id">网络连接编号</param>
        /// <param name="port_type">获取的端口类型</param>
        /// <returns>返回端口号</returns>
        public override int GetPort(int connection_id, Port_Type port_type)
        {
            if (ConnectionTable.TryGetValue(connection_id, out Net_Parameter parameter))
                return parameter.Connection.GetPort(port_type);
            return 0;
        }
        /// <summary>
        /// 暂停网络连接
        /// </summary>
        /// <param name="connection_id">网络连接编号</param>
        public override void PauseConnection(int connection_id)
        {
            if (ConnectionTable.TryGetValue(connection_id, out Net_Parameter parameter))
                parameter.Connection.Pause();
        }
        /// <summary>
        /// 接收数据
        /// </summary>
        /// <param name="connection_id">网络连接编号</param>
        /// <returns>返回数据</returns>
        public override byte[] Receive(int connection_id)
        {
            if (ConnectionTable.TryGetValue(connection_id, out Net_Parameter parameter))
                return parameter.Connection.Receive();
            return null;
        }
        /// <summary>
        /// 复位网络连接
        /// </summary>
        /// <param name="connection_id">网络连接编号</param>
        public override void ResetConnection(int connection_id)
        {
            if (ConnectionTable.TryGetValue(connection_id, out Net_Parameter parameter))
                parameter.Connection.Reset();
        }
        /// <summary>
        /// 发送数据
        /// </summary>
        /// <param name="connection_id">网络连接编号</param>
        /// <param name="data">数据体</param>
        public override void Send(int connection_id, byte[] data)
        {
            if (ConnectionTable.TryGetValue(connection_id, out Net_Parameter parameter))
                parameter.Connection.Send(data);
        }
        /// <summary>
        /// 终止网络连接
        /// </summary>
        /// <param name="connection_id">网络连接编号</param>
        public override void StopConnection(int connection_id)
        {
            if (ConnectionTable.TryGetValue(connection_id, out Net_Parameter parameter))
            {
                parameter.Connection.Stop();
                FreeConnectID((ushort)connection_id);
                ConnectionTable.Remove(connection_id);
            }
        }
        /// <summary>
        /// 网络连接中断接口
        /// </summary>
        /// <param name="exception">网络连接中断类型</param>
        /// <param name="connection_id">连接ID</param>
        public override void Interrupt(NetException_Type exception, int connection_id)
        {
            switch(exception)
            {
                case NetException_Type.Connection_Exist:
                case NetException_Type.Create_Error:
                case NetException_Type.ReConnection_Err:
                case NetException_Type.Send_Failed:
                    if (ConnectionTable.TryGetValue(connection_id, out Net_Parameter parameter))
                    {
                        string str = "";
                        for (int i = 0; i < parameter.ip_address.Length; i++)
                            str += string.Format("{0:X}.", parameter.ip_address[i]);
                        NCInterrupt.Interrupt(exception, connection_id, str, parameter.ip_port);
                    }
                    break;
            }
        }

        

        private int GetConnectID()
        {
            int result = -1;

            if (0 < FreeConnNumberTable.Count)
            {
                int key = FreeConnNumberTable.Keys[0];
                ushort value = FreeConnNumberTable[key];
                FreeConnNumberTable.Remove(key);
                result = key;
                key++;
                if (key <= value)
                    FreeConnNumberTable.Add(key, value);
            }

            return result;
        }
        private bool SetConnectID(ushort interrupt_id)
        {
            bool result = false;
            for (int i = 0; i < FreeConnNumberTable.Count; i++)
            {
                int key = FreeConnNumberTable.Keys[i];
                ushort value = FreeConnNumberTable[key];
                if (interrupt_id >= key && interrupt_id <= value)
                {
                    FreeConnNumberTable.Remove(key);
                    if (interrupt_id - 1 >= key)
                        FreeConnNumberTable.Add(key, (ushort)(interrupt_id - 1));
                    if (interrupt_id + 1 <= value)
                        FreeConnNumberTable.Add((ushort)(interrupt_id + 1), value);
                    result = true;
                    break;
                }
                else if (interrupt_id < key)
                    break;
            }
            return result;
        }
        private void FreeConnectID(ushort interrupt_id)
        {
            if (!FreeConnNumberTable.ContainsKey(interrupt_id))
            {
                FreeConnNumberTable.Add(interrupt_id, interrupt_id);
                for (int i = 0; i < FreeConnNumberTable.Count - 1; i++)
                {
                    int key = FreeConnNumberTable.Keys[i];
                    ushort value = FreeConnNumberTable.Values[i];
                    if (value + 1 == FreeConnNumberTable.Keys[i + 1])
                    {
                        value = FreeConnNumberTable.Values[i + 1];
                        int k2 = FreeConnNumberTable.Keys[i + 1];
                        FreeConnNumberTable.Remove(key);
                        FreeConnNumberTable.Remove(k2);
                        FreeConnNumberTable.Add(key, value);
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// 网络数据包
        /// </summary>
        public class Net_Parameter
        {
            /// <summary>
            /// 连接IP地址
            /// </summary>
            public string ip_address;
            /// <summary>
            /// 连接端口
            /// </summary>
            public int ip_port;
            /// <summary>
            /// IP地址类型
            /// </summary>
            public IP_Type ip_type;
            /// <summary>
            /// 
            /// </summary>
            public Protocol_Type protocol_type;
            /// <summary>
            /// 连接模式
            /// </summary>
            public Connection_Model connection_model;
            /// <summary>
            /// 连接主体
            /// </summary>
            public INetConnection Connection;
        }
    }
}
