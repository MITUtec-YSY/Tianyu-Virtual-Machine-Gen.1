using System;
using System.Collections.Generic;
using System.Text;

namespace SkyVM.ExModule
{
    /// <summary>
    /// 执行单元接口
    /// </summary>
    public interface IExecutorControl
    {
        /// <summary>
        /// 中断接口函数
        /// </summary>
        /// <param name="coreid">核心编号</param>
        /// <param name="interrupt">设备中断号</param>
        /// <param name="data">设备ID</param>
        object Interrupt(int coreid, InterruptCode interrupt, params object[] data);
    }
}
