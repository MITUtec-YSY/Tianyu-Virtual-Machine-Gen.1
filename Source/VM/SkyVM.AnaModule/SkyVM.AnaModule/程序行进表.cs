using System;
using System.Collections.Generic;
using System.Text;

namespace SkyVM.AnaModule
{
    /// <summary>
    /// 程序行进表
    /// </summary>
    public class ProgramEXT
    {
        /// <summary>
        /// 程序调用队列
        /// </summary>
        public int Count => ProgramEX.Count;
        /// <summary>
        /// 获取当前正在运行的程序
        /// </summary>
        public int GetProgram
        {
            get
            {
                int result = -1;
                if (0 < ProgramEX.Count)
                    result = ProgramEX.Peek().ProgramCode;
                return result;
            }
        }
        /// <summary>
        /// 获取当前正在运行的程序的回调指针
        /// </summary>
        public int GetPC
        {
            get
            {
                int result = -1;
                if (0 < ProgramEX.Count)
                    if (0 < ProgramEX.Peek().Count)
                        result = ProgramEX.Peek().Peek();
                return result;
            }
        }

        private readonly Stack<ProgramEXTNode> ProgramEX;

        /// <summary>
        /// 程序行进表构造函数
        /// </summary>
        public ProgramEXT()
        {
            ProgramEX = new Stack<ProgramEXTNode>();
        }

        /// <summary>
        /// 新程序调度-用于外部调用以及程序调用中断
        /// </summary>
        /// <param name="code">新程序号</param>
        public void NewProgram(int code)
        {
            ProgramEX.Push(new ProgramEXTNode
            {
                ProgramCode = code
            });
        }
        /// <summary>
        /// 过程调度-用于所有的程序调度后回调指针的保存
        /// </summary>
        /// <param name="pc">程序指针</param>
        public void CallFunction(int pc)
        {
            ProgramEX.Peek().Push(pc);
        }
        /// <summary>
        /// 程序回调-用于外部调用回调与程序调用中断回调
        /// </summary>
        /// <returns>返回回调结果</returns>
        public CallBack_Flag CallBackProgram()
        {
            CallBack_Flag flag = CallBack_Flag.CALLBACK_PROGRAM;
            ProgramEX.Pop();
            if (0 >= ProgramEX.Count)
                flag = CallBack_Flag.CALLBACK_END;
            return flag;
        }
        /// <summary>
        /// 过程回调-用于所有的程序调度后的指针返回
        /// </summary>
        /// <param name="pc">回调指针</param>
        /// <returns>返回回调结果</returns>
        public CallBack_Flag CallBackFunction(out int pc)
        {
            CallBack_Flag flag = CallBack_Flag.CALLBACK_FUNCTION;
            if (0 >= ProgramEX.Peek().Count)
                flag = CallBackProgram();
            if (CallBack_Flag.CALLBACK_END != flag)
                pc = ProgramEX.Peek().Pop();
            else
                pc = -1;
            return flag;
        }


        /// <summary>
        /// 程序回调结果
        /// </summary>
        public enum CallBack_Flag
        {
            /// <summary>
            /// 回调成功-回调结果为过程回调
            /// </summary>
            CALLBACK_FUNCTION,
            /// <summary>
            /// 回调成功-回调结果为程序回调
            /// </summary>
            CALLBACK_PROGRAM,
            /// <summary>
            /// 回调失败-进程需结束
            /// </summary>
            CALLBACK_END,
        }

        /// <summary>
        /// 程序行进表节点
        /// </summary>
        class ProgramEXTNode
        {
            public int ProgramCode;
            public int Count => FunctionCall.Count;

            private readonly Stack<int> FunctionCall;

            public ProgramEXTNode()
            {
                FunctionCall = new Stack<int>();
            }

            public void Push(int pc)
            {
                FunctionCall.Push(pc);
            }
            public int Pop()
            {
                return FunctionCall.Pop();
            }
            public int Peek()
            {
                return FunctionCall.Peek();
            }
        }
    }
}
