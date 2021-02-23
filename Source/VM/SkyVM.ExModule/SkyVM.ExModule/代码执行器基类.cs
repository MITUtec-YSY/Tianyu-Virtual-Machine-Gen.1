using SkyVM.AnaModule;
using SkyVM.InterfaceModule;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using System.Threading;
using static SkyVM.AnaModule.ParameterList;

namespace SkyVM.ExModule
{
    /// <summary>
    /// 程序代码执行器基类
    /// </summary>
    public abstract class CodeExecutor : IMemoryCacheInterrupt
    {
        /// <summary>
        /// 核心编号
        /// </summary>
        public int CoreID { get; private set; }
        /// <summary>
        /// 获取正在运行的进程ID
        /// </summary>
        public int ProcessID
        {
            get
            {
                if (null != Process)
                    return Process.MainProgramCode;
                return -1;
            }
        }

        /// <summary>
        /// 运行标志
        /// </summary>
        protected bool Flag;
        /// <summary>
        /// 程序段
        /// </summary>
        protected Process Process;

        /// <summary>
        /// 算数逻辑单元
        /// </summary>
        private readonly IALUnit ALU;
        /// <summary>
        /// 一级缓存
        /// </summary>
        private readonly ICache Cache;
        private readonly IExecutorControl ExecutorControl;
        private Thread Thread;
        private DateTime StartTime;
        /// <summary>
        /// 虚拟机核心运行标志
        /// </summary>
        private bool RunFlag;


        /// <summary>
        /// 程序代码执行器基类构造函数
        /// </summary>
        /// <param name="core_id">核心编号</param>
        /// <param name="control">核心控制器接口</param>
        /// <param name="complex">复杂运算单元接口</param>
        /// <param name="cache_size">缓存大小</param>
        public CodeExecutor(int core_id, IExecutorControl control, IComplexOperation complex, int cache_size = 3)
        {
            CoreID = core_id;
            ExecutorControl = control;
            ALU = new CoreModule.CoreModule(complex);
            Cache = new L1_Cache(this, cache_size);
            RunFlag = true;
        }

        /// <summary>
        /// 暂停核心
        /// </summary>
        public void Pause()
        {
            RunFlag = false;
        }
        /// <summary>
        /// 开始核心
        /// </summary>
        public void Reset()
        {
            RunFlag = true;
        }


        /// <summary>
        /// 开始进程
        /// </summary>
        /// <param name="process">进程组</param>
        public void Start(Process process)
        {
            if (null != process)
            {
                Process = process;
                Process.Running = true;
                Flag = true;
                StartTime = DateTime.Now;
                Thread = new Thread(new ThreadStart(RunMain));
                Thread.Start();
            }
        }
        /// <summary>
        /// 结束进程
        /// </summary>
        /// <returns>返回运行时间</returns>
        public void Stop()
        {
            if (Flag)
                Flag = false;
            if (null != Process)
            {
                Process.Running = false;
                Process.Time += DateTime.Now - StartTime;
                Process = null;
            }
        }

        private void RunMain()
        {
            while (Flag)
            {
                while (!RunFlag)
                    Thread.Sleep(50);

                Run();
            }
        }
        /// <summary>
        /// 执行方法
        /// </summary>
        protected abstract void Run();
        /// <summary>
        /// 中断接口函数
        /// </summary>
        /// <param name="coreid">核心编号</param>
        /// <param name="interrupt">设备中断号</param>
        /// <param name="data">设备ID</param>
        protected object Interrupt(int coreid, InterruptCode interrupt, params object[] data)
        {
            return ExecutorControl.Interrupt(coreid, interrupt, data);
        }

