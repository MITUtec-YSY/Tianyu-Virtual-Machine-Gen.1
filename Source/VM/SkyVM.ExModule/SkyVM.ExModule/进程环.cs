using System;
using System.Collections.Generic;
using System.Text;
using SkyVM.AnaModule;

namespace SkyVM.ExModule
{
    /// <summary>
    /// 进程运行组件
    /// </summary>
    public class ProcessRing
    {
        /// <summary>
        /// 获取当前进程数
        /// </summary>
        public int Count { get; private set; }

        private Node ProcessLink;
        
        /// <summary>
        /// 进程运行组件构造函数
        /// </summary>
        public ProcessRing()
        {
            Count = 0;
            ProcessLink = null;
        }

        /// <summary>
        /// 获取下一个需要运行的进程
        /// </summary>
        /// <returns>返回进程包</returns>
        public Process GetProcess()
        {
            Process process = null;
            for(int i = 0; i < Count; i++)
            {
                if (!ProcessLink.Next.Process.Running)
                {
                    process = ProcessLink.Next.Process;
                    ProcessLink = ProcessLink.Next;
                    break;
                }
                else
                    ProcessLink = ProcessLink.Next;
            }
            return process;
        }
        /// <summary>
        /// 增加新的进程
        /// </summary>
        /// <param name="process">进程包</param>
        public void NewProcess(Process process)
        {
            if (0 == Count)
            {
                ProcessLink = new Node
                {
                    Process = process
                };
                ProcessLink.Next = ProcessLink;
                ProcessLink.Process.Time = new TimeSpan(0, 0, 0, 0, 0);
                ProcessLink.Process.Running = false;
                Count++;
            }
            else
            {
                Node node = new Node
                {
                    Process = process,
                    Next = ProcessLink.Next
                };
                node.Process.Running = false;
                node.Process.Time = new TimeSpan(0, 0, 0, 0, 0);
                ProcessLink.Next = node;
                Count++;
            }
        }
        /// <summary>
        /// 移除一个进程
        /// </summary>
        /// <param name="main_code">主进程编号</param>
        public void DelProcess(int main_code)
        {
            for(int i = 0; i < Count; i++)
            {
                if (ProcessLink.Next.Process.MainProgramCode == main_code)
                {
                    if (1 == Count)
                        ProcessLink = null;
                    else
                        ProcessLink.Next = ProcessLink.Next.Next;
                    Count--;
                    break;
                }
                else
                    ProcessLink = ProcessLink.Next;
            }
        }
        /// <summary>
        /// 查询指定的进程
        /// </summary>
        /// <param name="process_id">进程ID</param>
        /// <returns>返回进程</returns>
        public Process GetProcess(int process_id)
        {
            Process process = null;
            for (int i = 0; i < Count; i++)
            {
                if (ProcessLink.Next.Process.MainProgramCode == process_id)
                {
                    process = ProcessLink.Next.Process;
                    break;
                }
                else
                    ProcessLink = ProcessLink.Next;
            }
            return process;
        }
        
        /// <summary>
        /// 进程包
        /// </summary>
        public class Node
        {
            /// <summary>
            /// 进程组件
            /// </summary>
            public Process Process;
            internal Node Next;
        }
    }
}
