using System;
using SkyVM.ExModule;
using SkyVM.InterfaceModule;

namespace SkyVM
{
    /// <summary>
    /// 天宇虚拟机-默认框架
    /// </summary>
    public sealed class VisualMachine : CodeExecutorUnit
    {
        /// <summary>
        /// 天宇虚拟机默认框架构造函数
        /// </summary>
        /// <param name="application">上层应用程序接口</param>
        /// <param name="path">初始启动文件路径</param>
        /// <param name="memory">内存接口</param>
        /// <param name="icomplex">复杂运算接口</param>
        /// <param name="time">时间接口</param>
        /// <param name="idevice">设备接口</param>
        /// <param name="inet">网络接口</param>
        public VisualMachine(IApplication application, string path, MemoryBase memory, IComplexOperation icomplex, TimeBase time, IDevice idevice, INetControl inet) : base(application, path, memory, 16, icomplex, time, idevice, inet, 3, 1, 100)
        {

        }
    }
}
