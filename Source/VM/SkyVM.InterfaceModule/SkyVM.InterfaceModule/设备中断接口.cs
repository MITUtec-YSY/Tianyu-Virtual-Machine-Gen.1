using System;
using System.Collections.Generic;
using System.Text;

namespace SkyVM.InterfaceModule
{
    /// <summary>
    /// 内存控制器中断接口
    /// </summary>
    public interface IMemoryControlInterrupt
    {
        /// <summary>
        /// 中断接口函数
        /// </summary>
        /// <param name="exception">内存中断类型</param>
        /// <param name="address">内存地址</param>
        /// <param name="size">访问大小</param>
        void Interrupt(MemoryControl_Type exception, long address, int size);
    }
    /// <summary>
    /// 内存缓存器中断接口
    /// </summary>
    public interface IMemoryCacheInterrupt
    {
        /// <summary>
        /// 中断接口函数
        /// </summary>
        /// <param name="exception">内存中断类型</param>
        /// <param name="address">内存地址</param>
        /// <param name="size">访问大小</param>
        void Interrupt(MemoryCache_Type exception, long address, int size);
        /// <summary>
        /// 缓存缺页中断函数
        /// </summary>
        /// <param name="address">内存地址</param>
        /// <param name="realize">实际大小</param>
        /// <returns>返回内存块</returns>
        MemoryBlock PageMissingInterrupt(long address, out long realize);
    }
    /// <summary>
    /// 设备中断接口
    /// </summary>
    public interface IDeviceInterrupt
    {
        /// <summary>
        /// 中断接口函数
        /// </summary>
        /// <param name="exception">设备中断类型</param>
        /// <param name="device_label">设备ID</param>
        void Interrupt(DeviceException_Type exception, string device_label);
    }
    /// <summary>
    /// 设备控制器中断接口
    /// </summary>
    public interface IDeviceControlInterrupt
    {
        /// <summary>
        /// 设备控制器中断接口函数
        /// </summary>
        /// <param name="exception">设备中断类型</param>
        /// <param name="device_id">设备ID</param>
        /// <param name="device">设备基础信息</param>
        void Interrupt(DeviceException_Type exception, int device_id, IOBase device);
    }
    /// <summary>
    /// 网络连接中断接口
    /// </summary>
    public interface INetInterrupt
    {
        /// <summary>
        /// 网络连接中断接口函数
        /// </summary>
        /// <param name="exception">网络异常类型</param>
        /// <param name="connection_id">连接ID</param>
        void Interrupt(NetException_Type exception, int connection_id);
    }
    /// <summary>
    /// 网络控制器中断接口
    /// </summary>
    public interface INetControlInterrupt
    {
        /// <summary>
        /// 网络控制器中断接口函数
        /// </summary>
        /// <param name="exception">网络异常类型</param>
        /// <param name="connection_id">连接ID</param>
        /// <param name="ip">连接IP地址</param>
        /// <param name="port">连接IP端口</param>
        void Interrupt(NetException_Type exception, int connection_id, string ip, int port);
    }

    /// <summary>
    /// 内存异常类型-控制器
    /// </summary>
    public enum MemoryControl_Type
    {
        /// <summary>
        /// 内存满异常
        /// </summary>
        Mem_Full,
        /// <summary>
        /// 时间异常
        /// </summary>
        Time_Error,
        /// <summary>
        /// 内存溢出
        /// </summary>
        Over_Flow,
        /// <summary>
        /// 内存越界
        /// </summary>
        Out_Bounds,
    }
    /// <summary>
    /// 内存异常类型-缓存
    /// </summary>
    public enum MemoryCache_Type
    {
        /// <summary>
        /// 内存溢出
        /// </summary>
        Over_Flow,
        /// <summary>
        /// 内存越界
        /// </summary>
        Out_Bounds,
    }
    /// <summary>
    /// 网络异常类型
    /// </summary>
    public enum NetException_Type
    {
        /// <summary>
        /// 网络连接已存在
        /// </summary>
        Connection_Exist,
        /// <summary>
        /// 建立连接失败
        /// </summary>
        Create_Error,
        /// <summary>
        /// 重连失败
        /// </summary>
        ReConnection_Err,
        /// <summary>
        /// 数据发送失败
        /// </summary>
        Send_Failed,
    }

    /// <summary>
    /// 设备异常类型
    /// </summary>
    public enum DeviceException_Type
    {
        /// <summary>
        /// 设备添加失败
        /// </summary>
        Add_Failed,
        /// <summary>
        /// 设备不存在（设备不在设备表中）
        /// </summary>
        Not_Exist,
        /// <summary>
        /// 设备离线（关闭或卸载）
        /// </summary>
        Off_Line,
        /// <summary>
        /// 设备异常
        /// </summary>
        Work_Failed,
        /// <summary>
        /// 中断异常
        /// </summary>
        Interrupt_Error,
    }
}
