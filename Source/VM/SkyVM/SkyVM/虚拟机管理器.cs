using SkyVM.ExModule;
using SkyVM.InterfaceModule;
using SkyVM.IOModule;
using System;
using System.Collections.Generic;
using System.Text;
using static SkyVM.IOModule.NetUnit;

namespace SkyVM
{
    /// <summary>
    /// 虚拟机控制器
    /// </summary>
    public abstract class VMController : IApplication, IDeviceControlInterrupt, INetControlInterrupt
    {
        #region 内存控制器接口

        /// <summary>
        /// 获取内存容量大小
        /// </summary>
        public long MemorySize => MemoryController.MemorySize;
        /// <summary>
        /// 获取正在使用的内存块
        /// </summary>
        public MemoryNode[] Using
        {
            get
            {
                MemoryNode[] nodes = null;
                if (SVM.MemoryLock_O())
                {
                    nodes = MemoryController.Using;
                    SVM.MemoryUnLock_O();
                }
                return nodes;
            }
        }
        /// <summary>
        /// 获取空闲的内存块
        /// </summary>
        public MemoryNode[] Freeing
        {
            get
            {
                MemoryNode[] nodes = null;
                if (SVM.MemoryLock_O())
                {
                    nodes = MemoryController.Freeing;
                    SVM.MemoryUnLock_O();
                }
                return nodes;
            }
        }

        #endregion

        #region 网络控制器接口

        /// <summary>
        /// 网络连接表
        /// </summary>
        public Dictionary<int, Net_Parameter> ConnectionTable
        {
            get
            {
                Dictionary<int, Net_Parameter> pairs = null;
                if (SVM.NetLock_O())
                {
                    pairs = NetController.ConnectionTable;
                    SVM.NetUnLock_O();
                }
                return pairs;
            }
        }

        #endregion


        private readonly MemoryUnit MemoryController;
        private readonly IOUnit IOController;
        private readonly NetUnit NetController;
        private readonly IComplexOperation Complex;
        private readonly Time TimeTool;
        private VisualMachine SVM;
        private string MainPath;

        /// <summary>
        /// 虚拟机控制器构造函数
        /// </summary>
        /// <param name="net_conn">网络连接创建组件</param>
        /// <param name="complex">复杂数学运算组件</param>
        /// <param name="time_path">时间组件文件路径</param>
        /// <param name="mem_size">内存大小（默认8MB）</param>
        /// <param name="mem_align">内存最小分配量</param>
        /// <param name="mem_offset">内存空闲空间分配差值</param>
        public VMController(INetConnect net_conn, IComplexOperation complex, string time_path = null, int mem_size = Memory_Size, short mem_align = Memory_Align, short mem_offset = Memory_Offset)
        {
            MemoryController = new MemoryUnit(mem_size, mem_align, mem_offset);
            IOController = new IOUnit(this);
            NetController = new NetUnit(net_conn, this);
            TimeTool = new Time(time_path);
            Complex = complex;
            SVM = null;
        }

        #region 虚拟机控制

        /// <summary>
        /// 虚拟机启动方法
        /// </summary>
        /// <param name="path">启动文件路径</param>
        public void Boot(string path)
        {
            if (null != path)
            {
                MainPath = path;
                SVM = new VisualMachine(this, MainPath, MemoryController, Complex, TimeTool, IOController, NetController);
            }
        }
        /// <summary>
        /// 虚拟机重启
        /// </summary>
        public void ReBoot()
        {
            if (null != SVM)
                SVM.Close();
            SVM = new VisualMachine(this, MainPath, MemoryController, Complex, TimeTool, IOController, NetController);
        }
        /// <summary>
        /// 虚拟机暂停
        /// </summary>
        public void Pause()
        {
            if (null != SVM)
                SVM.Pause();
        }
        /// <summary>
        /// 虚拟机恢复
        /// </summary>
        public void Reset()
        {
            if (null != SVM)
                SVM.Reset();
        }
        /// <summary>
        /// 虚拟机关机
        /// </summary>
        public void Shutdown()
        {
            if (null != SVM)
                SVM.Close();
        }

        #endregion