        /// <summary>
        /// 空指令操作
        /// </summary>
        protected void Instruction_Nop()
        {
            Thread.Sleep(5);
        }
        /// <summary>
        /// 回调指令操作
        /// </summary>
        protected void Instruction_Ret()
        {
            Process.Ret();
        }
        /// <summary>
        /// 无条件跳转指令操作
        /// </summary>
        /// <param name="label">跳转标签</param>
        protected void Instruction_Jmp(string label)
        {
            Process.Jmp(label);
        }
        /// <summary>
        /// 正跳转指令操作
        /// </summary>
        /// <param name="label">跳转标签</param>
        protected void Instruction_JNS(string label)
        {
            if (Symbol_Flag.Positive == ALU.Symbol)
                Process.Jmp(label);
        }
        /// <summary>
        /// 负跳转指令操作
        /// </summary>
        /// <param name="label">跳转标签</param>
        protected void Instruction_JS(string label)
        {
            if (Symbol_Flag.Negative == ALU.Symbol)
                Process.Jmp(label);
        }
        /// <summary>
        /// 零跳转指令操作
        /// </summary>
        /// <param name="label">跳转标签</param>
        protected void Instruction_JZ(string label)
        {
            if (Symbol_Flag.Zero == ALU.Symbol)
                Process.Jmp(label);
        }
        /// <summary>
        /// 核心休眠操作指令
        /// </summary>
        /// <param name="parameter">指令参数</param>
        protected void Instruction_Sleep(ParameterPackage parameter)
        {
            Instruction_GetData(parameter, out object time);
            Thread.Sleep((int)(long)time);
        }
        /// <summary>
        /// 公共数据入栈指令
        /// </summary>
        /// <param name="parameter">指令参数</param>
        protected void Instruction_Push(ParameterPackage parameter)
        {
            Instruction_GetData(parameter, out object value, true);
            if (4 > (value as byte[]).Length) 
            {
                byte[] vs = new byte[4];
                for (int i = 0; i < (value as byte[]).Length; i++)
                    vs[i] = (value as byte[])[i];
                value = vs;
            }
            if (!Process.Push((value as byte[])[0], (value as byte[])[1], (value as byte[])[2], (value as byte[])[3]))
                Interrupt(CoreID, InterruptCode.DataException_StackOverFlow);
        }
        /// <summary>
        /// 公共数据出栈指令
        /// </summary>
        /// <param name="parameter">指令参数</param>
        protected void Instruction_Pop(ParameterPackage parameter)
        {
            byte[] vs = new byte[4];
            if (Process.Pop(out vs[0], out vs[1], out vs[2], out vs[3]))
            {
                Instruction_GetData(parameter, out object value, false);
                Cache.Set(Process.Segment, (long)value, vs);
            }
            else
                Interrupt(CoreID, InterruptCode.DataException_StackOutOfBounds);
        }
        /// <summary>
        /// 计算指令操作
        /// </summary>
        /// <param name="parameters">指令参数</param>
        protected void Instruction_Calculate(ParameterPackage[] parameters)
        {
            Instruction_GetData(parameters[1], out object size);
            Instruction_GetData(parameters[2], out object page);
            Queue<CalculateParameter> calculates = new Queue<CalculateParameter>();
            for (int i = 3; i < parameters.Length; i++)
            {
                Instruction_GetData(parameters[i], out object value, true);
                calculates.Enqueue(new CalculateParameter
                {
                    Data = (byte[])value
                });
            }
            byte[] temp = ALU.Calculate(out bool of, (string)parameters[0].Parameter, calculates.ToArray());
            if (!of)
            {
                byte[] bytes = new byte[(long)size > 0 ? (long)size : 8];
                for (int i = 0; i < bytes.Length && i < temp.Length; i++)
                    bytes[i] = temp[i];
                Cache.Set(Process.Segment, (long)page, bytes);
            }
            else
                Interrupt(CoreID, InterruptCode.MathException_OverFlow);
        }
        /// <summary>
        /// 随机数生成操作
        /// </summary>
        /// <param name="parameters">指令参数</param>
        protected void Instruction_Rand(ParameterPackage[] parameters)
        {
            Instruction_GetData(parameters[0], out object page);
            Random random = new Random();
            if(2==parameters.Length)
            {
                Instruction_GetData(parameters[1], out object start);
                Cache.Set(Process.Segment, (long)page, BitConverter.GetBytes(random.Next((int)(long)start)));
            }
            else
            {
                Instruction_GetData(parameters[1], out object start);
                Instruction_GetData(parameters[2], out object end);
                Cache.Set(Process.Segment, (long)page, BitConverter.GetBytes(random.Next((int)(long)start, (int)(long)end)));
            }
        }
        /// <summary>
        /// 键盘输入操作
        /// </summary>
        /// <param name="parameters">指令参数</param>
        protected void Instruction_Scan(ParameterPackage[] parameters)
        {
            Instruction_GetData(parameters[0], out object page);
            Instruction_GetData(parameters[1], out object size);
            byte[] data = (byte[])Interrupt(CoreID, InterruptCode.BehaciourOperation_KeyBoardInput, (long)size / 2);
            Cache.Set(Process.Segment, (long)page, data);
        }
        /// <summary>
        /// 屏幕显示操作
        /// </summary>
        /// <param name="parameters">指令参数</param>
        protected void Instruction_Print(ParameterPackage[] parameters)
        {
            if (1 == parameters.Length)
                Interrupt(CoreID, InterruptCode.BehaciourOperation_ScreenPrint, (string)parameters[0].Parameter);
            else
            {
                Instruction_GetData(parameters[0], out object page);
                Instruction_GetData(parameters[1], out object size);
                Interrupt(CoreID, InterruptCode.BehaciourOperation_ScreenPrint, Encoding.Unicode.GetString(Cache.Get(Process.Segment, (long)page, (int)(long)size)));
            }
        }
        /// <summary>
        /// 屏幕显示操作（单字符）
        /// </summary>
        /// <param name="parameter">指令参数</param>
        protected void Instruction_Putc(ParameterPackage parameter)
        {
            Instruction_GetData(parameter, out object page);
            Interrupt(CoreID, InterruptCode.BehaciourOperation_ScreenPrint, Encoding.Unicode.GetString(Cache.Get(Process.Segment, (long)page, 2)));
        }
        /// <summary>
        /// 网络连接创建操作
        /// </summary>
        /// <param name="parameters">指令参数</param>
        protected void Instruction_Connect(ParameterPackage[] parameters)
        {
            Instruction_GetData(parameters[2], out object port);
            string ip = "";
            IP_Type ip_t = IP_Type.IPv4;
            Connection_Model model = Connection_Model.Mix;
            Protocol_Type protocol = Protocol_Type.TCP_IP;
            for(int i = 3; i < parameters.Length; i++)
            {
                bool flag = Instruction_GetData(parameters[i], out object value);
                if (flag)
                    switch ((value as string).ToUpper())
                    {
                        case "四代":
                        case "IPV4":
                            ip_t = IP_Type.IPv4;
                            break;
                        case "六代":
                        case "IPV6":
                            ip_t = IP_Type.IPv6;
                            break;
                        case "TCP_IP":
                            protocol = Protocol_Type.TCP_IP;
                            break;
                        case "UDP":
                            protocol = Protocol_Type.UDP;
                            break;
                        case "混合":
                        case "MIX":
                            model = Connection_Model.Mix;
                            break;
                        case "服务器":
                        case "SERVER":
                            model = Connection_Model.Server;
                            break;
                        case "客户端":
                        case "CLIENT":
                            model = Connection_Model.Client;
                            break;
                    }
                else
                    switch ((long)value)
                    {
                        case 6:
                            model = Connection_Model.Mix;
                            break;
                        case 1:
                            ip_t = IP_Type.IPv6;
                            break;
                        case 2:
                            protocol = Protocol_Type.TCP_IP;
                            break;
                        case 3:
                            protocol = Protocol_Type.UDP;
                            break;
                        case 4:
                            model = Connection_Model.Server;
                            break;
                        case 5:
                            model = Connection_Model.Client;
                            break;
                        case 0:
                        default:
                            ip_t = IP_Type.IPv4;
                            break;
                    }
            }
            bool type = Instruction_GetData(parameters[1], out object ips);
            if (type)
                ip = (string)ips;
            else
            {
                if (IP_Type.IPv4 == ip_t)
                {
                    byte[] vs = Cache.Get(Process.Segment, (long)ips, 4);
                    if (null != vs && 4 == vs.Length)
                        for (int i = 0; i < vs.Length - 1; i++)
                            ip += (int)vs[i] + ".";
                    ip += (int)vs[vs.Length - 1];
                }
                else
                {
                    byte[] vs = Cache.Get(Process.Segment, (long)ips, 16);
                    if (null != vs && 16 == vs.Length)
                        for (int i = 0; i < vs.Length - 1; i++)
                            ip += (int)vs[i] + ":";
                    ip += (int)vs[vs.Length - 1];
                }
            }
            Process.AddConnect((string)parameters[0].Parameter, (int)Interrupt(CoreID, InterruptCode.NetOperation_NetConnetionCreate, ip, (short)(long)port, ip_t, protocol, model));
        }
        /// <summary>
        /// 网络数据发送操作
        /// </summary>
        /// <param name="parameters">指令参数</param>
        protected void Instruction_Send(ParameterPackage[] parameters)
        {
            int id = Process.GetConnect((string)parameters[0].Parameter);
            if (-1 != id)
            {
                if (2 == parameters.Length)
                    Interrupt(CoreID, InterruptCode.NetOperation_NetDataSend, id, Encoding.Unicode.GetBytes((string)parameters[1].Parameter));
                else
                {
                    Instruction_GetData(parameters[1], out object page);
                    Instruction_GetData(parameters[2], out object size);
                    Interrupt(CoreID, InterruptCode.NetOperation_NetDataSend, id, Cache.Get(Process.Segment, (long)page, (int)(long)size));
                }
            }
            else
                Interrupt(CoreID, InterruptCode.BehaciourException_DataSourceEmpty);
        }
        /// <summary>
        /// 网络数据接收操作
        /// </summary>
        /// <param name="parameters">指令参数</param>
        protected void Instruction_Receive(ParameterPackage[] parameters)
        {
            int id = Process.GetConnect((string)parameters[0].Parameter);
            if (-1 != id)
            {
                byte[] vs = (byte[])Interrupt(CoreID, InterruptCode.NetOperation_NetDataReceive, id);
                if (null != vs && 0 < vs.Length)
                {
                    ApplyMemoryPackage address = (ApplyMemoryPackage)Interrupt(CoreID, InterruptCode.DataOperation_MemoryApply, vs.LongLength);
                    if (-1 != address.Address)
                    {
                        Process.AddMemory(address.Address - Process.Segment, (int)address.RealizeSize);
                        Cache.Set(Process.Segment, address.Address - Process.Segment, vs);
                        Instruction_GetData(parameters[1], out object page);
                        Cache.Set(Process.Segment, (long)page, BitConverter.GetBytes((int)(address.Address - Process.Segment)));
                    }
                    else
                        Interrupt(CoreID, InterruptCode.DataException_MemoryOverFlow);
                }
            }
            else
                Interrupt(CoreID, InterruptCode.BehaciourException_DataSourceEmpty);
        }
        /// <summary>
        /// 网络获取端口指令操作
        /// </summary>
        /// <param name="parameters">参数列表</param>
        protected void Instruction_GetPort(ParameterPackage[] parameters)
        {
            bool flag = Instruction_GetData(parameters[1], out object value);
            Port_Type type = Port_Type.Auto;
            if (flag) 
                switch ((value as string).ToUpper())
                {
                    case "服务器":
                    case "SERVER":
                        type = Port_Type.Server;
                        break;
                    case "客户端":
                    case "CLIENT":
                        type = Port_Type.Client;
                        break;
                    case "自动":
                    case "AUTO":
                        type = Port_Type.Auto;
                        break;
                }
            else
                switch ((long)value)
                {
                    case 7:
                        type = Port_Type.Server;
                        break;
                    case 8:
                        type = Port_Type.Client;
                        break;
                    case 9:
                    default:
                        type = Port_Type.Auto;
                        break;
                }
            int id = Process.GetConnect((string)parameters[0].Parameter);
            if (-1 != id)
            {
                int port = (int)Interrupt(CoreID, InterruptCode.NetOperation_NetConnetionPortGet, id, type);
                Instruction_GetData(parameters[2], out object page);
                Cache.Set(Process.Segment, (long)page, BitConverter.GetBytes(port));
            }
            else
                Interrupt(CoreID, InterruptCode.DataException_NullPoint);
        }
        /// <summary>
        /// 创建文件指令操作
        /// </summary>
        /// <param name="parameters">参数列表</param>
        protected void Instruction_Create(ParameterPackage[] parameters)
        {
            File_Type type = File_Type.File;
            string path;
            if (1 == parameters.Length)
                path = (string)parameters[0].Parameter;
            else
            {
                bool flag;
                object value;
                if (2 == parameters.Length)
                {
                    path = (string)parameters[0].Parameter;
                    flag = Instruction_GetData(parameters[1], out value);
                }
                else
                {
                    Instruction_GetData(parameters[0], out object page);
                    Instruction_GetData(parameters[1], out object size);
                    path = Encoding.Unicode.GetString(Cache.Get(Process.Segment, (long)page, (int)(long)size));
                    flag = Instruction_GetData(parameters[2], out value);
                }
                if (flag)
                    switch ((value as string).ToUpper())
                    {
                        case "目录":
                        case "ENTRY":
                            type = File_Type.Entry;
                            break;
                        case "文件":
                        case "FILE":
                        default:
                            type = File_Type.File;
                            break;
                    }
                else
                    switch ((long)value)
                    {
                        case 0:
                            type = File_Type.Entry;
                            break;
                        case 1:
                        default:
                            type = File_Type.File;
                            break;
                    }
            }
            Interrupt(CoreID, InterruptCode.FileOperation_FileCreate, path, type);
        }
        /// <summary>
        /// 删除文件指令操作
        /// </summary>
        /// <param name="parameters">参数列表</param>
        protected void Instruction_Delete(ParameterPackage[] parameters)
        {
            File_Type type = File_Type.File;
            string path;
            if (1 == parameters.Length)
                path = (string)parameters[0].Parameter;
            else
            {
                bool flag;
                object value;
                if (2 == parameters.Length)
                {
                    path = (string)parameters[0].Parameter;
                    flag = Instruction_GetData(parameters[1], out value);
                }
                else
                {
                    Instruction_GetData(parameters[0], out object page);
                    Instruction_GetData(parameters[1], out object size);
                    path = Encoding.Unicode.GetString(Cache.Get(Process.Segment, (long)page, (int)(long)size));
                    flag = Instruction_GetData(parameters[2], out value);
                }
                if (flag)
                    switch ((value as string).ToUpper())
                    {
                        case "目录":
                        case "ENTRY":
                            type = File_Type.Entry;
                            break;
                        case "文件":
                        case "FILE":
                        default:
                            type = File_Type.File;
                            break;
                    }
                else
                    switch ((long)value)
                    {
                        case 0:
                            type = File_Type.Entry;
                            break;
                        case 1:
                        default:
                            type = File_Type.File;
                            break;
                    }
            }
            Interrupt(CoreID, InterruptCode.FileOperation_FileDelete, path, type);
        }
        /// <summary>
        /// 设置文件指针指令操作
        /// </summary>
        /// <param name="parameters">参数列表</param>
        protected void Instruction_Position(ParameterPackage[] parameters)
        {
            Instruction_GetData(parameters[1], out object position);
            int id = Process.GetFile((string)parameters[0].Parameter);
            if (-1 != id)
                Interrupt(CoreID, InterruptCode.FileOperation_FilePositionMove, id, (long)position);
            else
                Interrupt(CoreID, InterruptCode.BehaciourException_DataSourceEmpty);
        }
        /// <summary>
        /// 申请内存指令操作
        /// </summary>
        /// <param name="parameters">参数列表</param>
        protected void Instruction_NewMemory(ParameterPackage[] parameters)
        {
            Instruction_GetData(parameters[1], out object size);
            ApplyMemoryPackage address = (ApplyMemoryPackage)Interrupt(CoreID, InterruptCode.DataOperation_MemoryApply, (int)(long)size);
            if (-1 != address.Address)
            {
                Process.AddMemory(address.Address - Process.Segment, (int)address.RealizeSize);
                Instruction_GetData(parameters[0], out object page);
                Cache.Set(Process.Segment, (long)page, BitConverter.GetBytes((int)(address.Address - Process.Segment)));
            }
            else
                Interrupt(CoreID, InterruptCode.DataException_MemoryOverFlow);
        }
        /// <summary>
        /// 释放内存指令操作
        /// </summary>
        /// <param name="parameters">参数列表</param>
        protected void Instruction_FreeMemory(ParameterPackage[] parameters)
        {
            Instruction_GetData(parameters[0], out object page);
            Process.DelMemory((long)page);
            Cache.Bad(Process.Segment, (long)page);
            Interrupt(CoreID, InterruptCode.DataOperation_MemoryRelease, Process.Segment + (long)page);
        }
        /// <summary>
        /// 初始化内存指令操作
        /// </summary>
        /// <param name="parameters">参数列表</param>
        protected void Instruction_InitMemory(ParameterPackage[] parameters)
        {
            Instruction_GetData(parameters[0], out object page);
            Instruction_GetData(parameters[1], out object optype);
            Instruction_GetData(parameters[2], out object immedit);
            Instruction_GetData(parameters[3], out object dup);
            byte[] vs = new byte[(long)(0 >= (long)optype ? 4 : optype)];
            for (int i = 0; i < vs.Length; i++)
            {
                vs[i] = (byte)((long)immedit % 256);
                immedit = (long)immedit / 256;
            }
            for (int i = 0; i < (long)(0 >= (long)dup ? 1 : (long)dup); i++)
            {
                Cache.Set(Process.Segment, (long)page, vs);
                page = (long)page + vs.LongLength;
            }
        }
        /// <summary>
        /// 读时间指令操作
        /// </summary>
        /// <param name="parameters">参数列表</param>
        protected void Instruction_ReadTime(ParameterPackage[] parameters)
        {
            bool flag = Instruction_GetData(parameters[1], out object value);
            TimeOperation_Type type;
            if (flag)
                switch ((value as string).ToUpper())
                {
                    case "SECOND":
                    case "秒":
                        type = TimeOperation_Type.Second;
                        break;
                    case "MINUTE":
                    case "分":
                        type = TimeOperation_Type.Minute;
                        break;
                    case "HOUR":
                    case "小时":
                        type = TimeOperation_Type.Hour;
                        break;
                    case "DAY":
                    case "日":
                        type = TimeOperation_Type.Day;
                        break;
                    case "MONTH":
                    case "月":
                        type = TimeOperation_Type.Month;
                        break;
                    case "YEAR":
                    case "年":
                    default:
                        type = TimeOperation_Type.Year;
                        break;
                }
            else
                switch ((long)value)
                {
                    case 5:
                        type = TimeOperation_Type.Second;
                        break;
                    case 1:
                        type = TimeOperation_Type.Month;
                        break;
                    case 2:
                        type = TimeOperation_Type.Day;
                        break;
                    case 3:
                        type = TimeOperation_Type.Hour;
                        break;
                    case 4:
                        type = TimeOperation_Type.Minute;
                        break;
                    case 0:
                    default:
                        type = TimeOperation_Type.Year;
                        break;
                }
            Instruction_GetData(parameters[0], out object page);
            Cache.Set(Process.Segment, (long)page, BitConverter.GetBytes((short)Interrupt(CoreID, InterruptCode.DataOperation_TimeGet, type)));
        }
        /// <summary>
        /// 写时间指令操作
        /// </summary>
        /// <param name="parameters">参数列表</param>
        protected void Instruction_WriteTime(ParameterPackage[] parameters)
        {
            bool flag = Instruction_GetData(parameters[1], out object value);
            TimeOperation_Type type;
            if (flag)
                switch ((value as string).ToUpper())
                {
                    case "SECOND":
                    case "秒":
                        type = TimeOperation_Type.Second;
                        break;
                    case "MINUTE":
                    case "分":
                        type = TimeOperation_Type.Minute;
                        break;
                    case "HOUR":
                    case "小时":
                        type = TimeOperation_Type.Hour;
                        break;
                    case "DAY":
                    case "日":
                        type = TimeOperation_Type.Day;
                        break;
                    case "MONTH":
                    case "月":
                        type = TimeOperation_Type.Month;
                        break;
                    case "YEAR":
                    case "年":
                    default:
                        type = TimeOperation_Type.Year;
                        break;
                }
            else
                switch ((long)value)
                {
                    case 5:
                        type = TimeOperation_Type.Second;
                        break;
                    case 1:
                        type = TimeOperation_Type.Month;
                        break;
                    case 2:
                        type = TimeOperation_Type.Day;
                        break;
                    case 3:
                        type = TimeOperation_Type.Hour;
                        break;
                    case 4:
                        type = TimeOperation_Type.Minute;
                        break;
                    case 0:
                    default:
                        type = TimeOperation_Type.Year;
                        break;
                }
            Instruction_GetData(parameters[0], out object page);
            Interrupt(CoreID, InterruptCode.DataOperation_TimeSet, BitConverter.ToInt16(Cache.Get(Process.Segment, (long)page, 2), 0), type);
        }
        /// <summary>
        /// 同步时间操作
        /// </summary>
        protected void Instruction_SyncTime()
        {
            Interrupt(CoreID, InterruptCode.DataOperation_TimeSynchronize);
        }
        /// <summary>
        /// 激活设备指令操作
        /// </summary>
        /// <param name="parameter">参数列表</param>
        protected void Instruction_ActDevice(ParameterPackage parameter)
        {
            Instruction_GetData(parameter, out object id);
            Interrupt(CoreID, InterruptCode.DeviceOperation_DeviceActive, (int)(long)id);
        }
        /// <summary>
        /// 暂停设备指令操作
        /// </summary>
        /// <param name="parameter">参数列表</param>
        protected void Instruction_PauseDevice(ParameterPackage parameter)
        {
            Instruction_GetData(parameter, out object id);
            Interrupt(CoreID, InterruptCode.DeviceOperation_DevicePause, (int)(long)id);
        }
        /// <summary>
        /// 复位设备指令操作
        /// </summary>
        /// <param name="parameter">参数列表</param>
        protected void Instruction_ResetDevice(ParameterPackage parameter)
        {
            Instruction_GetData(parameter, out object id);
            Interrupt(CoreID, InterruptCode.DeviceOperation_DeviceReset, (int)(long)id);
        }
        /// <summary>
        /// 关闭设备指令操作
        /// </summary>
        /// <param name="parameter">参数列表</param>
        protected void Instruction_CloseDevice(ParameterPackage parameter)
        {
            Instruction_GetData(parameter, out object id);
            Interrupt(CoreID, InterruptCode.DeviceOperation_DeviceClose, (int)(long)id);
        }
        /// <summary>
        /// 读设备指令操作
        /// </summary>
        /// <param name="parameters">参数列表</param>
        protected void Instruction_ReadDevice(ParameterPackage[] parameters)
        {
            Instruction_GetData(parameters[0], out object device_id);
            byte[] vs = (byte[])Interrupt(CoreID, InterruptCode.DeviceOperation_DeviceRead, (int)(long)device_id);
            if (null != vs)
            {
                ApplyMemoryPackage address = (ApplyMemoryPackage)Interrupt(CoreID, InterruptCode.DataOperation_MemoryApply, vs.Length);
                if (-1 != address.Address)
                {
                    Process.AddMemory(address.Address - Process.Segment, (int)address.RealizeSize);
                    Cache.Set(Process.Segment, address.Address - Process.Segment, vs);
                    Instruction_GetData(parameters[1], out object page);
                    Cache.Set(Process.Segment, (long)page, BitConverter.GetBytes((int)(address.Address - Process.Segment)));
                }
                else
                    Interrupt(CoreID, InterruptCode.DataException_MemoryOverFlow);
            }
        }
        /// <summary>
        /// 写设备指令操作
        /// </summary>
        /// <param name="parameters">参数列表</param>
        protected void Instruction_WriteDevice(ParameterPackage[] parameters)
        {
            Instruction_GetData(parameters[0], out object id);
            byte[] data;
            if (2 == parameters.Length)
                data = Encoding.Unicode.GetBytes((string)parameters[1].Parameter);
            else
            {
                Instruction_GetData(parameters[1], out object page);
                Instruction_GetData(parameters[2], out object size);
                data = Cache.Get(Process.Segment, (long)page, (int)(long)size);
            }
            Interrupt(CoreID, InterruptCode.DeviceOperation_DeviceWrite, (int)(long)id, data);
        }
        /// <summary>
        /// 暂停网络指令操作
        /// </summary>
        /// <param name="parameter">参数列表</param>
        protected void Instruction_PauseNet(ParameterPackage parameter)
        {
            int id = Process.GetConnect((string)parameter.Parameter);
            if (-1 != id)
                Interrupt(CoreID, InterruptCode.NetOperation_NetConnetionPause, id);
            else
                Interrupt(CoreID, InterruptCode.BehaciourException_DataSourceEmpty);
        }
        /// <summary>
        /// 复位网络指令操作
        /// </summary>
        /// <param name="parameter">参数列表</param>
        protected void Instruction_ResetNet(ParameterPackage parameter)
        {
            int id = Process.GetConnect((string)parameter.Parameter);
            if (-1 != id)
                Interrupt(CoreID, InterruptCode.NetOperation_NetConnetionReset, id);
            else
                Interrupt(CoreID, InterruptCode.BehaciourException_DataSourceEmpty);
        }
        /// <summary>
        /// 关闭网络指令操作
        /// </summary>
        /// <param name="parameter">参数列表</param>
        protected void Instruction_CloseNet(ParameterPackage parameter)
        {
            int id = Process.GetConnect((string)parameter.Parameter);
            if (-1 != id)
                Interrupt(CoreID, InterruptCode.NetOperation_NetConnetionClose, id);
            else
                Interrupt(CoreID, InterruptCode.BehaciourException_DataSourceEmpty);
        }
        /// <summary>
        /// 打开文件指令操作
        /// </summary>
        /// <param name="parameters">参数列表</param>
        protected void Instruction_OpenFile(ParameterPackage[] parameters)
        {
            string label = (string)parameters[0].Parameter;
            string path;
            object value;
            bool flag;
            if (3 == parameters.Length)
            {
                path = (string)parameters[1].Parameter;
                flag = Instruction_GetData(parameters[2], out value);
            }
            else
            {
                Instruction_GetData(parameters[1], out object page);
                Instruction_GetData(parameters[2], out object size);
                path = Encoding.Unicode.GetString(Cache.Get(Process.Segment, (long)page, (int)(long)size));
                flag = Instruction_GetData(parameters[3], out value);
            }

            Write_Type type;
            if (flag)
                switch ((value as string).ToUpper())
                {
                    case "APPEND":
                    case "追加":
                        type = Write_Type.Append;
                        break;
                    case "CREATE":
                    case "新建":
                        type = Write_Type.Create;
                        break;
                    case "COVER":
                    case "覆盖":
                    default:
                        type = Write_Type.Cover;
                        break;
                }
            else
                switch ((long)value)
                {
                    case 2:
                        type = Write_Type.Create;
                        break;
                    case 0:
                        type = Write_Type.Append;
                        break;
                    case 1:
                    default:
                        type = Write_Type.Cover;
                        break;
                }
            int id = (int)Interrupt(CoreID, InterruptCode.FileOperation_FileOpen, path, type);
            if (-1 != id)
            {
                int id_ole = Process.AddFile(label, id);
                if (-1 != id_ole)
                    Interrupt(CoreID, InterruptCode.FileOperation_FileClose, id_ole);
            }
        }
        /// <summary>
        /// 关闭文件指令操作
        /// </summary>
        /// <param name="parameter">参数列表</param>
        protected void Instruction_CloseFile(ParameterPackage parameter)
        {
            int id = Process.GetFile((string)parameter.Parameter);
            if (-1 != id)
            {
                Process.DelFile((string)parameter.Parameter);
                Interrupt(CoreID, InterruptCode.FileOperation_FileClose, id);
            }
            else
                Interrupt(CoreID, InterruptCode.BehaciourException_DataSourceEmpty);
        }
        /// <summary>
        /// 读文件指令操作
        /// </summary>
        /// <param name="parameters">参数列表</param>
        protected void Instruction_ReadFile(ParameterPackage[] parameters)
        {
            int id = Process.GetFile((string)parameters[0].Parameter);
            if (-1 != id)
            {
                Instruction_GetData(parameters[1], out object page);
                Instruction_GetData(parameters[2], out object size);
                Cache.Set(Process.Segment, (long)page, (byte[])Interrupt(CoreID, InterruptCode.FileOperation_FileRead, id, (int)(long)size));
            }
            else
                Interrupt(CoreID, InterruptCode.BehaciourException_DataSourceEmpty);
        }
        /// <summary>
        /// 写文件指令操作
        /// </summary>
        /// <param name="parameters">参数列表</param>
        protected void Instruction_WriteFile(ParameterPackage[] parameters)
        {
            int id = Process.GetFile((string)parameters[0].Parameter);
            if (-1 != id)
            {
                Instruction_GetData(parameters[1], out object page);
                Instruction_GetData(parameters[2], out object size);
                Interrupt(CoreID, InterruptCode.FileOperation_FileWrite, id, Cache.Get(Process.Segment, (long)page, (int)(long)size));
            }
            else
                Interrupt(CoreID, InterruptCode.BehaciourException_DataSourceEmpty);
        }
        /// <summary>
        /// 拷贝文件指令操作
        /// </summary>
        /// <param name="parameters">参数列表</param>
        protected void Instruction_CopyFile(ParameterPackage[] parameters)
        {
            string path_old, path_new;
            if (2 == parameters.Length)
            {
                path_old = (string)parameters[0].Parameter;
                path_new = (string)parameters[1].Parameter;
            }
            else
            {
                Instruction_GetData(parameters[0], out object page1);
                Instruction_GetData(parameters[1], out object size1);
                Instruction_GetData(parameters[2], out object page2);
                Instruction_GetData(parameters[3], out object size2);
                path_old = Encoding.Unicode.GetString(Cache.Get(Process.Segment, (long)page1, (int)(long)size1));
                path_new = Encoding.Unicode.GetString(Cache.Get(Process.Segment, (long)page2, (int)(long)size2));
            }
            Interrupt(CoreID, InterruptCode.FileOperation_FileCopy, path_old, path_new);
        }
        /// <summary>
        /// 初始化进程指令操作
        /// </summary>
        /// <param name="parameter">参数列表</param>
        protected void Instruction_InitProcess(ParameterPackage parameter)
        {
            Interrupt(CoreID, InterruptCode.CoreInterrupt_ProcessLoad, (string)parameter.Parameter);
        }
        /// <summary>
        /// 结束进程指令操作
        /// </summary>
        protected void Instruction_EndProcess()
        {
            Flag = false;
            Interrupt(CoreID, InterruptCode.CoreInterrupt_ProcessEnd);
        }
        /// <summary>
        /// 切换进程指令操作
        /// </summary>
        protected void Instruction_SwitchProcess()
        {
            Flag = false;
            Interrupt(CoreID, InterruptCode.CoreInterrupt_ProcessSwitch);
        }
        /// <summary>
        /// 载入程序指令操作
        /// </summary>
        /// <param name="parameters">参数列表</param>
        protected void Instruction_InitThread(ParameterPackage[] parameters)
        {
            ProcessAddPackage program = (ProcessAddPackage)Interrupt(CoreID, InterruptCode.CoreInterrupt_ThreadLoad, (string)parameters[1].Parameter);
            Process.AddProgram(program.Program_ID, (string)parameters[0].Parameter, program.Program);
        }
        /// <summary>
        /// 卸载程序指令操作
        /// </summary>
        /// <param name="parameter">参数列表</param>
        protected void Instruction_EndThread(ParameterPackage parameter)
        {
            Process.DelProgram((string)parameter.Parameter, out int id, out long address);
            Interrupt(CoreID, InterruptCode.CoreInterrupt_ThreadEnd, id, address);
        }
        /// <summary>
        /// 外部跳转指令操作
        /// </summary>
        /// <param name="parameters">参数列表</param>
        protected void Instruction_OutsideCall(ParameterPackage[] parameters)
        {
            Process.Call((string)parameters[1].Parameter, (string)parameters[0].Parameter);
        }
        /// <summary>
        /// 内存跳转指令操作
        /// </summary>
        /// <param name="parameter">参数列表</param>
        protected void Instruction_LocalCall(ParameterPackage parameter)
        {
            Process.Call((string)parameter.Parameter);
        }
        /// <summary>
        /// 结束程序指令操作
        /// </summary>
        protected void Instruction_End()
        {
            Flag = false;
            Interrupt(CoreID, InterruptCode.CoreInterrupt_ProcessEnd);
        }
        /// <summary>
        /// 修改时间片指令操作
        /// </summary>
        /// <param name="parameters">参数列表</param>
        protected void Instruction_SetTimeSlice(ParameterPackage[] parameters)
        {
            int times = 100, core;
            if (2 == parameters.Length)
            {
                Instruction_GetData(parameters[0], out object page);
                Instruction_GetData(parameters[1], out object coreid);
                core = (int)(long)coreid;
                times = Convert.ToInt32(Cache.Get(Process.Segment, (long)page, 4));
            }
            else
            {
                Instruction_GetData(parameters[0], out object coreid);
                core = (int)(long)coreid;
            }
            Interrupt(CoreID, InterruptCode.CoreInterrupt_TimeSliceSet, times, core);
        }
        /// <summary>
        /// 获取时间片指令操作
        /// </summary>
        /// <param name="parameters">参数列表</param>
        protected void Instruction_GetTimeSlice(ParameterPackage[] parameters)
        {
            Instruction_GetData(parameters[0], out object page);
            Instruction_GetData(parameters[1], out object coreid);
            Cache.Set(Process.Segment, (long)page, BitConverter.GetBytes((int)Interrupt(CoreID, InterruptCode.CoreInterrupt_TimeSliceGet, (int)(long)coreid)));
        }
        /// <summary>
        /// 获取核心数指令操作
        /// </summary>
        /// <param name="parameter">参数列表</param>
        protected void Instruction_GetCore(ParameterPackage parameter)
        {
            Instruction_GetData(parameter, out object page);
            Cache.Set(Process.Segment, (long)page, BitConverter.GetBytes((int)Interrupt(CoreID, InterruptCode.CoreInterrupt_CoreCountGet)));
        }

