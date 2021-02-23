using System;
using System.Collections.Generic;
using System.Text;

namespace SkyVM.InterfaceModule
{
    /// <summary>
    /// 输入输出控制器接口
    /// </summary>
    public abstract class IDeviceControl : IDevice
    {
        /// <summary>
        /// 输入输出控制器接口构造函数
        /// </summary>
        /// <param name="interrupt">设备控制器中断接口</param>
        public IDeviceControl(IDeviceControlInterrupt interrupt) : base(interrupt) { }

        /// <summary>
        /// 添加一个设备-创建时加载（虚拟机内部不可动态调整）
        /// </summary>
        /// <param name="interrupt_id">设备中断号</param>
        /// <param name="device">设备实例</param>
        public abstract void AddDevice(int interrupt_id, IOBase device);
        /// <summary>
        /// 删除一个设备
        /// </summary>
        /// <param name="device_id">设备号</param>
        public abstract void DeleteDevice(int device_id);
        /// <summary>
        /// 获取所有设备信息
        /// </summary>
        /// <returns>返回所有设备</returns>
        public abstract object[] GetDevices();
        /// <summary>
        /// 获取指定中断号设备信息
        /// </summary>
        /// <param name="interrupt_id">设备中断号</param>
        /// <returns>返回设备信息</returns>
        public abstract object GetDevice(int interrupt_id);
        /// <summary>
        /// 设置新的设备中断号
        /// </summary>
        /// <param name="device_id">设备号</param>
        /// <param name="new_interrupt">中断号</param>
        public abstract void SetInterrupt(int device_id, int new_interrupt);
        /// <summary>
        /// 添加指定设备的中断服务程序
        /// </summary>
        /// <param name="interrupt_id">设备中断号</param>
        /// <param name="programs">中断服务程序</param>
        public abstract void AddInterruptService(int interrupt_id, params string[] programs);
        /// <summary>
        /// 删除指定设备的指定终端服务程序
        /// </summary>
        /// <param name="interrupt_id">设备中断号</param>
        /// <param name="program">中断服务程序</param>
        public abstract void DelInterruptService(int interrupt_id, string program);
        /// <summary>
        /// 清空指定设备的中断服务程序列表
        /// </summary>
        /// <param name="interrupt_id">设备中断号</param>
        public abstract void CleanInterruptService(int interrupt_id);
    }
    /// <summary>
    /// IO设备接口
    /// </summary>
    public abstract class IDevice
    {
        /// <summary>
        /// 网络控制器中断接口
        /// </summary>
        protected IDeviceControlInterrupt ControlInterrupt;

        /// <summary>
        /// IO设备接口构造函数
        /// </summary>
        /// <param name="interrupt">设备控制器中断</param>
        public IDevice(IDeviceControlInterrupt interrupt)
        {
            ControlInterrupt = interrupt;
        }

        /// <summary>
        /// 载入一个设备-运行时加载（由虚拟机内部运行时代码加载）
        /// </summary>
        /// <param name="interrupt_id">设备中断号</param>
        /// <param name="device">设备接口</param>
        public abstract void LoadDevice(int interrupt_id, IOBase device);
        /// <summary>
        /// 卸载一个设备
        /// </summary>
        /// <param name="interrupt_id">设备中断号</param>
        public abstract void UnloadDevice(int interrupt_id);
        /// <summary>
        /// 激活设备
        /// </summary>
        /// <param name="interrupt_id">设备中断号</param>
        public abstract void DeviceActive(int interrupt_id);
        /// <summary>
        /// 暂停设备
        /// </summary>
        /// <param name="interrupt_id">设备中断号</param>
        public abstract void DevicePause(int interrupt_id);
        /// <summary>
        /// 复位设备
        /// </summary>
        /// <param name="interrupt_id">设备中断号</param>
        public abstract void DeviceReset(int interrupt_id);
        /// <summary>
        /// 关闭设备
        /// </summary>
        /// <param name="interrupt_id">设备中断号</param>
        public abstract void DeviceClose(int interrupt_id);
        /// <summary>
        /// 读设备
        /// </summary>
        /// <param name="interrupt_id">设备中断号</param>
        /// <returns>返回数据</returns>
        public abstract byte[] DeviceRead(int interrupt_id);
        /// <summary>
        /// 写设备
        /// </summary>
        /// <param name="interrupt_id">设备中断号</param>
        /// <param name="data">写入数据</param>
        public abstract void DeviceWrite(int interrupt_id, byte[] data);
    }

    /// <summary>
    /// 设备状态标识
    /// </summary>
    public enum Device_State
    {
        /// <summary>
        /// 设备处于关闭状态
        /// </summary>
        Closed,
        /// <summary>
        /// 设备正在运行
        /// </summary>
        Running,
        /// <summary>
        /// 设备已暂停
        /// </summary>
        Paused,
        /// <summary>
        /// 设备运行异常
        /// </summary>
        Anomalous,
    }
}
