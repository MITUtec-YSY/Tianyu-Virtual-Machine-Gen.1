using SkyVM.CoreModule.Complexs;
using SkyVM.ExModule;
using SkyVM.IOModule;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace 虚拟机测试_独立测试
{
    class Program
    {
        static void Main(string[] args)
        {
            string str = "";
            
            VM program = new VM();
            Console.Write("请输入文件路径（输入“end”结束）：");   //加载虚拟机执行文件
            str = Console.ReadLine();       // 输入执行文件路径
            VisualMachine vm = new VisualMachine    // 创建程序
                (
                program,
                str,
                new MemoryUnit(VM.MemorySize, (short)VM.MemoryAline, (short)VM.MemoryOffset),
                VM.MemoryMinSize,
                new ComplexAU(),
                new Time(),
                new IOUnit(program),
                new NetUnit(new NetControl(), program),
                VM.CacheSize,
                VM.CoreCount,
                VM.StackSize
                );
        }
    }
}
