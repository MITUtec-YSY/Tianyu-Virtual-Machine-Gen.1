using SkyVM.InterfaceModule;
using System;
using System.Collections.Generic;
using System.Text;

namespace SkyVM.IOModule
{
    /// <summary>
    /// 网络连接底层接口
    /// </summary>
    public interface INetConnect
    {
        /// <summary>
        /// 创建一个链接
        /// </summary>
        /// <param name="ip_address">ip_address</param>
        /// <param name="ip_port">ip_port</param>
        /// <param name="ip_type">ip_type</param>
        /// <param name="protocol_type">protocol_type</param>
        /// <param name="connection_model">connection_model</param>
        /// <returns>返回一个链接实例</returns>
        INetConnection CreateConnect(string ip_address, int ip_port, IP_Type ip_type, Protocol_Type protocol_type, Connection_Model connection_model);
    }
    /// <summary>
    /// 网络链接基类
    /// </summary>
    public abstract class INetConnection
    {
        /// <summary>
        /// 网络链接中断接口
        /// </summary>
        protected readonly INetInterrupt Interrupt;

        /// <summary>
        /// 网络链接基类构造函数
        /// </summary>
        /// <param name="interrupt">中断接口</param>
        public INetConnection(INetInterrupt interrupt)
        {
            Interrupt = interrupt;
        }

        /// <summary>
        /// 发送数据
        /// </summary>
        /// <param name="data">数据</param>
        public abstract void Send(byte[] data);
        /// <summary>
        /// 接收数据
        /// </summary>
        /// <returns>返回数据</returns>
        public abstract byte[] Receive();
        /// <summary>
        /// 暂停连接
        /// </summary>
        public abstract void Pause();
        /// <summary>
        /// 重置连接
        /// </summary>
        public abstract void Reset();
        /// <summary>
        /// 断开连接
        /// </summary>
        public abstract void Stop();
        /// <summary>
        /// 获取连接的指定接口
        /// </summary>
        /// <param name="port_type">端口类型</param>
        /// <returns>返回端口号</returns>
        public abstract short GetPort(Port_Type port_type);
    }
}
