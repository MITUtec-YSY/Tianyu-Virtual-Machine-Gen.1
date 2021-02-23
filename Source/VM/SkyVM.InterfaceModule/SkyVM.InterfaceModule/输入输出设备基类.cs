using System;
using System.Collections.Generic;
using System.Text;

namespace SkyVM.InterfaceModule
{
    /// <summary>
    /// 输入输出设备基类
    /// </summary>
    public abstract class IOBase
    {
        /// <summary>
        /// 设备状态
        /// </summary>
        public Device_State State { get; protected set; }
        /// <summary>
        /// 设备属性
        /// </summary>
        public DeviceProperty Property { get; protected set; }
        /// <summary>
        /// 上层中断
        /// </summary>
        protected IDeviceControlInterrupt Interrupt { get; private set; }
        /// <summary>
        /// 输入数据队列
        /// </summary>
        protected readonly Queue<byte[]> InPutQueue;
        /// <summary>
        /// 输出数据队列
        /// </summary>
        protected readonly Queue<byte[]> OutPutQueue;

        /// <summary>
        /// 输入输出设备基类构造函数
        /// </summary>
        /// <param name="interrupt">设备中断接口</param>
        public IOBase(IDeviceControlInterrupt interrupt)
        {
            Interrupt = interrupt;
            InPutQueue = new Queue<byte[]>();
            OutPutQueue = new Queue<byte[]>();
        }

        /// <summary>
        /// 启动设备-将设备从停止/暂停中恢复
        /// </summary>
        public abstract void Run();
        /// <summary>
        /// 停止设备-将设备从运行/暂停/异常状态下停止
        /// </summary>
        public abstract void Stop();
        /// <summary>
        /// 暂停设备-将设备从运行状态下暂停
        /// </summary>
        public abstract void Pause();
        /// <summary>
        /// 复位设备-将设备从任意状态恢复初始化设置并重新运行
        /// </summary>
        public abstract void Reset();
        /// <summary>
        /// 从设备中获取数据
        /// </summary>
        /// <returns>返回数据包</returns>
        public byte[] Get()
        {
            if (0 < OutPutQueue.Count)
                return OutPutQueue.Dequeue();
            return null;
        }
        /// <summary>
        /// 向设备发送数据
        /// </summary>
        /// <param name="data">数据</param>
        public void Set(byte[] data)
        {
            if (null != data)
                InPutQueue.Enqueue(data);
        }
    }

    /// <summary>
    /// 设备属性包
    /// </summary>
    public class DeviceProperty
    {
        /// <summary>
        /// 设备名
        /// </summary>
        public string DeviceName;
        /// <summary>
        /// 设备属性描述
        /// </summary>
        public string DeviceDescription;
        /// <summary>
        /// 设备ID
        /// </summary>
        public string DeviceID;
    }
}
