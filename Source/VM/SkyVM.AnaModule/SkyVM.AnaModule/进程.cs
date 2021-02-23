using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using static SkyVM.AnaModule.InstructionList;

namespace SkyVM.AnaModule
{
    /// <summary>
    /// 进程
    /// </summary>
    public class Process
    {
        /// <summary>
        /// 程序运行标志
        /// </summary>
        public bool Running;
        /// <summary>
        /// 进程运行时
        /// </summary>
        public TimeSpan Time;
        /// <summary>
        /// 进程号（主程序号）
        /// </summary>
        public readonly int MainProgramCode;
        /// <summary>
        /// 获取当前运行的程序指针
        /// </summary>
        public int PC
        {
            get
            {
                return null != GetRunningProgram() ? GetRunningProgram().PC : -1;
            }
        }
        /// <summary>
        /// 获取程序段地址
        /// </summary>
        public long Segment
        {
            get
            {
                return null != GetRunningProgram() ? GetRunningProgram().MemorySpace : -1;
            }
        }

        /// <summary>
        /// 程序行进表
        /// </summary>
        private readonly ProgramEXT RunTable;
        /// <summary>
        /// 程序加载表
        /// </summary>
        private readonly ProgramLoadTable LoadTable;//提供程序段加载的别名与程序代码-需要有增加/删除
        /// <summary>
        /// 公共数据堆栈
        /// </summary>
        private readonly PublicDataStack DataStack; //需要提供入栈/出栈（4字节固定大小）
        /// <summary>
        /// 内存占用表
        /// </summary>
        private readonly SortedList<long, Memory_Package> MOCCTable; //提供内存检测的内存号与长度-需要有增加/删除/查询/检测
        /// <summary>
        /// 文件表
        /// </summary>
        private readonly SortedList<string, int> FileTabel;
        /// <summary>
        /// 网络连接表
        /// </summary>
        private readonly SortedList<string, int> ConnectTabel;
        /// <summary>
        /// 主程序单元
        /// </summary>
        private readonly ProgramUnit MainProgram;
        /// <summary>
        /// 程序表
        /// </summary>
        private readonly Hashtable ProgramTable; //提供增加/删除

        private readonly int StackSize;
        private int Program;

        /// <summary>
        /// 进程构造函数
        /// </summary>
        /// <param name="pid">进程号-主程序号</param>
        /// <param name="main_program">主进程程序段</param>
        /// <param name="s_size">公共数据堆栈大小</param>
        public Process(int pid, ProgramUnit main_program, int s_size)
        {
            MainProgramCode = pid;

            RunTable = new ProgramEXT();
            LoadTable = new ProgramLoadTable();
            DataStack = new PublicDataStack();
            MOCCTable = new SortedList<long, Memory_Package>();
            FileTabel = new SortedList<string, int>();
            ConnectTabel = new SortedList<string, int>();
            ProgramTable = new Hashtable();
            MainProgram = main_program;

            StackSize = s_size;
            Program = pid;
            RunTable.NewProgram(Program);
        }

        //程序控制接口
        /// <summary>
        /// 获取下一条指令
        /// </summary>
        /// <returns>返回指令</returns>
        public InstructionPackage Next()
        {
            return null != GetRunningProgram() ?
                GetRunningProgram().GetInstruction() :
                new InstructionPackage
                {
                    Instructuion = Instructuions_Package.END,
                    Parameters = null
                };
        }
        /// <summary>
        /// 程序跳转指令
        /// </summary>
        /// <param name="label">跳转标签</param>
        /// <returns>返回跳转结果</returns>
        public bool Jmp(string label)
        {
            return null != GetRunningProgram() ? -1 != GetRunningProgram().Jmp(label) : false;
        }
        /// <summary>
        /// 程序调用指令
        /// </summary>
        /// <param name="label">过程标签</param>
        /// <param name="p_name">调用过程程序段别名（外部调用时使用）</param>
        /// <returns>返回调用结果</returns>
        public bool Call(string label, string p_name = null)
        {
            if (null == p_name)
                return InsideCall(label);
            else
                return OusideCall(label, p_name);
        }
        /// <summary>
        /// 调用返回指令
        /// </summary>
        public void Ret()
        {
            RunTable.CallBackFunction(out int pc);
            Program = RunTable.GetProgram;
            if (null != GetRunningProgram())
                GetRunningProgram().Jmp(pc);
        }

        /// <summary>
        /// 添加一个申请的内存记录
        /// </summary>
        /// <param name="mem_id">内存块号</param>
        /// <param name="mem_size">内存长度</param>
        public void AddMemory(long mem_id, int mem_size)
        {
            MOCCTable.Add(mem_id, new Memory_Package
            {
                FatherProgram = LoadTable.GetProgram(RunTable.GetProgram),
                Size = mem_size
            });
        }
        /// <summary>
        /// 删除一个申请内存记录
        /// </summary>
        /// <param name="mem_id">内存块号</param>
        public void DelMemory(long mem_id)
        {
            MOCCTable.Remove(mem_id);
        }
        /// <summary>
        /// 获取指定内存块申请大小
        /// </summary>
        /// <param name="mem_id">内存块号</param>
        /// <returns>返回申请大小</returns>
        public int GetMemSize(long mem_id)
        {
            if (MOCCTable.TryGetValue(mem_id, out Memory_Package value))
                return value.Size;
            return -1;
        }
        /// <summary>
        /// 检测内存有效性
        /// </summary>
        /// <param name="mem_id">内存块号</param>
        /// <returns>返回结果</returns>
        public bool Contains(long mem_id)
        {
            return MOCCTable.ContainsKey(mem_id);
        }
        /// <summary>
        /// 获取所有占用内存
        /// </summary>
        /// <returns>返回占用表</returns>
        public long[] GetAllMemorys()
        {
            List<long> vs = new List<long>(MOCCTable.Keys)
            {
                MainProgram.MemorySpace
            };
            foreach (var i in ProgramTable)
                vs.Add((i as ProgramUnit).MemorySpace);
            return vs.ToArray();
        }

