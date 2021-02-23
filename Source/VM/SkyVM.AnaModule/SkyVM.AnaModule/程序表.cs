using System;
using System.Collections.Generic;
using System.Text;
using static SkyVM.AnaModule.InstructionList;
using static SkyVM.AnaModule.ParameterList;
using System.Numerics;
using System.Linq.Expressions;

namespace SkyVM.AnaModule
{
    /// <summary>
    /// 程序异常类
    /// 异常：重复的标识符（0x01)
    /// 异常：内存设置参数错误（0x02）
    /// 异常：内存重复设置（0x03）
    /// 异常：未定义的标识符（0x04）
    /// </summary>
    public class ProgramException : Exception
    {
        /// <summary>
        /// 程序异常码
        /// </summary>
        public int Exception { get; }

        /// <summary>
        /// 程序异常类构造函数
        /// </summary>
        /// <param name="ec">程序异常码</param>
        public ProgramException(int ec) : base()
        {
            Exception = ec;
        }
    }
    /// <summary>
    /// 程序单元
    /// </summary>
    public class ProgramUnit
    {
        /// <summary>
        /// 内存空间
        /// </summary>
        public long MemorySpace;
        /// <summary>
        /// 内存空间大小
        /// </summary>
        public int MemorySize;
        /// <summary>
        /// 运行程序指针
        /// </summary>
        public int PC => InstructionList.PC;

        private readonly SortedList<string, int> IDTable;
        private readonly InstructionList InstructionList;
        private bool MemorySet;

        /// <summary>
        /// 程序单元构造函数
        /// </summary>
        public ProgramUnit()
        {
            IDTable = new SortedList<string, int>();
            InstructionList = new InstructionList();
            MemorySet = false;
        }

        /// <summary>
        /// 添加一个标识符
        /// </summary>
        /// <param name="id">标识符名</param>
        public void AddID(string id)
        {
            if (!IDTable.ContainsKey(id))
                IDTable.Add(id, InstructionList.Count);
            else
                throw new ProgramException(0x01);
        }
        /// <summary>
        /// 程序文件读取结束
        /// </summary>
        public void EndRead()
        {
            InstructionList.EndRead();
        }
        /// <summary>
        /// 添加指令方法
        /// </summary>
        /// <param name="instructuion">指令类型</param>
        /// <param name="parameters">参数列表</param>
        public void Add(Instructuions_Package instructuion, ParameterPackage[] parameters)
        {
            if (Instructuions_Package.Set_Memory == instructuion)
                if (!MemorySet)
                    if (0 < parameters.Length)
                    {
                        if (Parameter_Type.Long == parameters[0].Type)
                            SetMemory((int)parameters[0].Parameter);
                        else if (Parameter_Type.BigNumber == parameters[0].Type)
                            SetMemory((int)(BigInteger)parameters[0].Parameter);
                    }
                    else
                        throw new ProgramException(0x02);
                else
                    throw new ProgramException(0x03);
            else
                InstructionList.Add(instructuion, parameters);
        }
        /// <summary>
        /// 获取一条指令
        /// </summary>
        /// <returns>返回当前执行指令的下一条指令</returns>
        public InstructionPackage GetInstruction()
        {
            return InstructionList.GetNext();
        }
        /// <summary>
        /// 非指令跳转
        /// </summary>
        /// <param name="location">跳转位置</param>
        public void Jmp(int location)
        {
            InstructionList.Jmp(location);
        }
        /// <summary>
        /// 指令跳转
        /// </summary>
        /// <param name="id">跳转标签</param>
        public int Jmp(string id)
        {
            if (IDTable.TryGetValue(id, out int value))
            {
                InstructionList.Jmp(value);
                return value;
            }
            else
                throw new ProgramException(0x04);
        }

        private void SetMemory(int size)
        {
            MemorySize = MemorySize < size ? size : MemorySize;
            MemorySet = true;
        }
    }
}
