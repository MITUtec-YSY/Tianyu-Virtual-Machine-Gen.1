using System;
using System.Collections.Generic;
using System.Text;
using static SkyVM.AnaModule.ParameterList;

namespace SkyVM.AnaModule
{
    /// <summary>
    /// 指令列表
    /// </summary>
    public class InstructionList
    {
        /// <summary>
        /// 获取总程序指令个数
        /// </summary>
        public int Count { get; private set; }
        /// <summary>
        /// 获取当前的程序指针
        /// </summary>
        public int PC { get; private set; }

        private readonly InstructionNode Head;
        private InstructionNode End;
        private InstructionNode Now;

        /// <summary>
        /// 指令列表构造函数
        /// </summary>
        public InstructionList()
        {
            Head = new InstructionNode(Instructuions_Package.NOP)
            {
                Next = null,
                Pre = null
            };
            End = Head;
            Now = Head;
            Count = 0;
            PC = 0;
        }

        /// <summary>
        /// 添加指令方法
        /// </summary>
        /// <param name="instructuion">指令类型</param>
        /// <param name="parameters">参数列表</param>
        public void Add(Instructuions_Package instructuion, ParameterPackage[] parameters)
        {
            End.Next = new InstructionNode(instructuion)
            {
                Next = null,
                Pre = End
            };
            End = End.Next;
            Count++;
            if (null != parameters)
                for (int i = 0; i < parameters.Length; i++)
                    End.Add(parameters[i].Type, parameters[i].Parameter);
        }
        /// <summary>
        /// 结束程序方法-用于读取文件完成后在最后加入一条本地结束指令
        /// </summary>
        public void EndRead()
        {
            End.Next = new InstructionNode(Instructuions_Package.END)
            {
                Next = null, 
                Pre = End
            };
            End = End.Next;
        }
        /// <summary>
        /// 获取下一条指令
        /// </summary>
        /// <returns>返回下一条指令</returns>
        public InstructionPackage GetNext()
        {
            InstructionPackage result = null;
            if (null != Now.Next)
            {
                result = Now.Next.GetInstruction();
                Now = Now.Next;
                PC++;
            }
            return result;
        }
        /// <summary>
        /// 指令跳转
        /// </summary>
        /// <param name="location">跳转定位</param>
        public void Jmp(int location)
        {
            if (0 >= location)
            {
                PC = 0;
                Now = Head;
            }
            else if (Count <= location)
            {
                PC = Count - 1;
                Now = End;
            }
            else if (PC != location)
            {
                if (PC < location)
                    for (int i = location - PC; i > 0; i--)
                    {
                        Now = Now.Next;
                        PC++;
                    }
                else
                    for(int i = PC - location; i > 0; i--)
                    {
                        Now = Now.Pre;
                        PC--;
                    }
            }
        }

        /// <summary>
        /// 指令传递包
        /// </summary>
        public class InstructionPackage
        {
            /// <summary>
            /// 指令操作
            /// </summary>
            public Instructuions_Package Instructuion;
            /// <summary>
            /// 参数表
            /// </summary>
            public ParameterPackage[] Parameters;
        }

        /// <summary>
        /// 指令节点
        /// </summary>
        class InstructionNode
        {
            public InstructionNode Next;
            public InstructionNode Pre;

            private readonly Instructuions_Package Instructuion;
            private readonly ParameterList ParameterList;

            public InstructionNode(Instructuions_Package instructuion)
            {
                Instructuion = instructuion;
                ParameterList = new ParameterList();
            }

            public void Add(Parameter_Type type, object para)
            {
                ParameterList.Add(type, para);
            }
            public InstructionPackage GetInstruction()
            {
                return new InstructionPackage
                {
                    Instructuion = Instructuion,
                    Parameters = ParameterList.GetParameters()
                };
            }
        }
    }
}
