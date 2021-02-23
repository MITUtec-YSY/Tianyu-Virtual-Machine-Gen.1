using System;
using System.Collections.Generic;
using System.Text;

namespace SkyVM.InterfaceModule
{
    /// <summary>
    /// 网络控制器接口
    /// </summary>
    public abstract class INetControl : INetInterrupt
    {
        /// <summary>
        /// 网络控制器中断接口
        /// </summary>
        protected readonly INetControlInterrupt NCInterrupt;

        /// <summary>
        /// 网络控制器接口
        /// </summary>
        /// <param name="interrupt">网络控制器中断</param>
        public INetControl(INetControlInterrupt interrupt)
        {
            NCInterrupt = interrupt;
        }

        /// <summary>
        /// 建立网络连接
        /// </summary>
        /// <param name="ip_address">对端IP地址</param>
        /// <param name="ip_port">对端IP端口</param>
        /// <param name="ip_type">IP地址类型</param>
        /// <param name="protocol_type">连接协议类型</param>
        /// <param name="connection_model">连接模式</param>
        /// <returns>返回连接号</returns>
        public abstract int CreateConnection(string ip_address, int ip_port, IP_Type ip_type, Protocol_Type protocol_type, Connection_Model connection_model);
        /// <summary>
        /// 暂停网络连接
        /// </summary>
        /// <param name="connection_id">网络连接编号</param>
        public abstract void PauseConnection(int connection_id);
        /// <summary>
        /// 复位网络连接
        /// </summary>
        /// <param name="connection_id">网络连接编号</param>
        public abstract void ResetConnection(int connection_id);
        /// <summary>
        /// 终止网络连接
        /// </summary>
        /// <param name="connection_id">网络连接编号</param>
        public abstract void StopConnection(int connection_id);
        /// <summary>
        /// 获取网络连接端口号
        /// </summary>
        /// <param name="connection_id">网络连接编号</param>
        /// <param name="port_type">获取的端口类型</param>
        /// <returns>返回端口号</returns>
        public abstract int GetPort(int connection_id, Port_Type port_type);
        /// <summary>
        /// 发送数据
        /// </summary>
        /// <param name="connection_id">网络连接编号</param>
        /// <param name="data">数据体</param>
        public abstract void Send(int connection_id, byte[] data);
        /// <summary>
        /// 接收数据
        /// </summary>
        /// <param name="connection_id">网络连接编号</param>
        /// <returns>返回数据</returns>
        public abstract byte[] Receive(int connection_id);

        /// <summary>
        /// 网络连接中断接口
        /// </summary>
        /// <param name="exception">网络连接中断类型</param>
        /// <param name="connection_id">连接ID</param>
        public abstract void Interrupt(NetException_Type exception, int connection_id);
    }

    /// <summary>
    /// IP地址类型
    /// </summary>
    public enum IP_Type
    {
        /// <summary>
        /// 第四代IP地址
        /// </summary>
        IPv4,
        /// <summary>
        /// 第六代IP地址
        /// </summary>
        IPv6,
    }
    /// <summary>
    /// 协议类型
    /// </summary>
    public enum Protocol_Type
    {
        /// <summary>
        /// 传输控制协议/网际协议
        /// </summary>
        TCP_IP,
        /// <summary>
        /// 用户数据报协议
        /// </summary>
        UDP,
    }
    /// <summary>
    /// 连接类型
    /// </summary>
    public enum Connection_Model
    {
        /// <summary>
        /// 服务器
        /// </summary>
        Server,
        /// <summary>
        /// 客户端
        /// </summary>
        Client,
        /// <summary>
        /// 混合模式
        /// </summary>
        Mix,
    }
    /// <summary>
    /// 端口类型
    /// </summary>
    public enum Port_Type
    {
        /// <summary>
        /// 服务器端口
        /// </summary>
        Server,
        /// <summary>
        /// 客户端端口
        /// </summary>
        Client,
        /// <summary>
        /// 自动端口
        /// </summary>
        Auto,
    }
}