        #region 设备控制器接口

        /// <summary>
        /// 添加一个设备-创建时加载（虚拟机内部不可动态调整）
        /// </summary>
        /// <param name="interrupt_id">设备中断号</param>
        /// <param name="device">设备实例</param>
        public void AddDevice(int interrupt_id, IOBase device)
        {
            IOController.AddDevice(interrupt_id, device);
        }
        /// <summary>
        /// 删除一个设备
        /// </summary>
        /// <param name="device_id">设备号</param>
        public void DeleteDevice(int device_id)
        {
            IOController.DeleteDevice(device_id);
        }
        /// <summary>
        /// 获取所有设备信息
        /// </summary>
        /// <returns>返回所有设备</returns>
        public object[] GetDevices()
        {
            return IOController.GetDevices();
        }
        /// <summary>
        /// 获取指定中断号设备信息
        /// </summary>
        /// <param name="interrupt_id">设备中断号</param>
        /// <returns>返回设备信息</returns>
        public object GetDevice(int interrupt_id)
        {
            return IOController.GetDevice(interrupt_id);
        }
        /// <summary>
        /// 设置新的设备中断号
        /// </summary>
        /// <param name="device_id">设备号</param>
        /// <param name="new_interrupt">中断号</param>
        public void SetInterrupt(int device_id, int new_interrupt)
        {
            IOController.SetInterrupt(device_id, new_interrupt);
        }
        /// <summary>
        /// 添加指定设备的中断服务程序
        /// </summary>
        /// <param name="interrupt_id">设备中断号</param>
        /// <param name="programs">中断服务程序</param>
        public void AddInterruptService(int interrupt_id, params string[] programs)
        {
            IOController.AddInterruptService(interrupt_id, programs);
        }
        /// <summary>
        /// 删除指定设备的指定终端服务程序
        /// </summary>
        /// <param name="interrupt_id">设备中断号</param>
        /// <param name="program">中断服务程序</param>
        public void DelInterruptService(int interrupt_id, string program)
        {
            IOController.DelInterruptService(interrupt_id, program);
        }
        /// <summary>
        /// 清空指定设备的中断服务程序列表
        /// </summary>
        /// <param name="interrupt_id">设备中断号</param>
        public void CleanInterruptService(int interrupt_id)
        {
            IOController.CleanInterruptService(interrupt_id);
        }

        #endregion


        #region 设备控制器中断接口
        /// <summary>
        /// 设备中断接口
        /// </summary>
        /// <param name="exception">异常</param>
        /// <param name="device_id">设备号</param>
        /// <param name="device">设备主体</param>
        public abstract void Interrupt(DeviceException_Type exception, int device_id, IOBase device);

        #endregion


        #region 网络控制器中断接口
        /// <summary>
        /// 网络中断接口
        /// </summary>
        /// <param name="exception">异常</param>
        /// <param name="connection_id">连接号</param>
        /// <param name="ip">IP地址</param>
        /// <param name="port">端口</param>
        public abstract void Interrupt(NetException_Type exception, int connection_id, string ip, int port);

        #endregion

        #region 程序输出界面接口

        /// <summary>
        /// 网络连接创建失败
        /// </summary>
        /// <param name="process">连接创建程序ID</param>
        public abstract void NetCreateFailed(int process);
        /// <summary>
        /// 输出方法
        /// </summary>
        /// <param name="str">输出字符串</param>
        public abstract void Print(string str);
        /// <summary>
        /// 输入方法
        /// </summary>
        /// <param name="size">输入数据长度</param>
        /// <returns>返回字节流</returns>
        public abstract byte[] Scan(long size);
        /// <summary>
        /// 虚拟机结束方法
        /// </summary>
        public abstract void ShutDown();

        #endregion

        #region 静态常量组

        /// <summary>
        /// 内存大小
        /// </summary>
        protected const int Memory_Size = 1024 * 1024 * 8;
        /// <summary>
        /// 内存最小分配大小
        /// </summary>
        protected const short Memory_Align = 16;
        /// <summary>
        /// 空闲内存分配差值
        /// </summary>
        protected const short Memory_Offset = 16;

        #endregion
    }
}