        /// <summary>
        /// 添加网络连接
        /// </summary>
        /// <param name="label">网络连接标签</param>
        /// <param name="conn_id">网络连接号</param>
        /// <returns>返回覆盖的网络连接</returns>
        public int AddConnect(string label, int conn_id)
        {
            if (ConnectTabel.TryGetValue(label,out int value))
            {
                ConnectTabel.Remove(label);
                return value;
            }
            ConnectTabel.Add(label, conn_id);
            return -1;
        }
        /// <summary>
        /// 删除网络连接
        /// </summary>
        /// <param name="label">网络连接标签</param>
        public void DelConnect(string label)
        {
            ConnectTabel.Remove(label);
        }
        /// <summary>
        /// 获取网络连接号
        /// </summary>
        /// <param name="label">网络连接标签</param>
        /// <returns>返回网络连接号</returns>
        public int GetConnect(string label)
        {
            if (ConnectTabel.TryGetValue(label, out int value))
                return value;
            return -1;
        }
        /// <summary>
        /// 获取所有网络连接
        /// </summary>
        /// <returns>返回网络连接组</returns>
        public int[] GetConnects()
        {
            return new List<int>(ConnectTabel.Values).ToArray();
        }

        /// <summary>
        /// 添加文件连接
        /// </summary>
        /// <param name="label">文件标识符</param>
        /// <param name="file_id">文件号</param>
        /// <returns>返回覆盖的文件号</returns>
        public int AddFile(string label, int file_id)
        {
            if (FileTabel.TryGetValue(label, out int value))
            {
                FileTabel.Remove(label);
                return value;
            }
            FileTabel.Add(label, file_id);
            return -1;
        }
        /// <summary>
        /// 删除文件连接
        /// </summary>
        /// <param name="label">文件标识符</param>
        public void DelFile(string label)
        {
            FileTabel.Remove(label);
        }
        /// <summary>
        /// 获取文件连接号
        /// </summary>
        /// <param name="label">文件标识符</param>
        /// <returns>返回文件连接号</returns>
        public int GetFile(string label)
        {
            if (FileTabel.TryGetValue(label, out int value))
                return value;
            return -1;
        }
        /// <summary>
        /// 获取所有文件连接
        /// </summary>
        /// <returns>返回文件连接组</returns>
        public int[] GetFiles()
        {
            return new List<int>(FileTabel.Values).ToArray();
        }

        /// <summary>
        /// 载入一个程序段
        /// </summary>
        /// <param name="p_id">程序ID</param>
        /// <param name="p_name">程序别名</param>
        /// <param name="program">程序主体</param>
        /// <returns>返回加载结果</returns>
        public bool AddProgram(int p_id, string p_name, ProgramUnit program)
        {
            bool result = false ;
            if (LoadTable.Add(p_name, p_id))
            {
                ProgramTable.Add(p_name, program);
                result = true;
            }
            return result;
        }
        /// <summary>
        /// 删除一个程序段
        /// </summary>
        /// <param name="p_name">程序段别名</param>
        /// <param name="id">获取移除程序的ID</param>
        /// <param name="address">获取移除程序的地址</param>
        public void DelProgram(string p_name, out int id, out long address)
        {
            id = LoadTable.GetProgram(p_name);
            address = -1;
            if (-1 != id)
            {
                LoadTable.Del(p_name);
                object var = ProgramTable[p_name];
                if (null != var)
                {
                    address = (var as ProgramUnit).MemorySpace;
                    ProgramTable.Remove(p_name);
                }
            }
        }

        /// <summary>
        /// 数据入栈
        /// </summary>
        /// <param name="d1">内存块1</param>
        /// <param name="d2">内存块2</param>
        /// <param name="d3">内存块3</param>
        /// <param name="d4">内存块4</param>
        public bool Push(byte d1, byte d2, byte d3, byte d4)
        {
            bool result = true;

            if (DataStack.Count < StackSize)
                DataStack.Push(d1, d2, d3, d4);
            else
                result = false;

            return result;
        }
        /// <summary>
        /// 数据出栈
        /// </summary>
        /// <param name="d1">内存块1</param>
        /// <param name="d2">内存块2</param>
        /// <param name="d3">内存块3</param>
        /// <param name="d4">内存块4</param>
        /// <returns>返回出栈结果</returns>
        public bool Pop(out byte d1, out byte d2, out byte d3, out byte d4)
        {
            bool result = true;
            d1 = d2 = d3 = d4 = 0x00;
            if (0 < DataStack.Count)
                DataStack.Pop(out d1, out d2, out d3, out d4);
            else
                result = false;
            return result;
        }


        private ProgramUnit GetRunningProgram()
        {
            if (MainProgramCode == Program)
                return MainProgram;
            else
                return (ProgramUnit)ProgramTable[LoadTable.GetProgram(Program)];
        }
        private bool InsideCall(string label)
        {
            RunTable.CallFunction(PC);
            return Jmp(label);
        }
        private bool OusideCall(string label, string program)
        {
            int temp = LoadTable.GetProgram(program);
            if (-1 != temp)
            {
                RunTable.CallFunction(PC);
                RunTable.NewProgram(temp);
                Program = temp;
                return Jmp(label);
            }
            else
                return false;
        }


        class Memory_Package
        {
            public string FatherProgram;
            public int Size;
        }
    }
}
