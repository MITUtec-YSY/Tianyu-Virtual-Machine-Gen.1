using SkyVM.ExModule;
using SkyVM.InterfaceModule;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace 虚拟机测试_独立测试
{
    /// <summary>
    /// 虚拟机执行单元实现类
    /// </summary>
    class VisualMachine : CodeExecutorUnit
    {
        public VisualMachine(IApplication application, string path, MemoryBase memory, int min_mem_size, IComplexOperation icomplex, TimeBase time, IDevice idevice, INetControl inet, int cache_size, int core_num, int stack_size)
            : base(application, path, memory, min_mem_size, icomplex, time, idevice, inet, cache_size, core_num, stack_size)
        {

        }
    }
}