        /// <summary>
        /// 数据解析方法
        /// </summary>
        /// <param name="parameter">参数</param>
        /// <param name="value">转换值</param>
        /// <param name="arrays">是否为数组标识符</param>
        /// <returns>返回标志（true：字符串类型 | false：长整数类型）</returns>
        private bool Instruction_GetData(ParameterPackage parameter, out object value, bool arrays = false)
        {
            value = null;
            bool result = false;
            switch (parameter.Type)
            {
                case Parameter_Type.STR:
                    if (arrays)
                        value = Encoding.Unicode.GetBytes((string)parameter.Parameter);
                    else
                    {
                        result = true;
                        value = (string)parameter.Parameter;
                    }
                    break;
                case Parameter_Type.Expression:
                    if (arrays)
                        value = Calculate((Expression)parameter.Parameter);
                    else
                        value = BitConverter.ToInt64(Calculate((Expression)parameter.Parameter), 0);
                    break;
                case Parameter_Type.BigNumber:
                    if (arrays)
                        value = ((BigInteger)parameter.Parameter).ToByteArray();
                    else
                        value = (long)(BigInteger)parameter.Parameter;
                    break;
                case Parameter_Type.Long:
                    if (arrays)
                        value = BitConverter.GetBytes((long)parameter.Parameter);
                    else
                        value = (long)parameter.Parameter;
                    break;
            }
            return result;
        }
        /// <summary>
        /// 表达式求值方法
        /// </summary>
        /// <param name="expression">表达式</param>
        /// <returns>返回表达式结果</returns>
        private byte[] Calculate(Expression expression)
        {
            if (null != expression)
            {
                List<long> rl = new List<long>();
                for(int i = 0; i < expression.Count; i++)
                {
                    Calculate_GetData(expression[i], rl, out long r1, out long r2);
                    switch (expression[i].Operation)
                    {
                        case Expression.Expression_Operation.GET:
                            byte[] vs = Cache.Get(Process.Segment, r1, (int)r2);
                            if (null != vs)
                            {
                                long temp = 0;
                                for (int j = vs.Length - 1; j >= 0; j--)
                                    temp = temp * 256 + vs[j];
                                rl.Add(temp);
                            }
                            break;
                        case Expression.Expression_Operation.ADD:
                            rl.Add(r1 + r2);
                            break;
                        case Expression.Expression_Operation.SUB:
                            rl.Add(r1 - r2);
                            break;
                        case Expression.Expression_Operation.MUL:
                            rl.Add(r1 * r2);
                            break;
                        case Expression.Expression_Operation.DIV:
                            rl.Add(r1 / r2);
                            break;
                        case Expression.Expression_Operation.AND:
                            rl.Add(r1 & r2);
                            break;
                        case Expression.Expression_Operation.OR:
                            rl.Add(r1 | r2);
                            break;
                        case Expression.Expression_Operation.NOT:
                            rl.Add(~r1);
                            break;
                        case Expression.Expression_Operation.XOR:
                            rl.Add(r1 ^ r2);
                            break;
                    }
                }
                return BitConverter.GetBytes(rl[rl.Count - 1]);
            }
            return null;
        }
        /// <summary>
        /// 表达式求值子函数
        /// </summary>
        /// <param name="op"></param>
        /// <param name="rl"></param>
        /// <param name="r1"></param>
        /// <param name="r2"></param>
        private void Calculate_GetData(Expression.OP op, List<long> rl, out long r1, out long r2)
        {
            if (op.N1_Flag)
                r1 = rl[(int)(long)op.N1];
            else
                r1 = (long)op.N1;
            if (op.N2_Flag)
                r2 = rl[(int)(long)op.N2];
            else
                r2 = (long)op.N2;
        }

        /// <summary>
        /// 一级缓存控制器中断接口
        /// </summary>
        /// <param name="exception">异常类型</param>
        /// <param name="address">访问地址</param>
        /// <param name="size">访问大小</param>
        void IMemoryCacheInterrupt.Interrupt(MemoryCache_Type exception, long address, int size)
        {
            switch (exception)
            {
                case MemoryCache_Type.Out_Bounds:
                    Interrupt(CoreID, InterruptCode.DataException_ArraysOutOfBounds, address, size);
                    break;
                case MemoryCache_Type.Over_Flow:
                    Interrupt(CoreID, InterruptCode.DataException_MemoryOverFlow);
                    break;
            }
        }
        /// <summary>
        /// 缓存缺页中断
        /// </summary>
        /// <param name="address">访问地址</param>
        /// <param name="realize">访问实际地址</param>
        /// <returns>返回地址块</returns>
        MemoryBlock IMemoryCacheInterrupt.PageMissingInterrupt(long address, out long realize)
        {
            realize = -1;
            MemoryReadPackage block = (MemoryReadPackage)Interrupt(CoreID, InterruptCode.DataOperation_PageMissing, address);
            if (null != block)
                realize = block.RealizeAddress;
            return block.Block;
        }
    }

    /// <summary>
    /// 内存读取包
    /// </summary>
    public class MemoryReadPackage
    {
        /// <summary>
        /// 实际内存地址
        /// </summary>
        public long RealizeAddress;
        /// <summary>
        /// 内存块
        /// </summary>
        public MemoryBlock Block;
    }
    /// <summary>
    /// 线程加载包
    /// </summary>
    public class ProcessAddPackage
    {
        /// <summary>
        /// 线程ID
        /// </summary>
        public int Program_ID;
        /// <summary>
        /// 程序组
        /// </summary>
        public ProgramUnit Program;
    }
}
