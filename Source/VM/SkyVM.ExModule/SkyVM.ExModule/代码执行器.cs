using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using SkyVM.AnaModule;
using SkyVM.CoreModule;
using SkyVM.InterfaceModule;

namespace SkyVM.ExModule
{
    /// <summary>
    /// 上层控制界面接口
    /// </summary>
    public interface IApplication
    {
        /// <summary>
        /// 显示消息
        /// </summary>
        /// <param name="str">消息</param>
        void Print(string str);
        /// <summary>
        /// 获取消息
        /// </summary>
        /// <param name="size">数据大小</param>
        /// <returns>返回消息</returns>
        byte[] Scan(long size);
        /// <summary>
        /// 网络连接失效
        /// </summary>
        /// <param name="process">进程号</param>
        void NetCreateFailed(int process);
        /// <summary>
        /// 虚拟机关机
        /// </summary>
        void ShutDown();
    }
    /// <summary>
    /// 虚拟机代码执行器（用于代码执行）
    /// </summary>
    public abstract class CodeExecutorUnit : IToFileLoad, IExecutorControl
    {
        /// <summary>
        /// 获取文件操作表
        /// </summary>
        public Dictionary<int, FileStream> GetFiles
        {
            get
            {
                FileLock();
                Dictionary<int, FileStream> pairs = new Dictionary<int, FileStream>(FileTable);
                FileUnLock();
                return pairs;
            }
        }

        private readonly IApplication Application;
        private int App_Lock;

        private readonly int StackSize;

        private bool AddNewProgramFlag;

        /// <summary>
        /// 文件加载系统-由本层实现多线程原子锁
        /// </summary>
        private readonly FileLoad FileLoad;
        /// <summary>
        /// 文件加载系统锁
        /// </summary>
        private int FileLoad_Lock;

        /// <summary>
        /// 复杂运算核心系统-核心系统实现多线程原子锁
        /// </summary>
        private readonly IComplexOperation IComplex;

        /// <summary>
        /// IO设备控制器接口-由本层实现多线程原子锁
        /// </summary>
        private readonly IDevice IDevice;
        /// <summary>
        /// IO设备控制器锁
        /// </summary>
        private int Device_Lock;

        /// <summary>
        /// 网络控制器接口-由本层实现多线程原子锁
        /// </summary>
        private readonly INetControl INet;
        /// <summary>
        /// 网络控制器锁
        /// </summary>
        private int Net_Lock;

        /// <summary>
        /// 内存控制器接口-由本层实现多线程原子锁
        /// </summary>
        private readonly MemoryBase Memory;
        /// <summary>
        /// 内存控制器锁
        /// </summary>
        private int Memory_Lock;
        
        /// <summary>
        /// 文件操作表-由本层实现多线程原子锁
        /// </summary>
        private readonly Dictionary<int, FileStream> FileTable;
        /// <summary>
        /// 文件操作表锁
        /// </summary>
        private int File_Lock;

        /// <summary>
        /// 时钟操作-由本层实现多线程原子锁
        /// </summary>
        private readonly TimeBase Time;
        /// <summary>
        /// 时钟操作锁
        /// </summary>
        private int Time_Lock;

        /// <summary>
        /// 虚拟机运行核心
        /// </summary>
        private readonly ProcessCodeExecutorUnit[] Cores;
        private readonly CoreCodeExecutorUnit BackGroundCore;

        /// <summary>
        /// 空闲文件号表
        /// </summary>
        private readonly SortedList<int, int> FreeFileIDTable;

        /// <summary>
        /// 空闲线程号表
        /// </summary>
        private readonly SortedList<int, int> FreeProgramIDTable;

        /// <summary>
        /// 进程环-由本层实现多线程原子锁
        /// </summary>
        private readonly ProcessRing Ring;
        /// <summary>
        /// 进程环锁
        /// </summary>
        private int Ring_Lock;

        /// <summary>
        /// 虚拟机代码执行器构造函数
        /// </summary>
        /// <param name="application">上层应用界面</param>
        /// <param name="path">系统启动程序</param>
        /// <param name="memory">内存接口</param>
        /// <param name="core_num">虚拟机核心数</param>
        /// <param name="icomplex">复杂执行单元接口</param>
        /// <param name="time">时间操作接口</param>
        /// <param name="cache_size">缓存区大小</param>
        /// <param name="min_mem_size">进程内存分配最小量</param>
        /// <param name="idevice">IO设备接口</param>
        /// <param name="inet">网络接口</param>
        /// <param name="stack_size">进程堆栈数</param>
        public CodeExecutorUnit(IApplication application, string path, MemoryBase memory, int min_mem_size, IComplexOperation icomplex, TimeBase time, IDevice idevice = null, INetControl inet = null, int cache_size = 3, int core_num = 1, int stack_size = 100)
        {
            Application = application;
            App_Lock = 0;

            StackSize = stack_size;
            AddNewProgramFlag = true;

            FreeFileIDTable = new SortedList<int, int>
            {
                { 0, ushort.MaxValue }
            };

            FreeProgramIDTable = new SortedList<int, int>
            {
                { 0, ushort.MaxValue }
            };

            FileLoad = new FileLoad(min_mem_size, this);
            FileLoad_Lock = 0;

            IDevice = idevice;
            Device_Lock = 0;

            INet = inet;
            Net_Lock = 0;

            Memory = memory;
            Memory_Lock = 0;

            Time = time;
            Time_Lock = 0;

            FileTable = new Dictionary<int, FileStream>();
            File_Lock = 0;

            Ring = new ProcessRing();
            Ring_Lock = 0;

            IComplex = icomplex;
            
            Cores = new ProcessCodeExecutorUnit[core_num > 0 ? core_num : 0];
            for (int i = 0; i < Cores.Length; i++)
                Cores[i] = new ProcessCodeExecutorUnit(i, this, icomplex, cache_size);

            if (null != path)
            {
                BackGroundCore = new CoreCodeExecutorUnit(this, icomplex, cache_size);
                ProgramUnit program = FileLoad.Complete(path);
                if (null != program)
                {
                    program.MemorySpace = Memory.ApplyMemory(program.MemorySize, out long realize);
                    if (-1 != realize)
                    {
                        int id = GetProgramID();
                        if (-1 != id)
                            BackGroundCore.Start(new Process(id, program, stack_size)
                            {
                                Running = false,
                                Time = new TimeSpan(0, 0, 0, 0, 0)
                            });
                    }
                    else
                        Application.Print("虚拟机错误 内存溢出 停止创建进程");
                }

            }
        }

        /// <summary>
        /// 暂停核心
        /// </summary>
        public void Pause()
        {
            BackGroundCore.Pause();
            for (int i = 0; i < Cores.Length; i++)
                Cores[i].Pause();
        }
        /// <summary>
        /// 开始核心
        /// </summary>
        public void Reset()
        {
            BackGroundCore.Reset();
            for (int i = 0; i < Cores.Length; i++)
                Cores[i].Reset();
        }
        /// <summary>
        /// 关闭虚拟机方法
        /// </summary>
        public void Close()
        {
            if (null != BackGroundCore)
                BackGroundCore.Stop();
            ShutDown();
        }

        #region 操作锁
        private void AppLock()
        {
            while (0 != Interlocked.Exchange(ref App_Lock, 1))
                Thread.Sleep(10);
        }
        private void AppUnLock()
        {
            Interlocked.Exchange(ref App_Lock, 0);
        }

        private void FileLoadLock()
        {
            while (0 != Interlocked.Exchange(ref FileLoad_Lock, 1))
                Thread.Sleep(10);
        }
        private void FileLoadUnLock()
        {
            Interlocked.Exchange(ref FileLoad_Lock, 0);
        }
        
        /// <summary>
        /// 设备控制器上锁
        /// </summary>
        /// <returns>返回上锁结果</returns>
        public bool DeviceLock_O()
        {
            return 0 == Interlocked.Exchange(ref Device_Lock, 1);
        }
        private void DeviceLock()
        {
            while (0 != Interlocked.Exchange(ref Device_Lock, 1))
                Thread.Sleep(10);
        }
        /// <summary>
        /// 设备控制器解锁
        /// </summary>
        public void DeviceUnLock_O()
        {
            Interlocked.Exchange(ref Device_Lock, 0);
        }
        private void DeviceUnLock()
        {
            Interlocked.Exchange(ref Device_Lock, 0);
        }

        /// <summary>
        /// 网络控制器上锁
        /// </summary>
        /// <returns>返回上锁结果</returns>
        public bool NetLock_O()
        {
            return 0 == Interlocked.Exchange(ref Net_Lock, 1);
        }
        private void NetLock()
        {
            while (0 != Interlocked.Exchange(ref Net_Lock, 1))
                Thread.Sleep(10);
        }
        /// <summary>
        /// 网络控制器解锁
        /// </summary>
        public void NetUnLock_O()
        {
            Interlocked.Exchange(ref Net_Lock, 0);
        }
        private void NetUnLock()
        {
            Interlocked.Exchange(ref Net_Lock, 0);
        }

        /// <summary>
        /// 内存控制器上锁
        /// </summary>
        /// <returns>返回上锁结果</returns>
        public bool MemoryLock_O()
        {
            return 0 == Interlocked.Exchange(ref Memory_Lock, 1);
        }
        private void MemoryLock()
        {
            while (0 != Interlocked.Exchange(ref Memory_Lock, 1))
                Thread.Sleep(10);
        }
        /// <summary>
        /// 内存控制器解锁
        /// </summary>
        public void MemoryUnLock_O()
        {
            Interlocked.Exchange(ref Memory_Lock, 0);
        }
        private void MemoryUnLock()
        {
            Interlocked.Exchange(ref Memory_Lock, 0);
        }

        private void FileLock()
        {
            while (0 != Interlocked.Exchange(ref File_Lock, 1))
                Thread.Sleep(10);
        }
        private void FileUnLock()
        {
            Interlocked.Exchange(ref File_Lock, 0);
        }

        private void TimeLock()
        {
            while (0 != Interlocked.Exchange(ref Time_Lock, 1))
                Thread.Sleep(10);
        }
        private void TimeUnLock()
        {
            Interlocked.Exchange(ref Time_Lock, 0);
        }

        private void RingLock()
        {
            while (0 != Interlocked.Exchange(ref Ring_Lock, 1))
                Thread.Sleep(10);
        }
        private void RingUnLock()
        {
            Interlocked.Exchange(ref Ring_Lock, 0);
        }
        #endregion

        #region 标识符分配操作
        private int GetFileID()
        {
            int result = -1;

            if (0 < FreeFileIDTable.Count)
            {
                int key = FreeFileIDTable.Keys[0];
                int value = FreeFileIDTable[key];
                FreeFileIDTable.Remove(key);
                result = key;
                key++;
                if (key <= value)
                    FreeFileIDTable.Add(key, value);
            }

            return result;
        }
        private void FreeFileID(int interrupt_id)
        {
            if (!FreeFileIDTable.ContainsKey(interrupt_id))
            {
                FreeFileIDTable.Add(interrupt_id, interrupt_id);
                for (int i = 0; i < FreeFileIDTable.Count - 1; i++)
                {
                    int key = FreeFileIDTable.Keys[i];
                    int value = FreeFileIDTable.Values[i];
                    if (value + 1 == FreeFileIDTable.Keys[i + 1])
                    {
                        value = FreeFileIDTable.Values[i + 1];
                        int k2 = FreeFileIDTable.Keys[i + 1];
                        FreeFileIDTable.Remove(key);
                        FreeFileIDTable.Remove(k2);
                        FreeFileIDTable.Add(key, value);
                        break;
                    }
                }
            }
        }

        private int GetProgramID()
        {
            int result = -1;

            if (0 < FreeProgramIDTable.Count)
            {
                int key = FreeProgramIDTable.Keys[0];
                int value = FreeProgramIDTable[key];
                FreeProgramIDTable.Remove(key);
                result = key;
                key++;
                if (key <= value)
                    FreeProgramIDTable.Add(key, value);
            }

            return result;
        }
        private void FreeProgramID(int interrupt_id)
        {
            if (!FreeProgramIDTable.ContainsKey(interrupt_id))
            {
                FreeProgramIDTable.Add(interrupt_id, interrupt_id);
                for (int i = 0; i < FreeProgramIDTable.Count - 1; i++)
                {
                    int key = FreeProgramIDTable.Keys[i];
                    int value = FreeProgramIDTable.Values[i];
                    if (value + 1 == FreeProgramIDTable.Keys[i + 1])
                    {
                        value = FreeProgramIDTable.Values[i + 1];
                        int k2 = FreeProgramIDTable.Keys[i + 1];
                        FreeProgramIDTable.Remove(key);
                        FreeProgramIDTable.Remove(k2);
                        FreeProgramIDTable.Add(key, value);
                        break;
                    }
                }
            }
        }
        #endregion

        private void EndProcess(int process_id)
        {
            RingLock();
            Process process = Ring.GetProcess(process_id);
            RingUnLock();
            if (null != process)
            {
                int[] files = process.GetFiles();
                int[] connects = process.GetConnects();
                long[] memorys = process.GetAllMemorys();
                if (null != files)
                {
                    FileLock();
                    for (int i = 0; i < files.Length; i++)
                        if (FileTable.TryGetValue(files[i], out FileStream fs))
                        {
                            fs.Close();
                            FileTable.Remove(files[i]);
                        }
                    FileUnLock();
                }
                if (null != connects)
                {
                    NetLock();
                    for (int i = 0; i < connects.Length; i++)
                        INet.StopConnection(connects[i]);
                    NetUnLock();
                }
                if (null != memorys)
                {
                    MemoryLock();
                    for (int i = 0; i < memorys.Length; i++)
                        Memory.ReleaseMemory(memorys[i]);
                    MemoryUnLock();
                }
            }
            RingLock();
            Ring.DelProcess(process_id);
            RingUnLock();
        }

        bool IToFileLoad.CheckCalculateString(string str)
        {
            return IComplex.OperateTest(str);
        }
        void IToFileLoad.Interrupt(int exception_code, int line, string message, string path)
        {
            switch (exception_code)
            {
                case 0x00:
                    Print(string.Format("错误的语法  在第 {0} 行 \r\n {1} \r\n 文件：{2}", line, message, path));
                    break;
                case 0x01:
                    Print(string.Format("错误的方法类型  在第 {0} 行 \r\n {1} \r\n 文件：{2}", line, message, path));
                    break;
                case 0x10:
                    Print(string.Format("错误的字符  在第 {0} 行 \r\n {1} \r\n 文件：{2}", line, message, path));
                    break;
                case 0x20:
                    Print(string.Format("错误的运算  在第 {0} 行 \r\n {1} \r\n 文件：{2}", line, message, path));
                    break;
                case 0x21:
                    Print(string.Format("表达式中存在错误的符号  在第 {0} 行 \r\n {1} \r\n 文件：{2}", line, message, path));
                    break;
                case 0x22:
                    Print(string.Format("表达式中存在错误的运算符  在第 {0} 行 \r\n {1} \r\n 文件：{2}", line, message, path));
                    break;
                case 0x30:
                    Print(string.Format("文件读取异常\r\n {0} \r\n 文件：{1}", message, path));
                    break;
                case 0x31:
                    Print(string.Format("系统异常\r\n {0} \r\n 文件：{1}", message, path));
                    break;
            }
            AddNewProgramFlag = false;
        }

        object IExecutorControl.Interrupt(int coreid, InterruptCode interrupt, params object[] data)
        {
            switch (interrupt.IntCode)
            {
                case CodeTable.MathException_DivZero:
                    ERROR(coreid, "除零异常");
                    break;
                case CodeTable.MathException_OverFlow:
                    ERROR(coreid, "计算溢出");
                    break;

                case CodeTable.DataException_ArraysOutOfBounds:
                case CodeTable.DataException_NullPoint:
                    ERROR(coreid, "数据异常");
                    break;
                case CodeTable.DataOperation_PageMissing:
                    return DO_PAGE_MISS(coreid, (long)data[0]);
                case CodeTable.DataOperation_MemoryApply:
                    return DO_MEM_APPLY((int)data[0]);
                case CodeTable.DataOperation_MemoryRelease:
                    DO_MEM_FREE((long)data[0]);
                    break;
                case CodeTable.DataOperation_TimeGet:
                    {
                        TimeLock();
                        short time = Time.ReadTime((TimeOperation_Type)data[0]);
                        TimeUnLock();
                        return time;
                    }
                case CodeTable.DataOperation_TimeSet:
                    {
                        TimeLock();
                        Time.WriteTime((short)data[0], (TimeOperation_Type)data[1]);
                        TimeUnLock();
                    }
                    break;
                case CodeTable.DataOperation_TimeSynchronize:
                    {
                        TimeLock();
                        Time.SyncTime();
                        TimeUnLock();
                    }
                    break;
                case CodeTable.DataException_StackOverFlow:
                    ERROR(coreid, "堆栈溢出");
                    break;
                case CodeTable.DataException_StackOutOfBounds:
                    ERROR(coreid, "堆栈越界");
                    break;
                case CodeTable.DataException_MemoryOverFlow:
                    {
                        Print("虚拟机错误 内存溢出 即将结束虚拟机");
                        ShutDown();
                    }
                    break;

                case CodeTable.CoreInterrupt_ProcessSwitch:
                    CO_PE_SWITCH(coreid);
                    break;
                case CodeTable.CoreInterrupt_ProcessEnd:
                    CO_PE_END(coreid);
                    break;
                case CodeTable.CoreInterrupt_ThreadEnd:
                    CE_PE_CALLBACK((int)data[0], (long)data[1]);
                    break;
                case CodeTable.CoreInterrupt_ProcessLoad:
                    CO_PE_NEW((string)data[0]);
                    break;
                case CodeTable.CoreInterrupt_ThreadLoad:
                    return CO_TE_NEW((string)data[0]);

                case CodeTable.BehaciourOperation_KeyBoardInput:
                    return CO_KEY_IN((long)data[0]);
                case CodeTable.BehaciourOperation_ScreenPrint:
                    Print((string)data[0]);
                    break;
                case CodeTable.BehaciourException_DataSourceEmpty:
                    Print("访问对象为空【" + coreid + "】");
                    break;

                case CodeTable.NetOperation_NetConnetionCreate:
                    {
                        NetLock();
                        int id = INet.CreateConnection((string)data[0], (short)data[1], (IP_Type)data[2], (Protocol_Type)data[3], (Connection_Model)data[4]);
                        NetUnLock();
                        if (-1 != id)
                            return id;
                    }
                    break;
                case CodeTable.NetOperation_NetDataSend:
                    {
                        NetLock();
                        INet.Send((int)data[0], (byte[])data[1]);
                        NetUnLock();
                    }
                    break;
                case CodeTable.NetOperation_NetDataReceive:
                    {
                        NetLock();
                        byte[] vs = INet.Receive((int)data[0]);
                        NetUnLock();
                        return vs;
                    }
                case CodeTable.NetOperation_NetConnetionPortGet:
                    {
                        NetLock();
                        int port = INet.GetPort((int)data[0], (Port_Type)data[1]);
                        NetUnLock();
                        return port;
                    }
                case CodeTable.NetOperation_NetConnetionPause:
                    {
                        NetLock();
                        INet.PauseConnection((int)data[0]);
                        NetUnLock();
                    }
                    break;
                case CodeTable.NetOperation_NetConnetionReset:
                    {
                        NetLock();
                        INet.ResetConnection((int)data[0]);
                        NetUnLock();
                    }
                    break;
                case CodeTable.NetOperation_NetConnetionClose:
                    {
                        NetLock();
                        INet.StopConnection((int)data[0]);
                        NetUnLock();
                    }
                    break;

                case CodeTable.DeviceOperation_DeviceActive:
                    {
                        DeviceLock();
                        IDevice.DeviceActive((int)data[0]);
                        DeviceUnLock();
                    }
                    break;
                case CodeTable.DeviceOperation_DevicePause:
                    {
                        DeviceLock();
                        IDevice.DevicePause((int)data[0]);
                        DeviceUnLock();
                    }
                    break;
                case CodeTable.DeviceOperation_DeviceReset:
                    {
                        DeviceLock();
                        IDevice.DeviceReset((int)data[0]);
                        DeviceUnLock();
                    }
                    break;
                case CodeTable.DeviceOperation_DeviceClose:
                    {
                        DeviceLock();
                        IDevice.DeviceClose((int)data[0]);
                        DeviceUnLock();
                    }
                    break;
                case CodeTable.DeviceOperation_DeviceRead:
                    {
                        DeviceLock();
                        byte[] vs = IDevice.DeviceRead((int)data[0]);
                        DeviceUnLock();
                        return vs;
                    }
                case CodeTable.DeviceOperation_DeviceWrite:
                    {
                        DeviceLock();
                        IDevice.DeviceWrite((int)data[0], (byte[])data[1]);
                        DeviceUnLock();
                    }
                    break;

                case CodeTable.FileOperation_FileOpen:
                    return DO_FILE_OPEN((string)data[0], (Write_Type)data[1]);
                case CodeTable.FileOperation_FileClose:
                    {
                        FileLock();
                        if (FileTable.TryGetValue((int)data[0], out FileStream value))
                        {
                            value.Close();
                            FileTable.Remove((int)data[0]);
                            FreeProgramID((int)data[0]);
                        }
                        FileUnLock();
                    }
                    break;
                case CodeTable.FileOperation_FileRead:
                    {
                        FileLock();
                        byte[] vs = null;
                        if (FileTable.TryGetValue((int)(data[0]), out FileStream value))
                        {
                            vs = new byte[(int)(data[1])];
                            try
                            {
                                value.Read(vs, 0, vs.Length);
                            }
                            catch (Exception)
                            {

                            }
                        }
                        FileUnLock();
                        return vs;
                    }
                case CodeTable.FileOperation_FileWrite:
                    {
                        FileLock();
                        if (FileTable.TryGetValue((int)data[0], out FileStream value))
                            value.Write((byte[])data[1], 0, (data[1] as byte[]).Length);
                        FileUnLock();
                    }
                    break;
                case CodeTable.FileOperation_FileCopy:
                    {
                        FileLock();
                        File.Copy((string)data[0], (string)data[1]);
                        FileUnLock();
                    }
                    break;
                case CodeTable.FileOperation_FilePositionMove:
                    {
                        FileLock();
                        if (FileTable.TryGetValue((int)data[0], out FileStream value))
                            value.Position = (long)data[1];
                        FileUnLock();
                    }
                    break;
                case CodeTable.FileOperation_FileCreate:
                    DO_FILE_NEW((string)data[0], (File_Type)data[1]);
                    break;
                case CodeTable.FileOperation_FileDelete:
                    {
                        FileLock();
                        switch ((File_Type)data[1])
                        {
                            case File_Type.Entry:
                                if (Directory.Exists((string)data[0]))
                                    Directory.Delete((string)data[0]);
                                break;
                            case File_Type.File:
                                File.Delete((string)data[0]);
                                break;
                        }
                        FileUnLock();
                    }
                    break;

                case CodeTable.CoreInterrupt_TimeSliceSet:
                    Cores[(int)data[1] % Cores.Length].TimeSlice = (int)data[0];
                    break;
                case CodeTable.CoreInterrupt_TimeSliceGet:
                    return Cores[(int)data[0] % Cores.Length].TimeSlice;
                case CodeTable.CoreInterrupt_CoreCountGet:
                    return Cores.Length;
            }
            return null;
        }
        

        private void ERROR(int coreid, params string[] vs)
        {
            if (-1 == coreid)
            {
                Print("虚拟机错误" + vs[0] + " 即将结束虚拟机");
                ShutDown();
            }
            else if (EndCore(coreid, out int process_id))
                Print("进程：" + process_id + vs[0] + " 已结束");
        }
        private MemoryReadPackage DO_PAGE_MISS(int coreid, long address)
        {
            MemoryLock();
            MemoryBlock block = Memory.ReadMemory(address, out long realize);
            MemoryUnLock();
            MemoryReadPackage memory = null;
            if (-1 != realize)
            {
                memory = new MemoryReadPackage
                {
                    Block = block,
                    RealizeAddress = realize
                };
            }
            else if (-1 == coreid)
            {
                Print("虚拟机错误 数据异常 即将结束虚拟机");
                ShutDown();
            }
            else if(EndCore(coreid,out int process_id))
                Print("进程：" + process_id + " 进程数据异常 即将结束进程");
            return memory;
        }
        private ApplyMemoryPackage DO_MEM_APPLY(long size)
        {
            MemoryLock();
            long address = Memory.ApplyMemory(size, out long realize);
            MemoryUnLock();
            return new ApplyMemoryPackage
            {
                Address = address,
                RealizeSize = realize
            };
        }
        private void DO_MEM_FREE(long address)
        {
            MemoryLock();
            Memory.ReleaseMemory(address);
            MemoryUnLock();
        }
        private void CO_PE_SWITCH(int coreid)
        {
            if (-1 != coreid)
            {
                Cores[coreid].Stop();
                RingLock();
                Process process = Ring.GetProcess();
                if (null != process)
                    Cores[coreid].Start(process);
                RingUnLock();
            }
        }
        private void CO_PE_END(int coreid)
        {
            if (-1 == coreid)
                ShutDown();
            else if (EndCore(coreid, out _))
                StartCore(coreid);
        }
        private void CE_PE_CALLBACK(int program_id, long address)
        {
            if (-1 != program_id)
            {
                FileLock();
                FreeProgramID(program_id);
                FileUnLock();
                if (-1 != address)
                    DO_MEM_FREE(address);
            }
        }
        private void CO_PE_NEW(string path)
        {
            FileLoadLock();
            ProgramUnit program = FileLoad.Complete(path);
            if (AddNewProgramFlag)
            {
                program.MemorySpace = DO_MEM_APPLY(program.MemorySize).Address;
                if (-1 != program.MemorySpace)
                {
                    int id = GetProgramID();
                    if (-1 != id)
                        Ring.NewProcess(new Process(id, program, StackSize)
                        {
                            Running = false,
                            Time = new TimeSpan(0, 0, 0, 0, 0)
                        });
                    else
                        DO_MEM_FREE(program.MemorySpace);
                }
                else
                    Print("虚拟机错误 内存溢出 停止创建进程");
            }
            else
                Print("虚拟机错误 数据异常 停止创建进程");
            FileLoadUnLock();
        }
        private ProcessAddPackage CO_TE_NEW(string path)
        {
            FileLoadLock();
            ProcessAddPackage process = null;
            int id = GetProgramID();
            if (-1 != id)
            {
                ProgramUnit program = FileLoad.Complete(path);
                if (AddNewProgramFlag)
                {
                    program.MemorySpace = DO_MEM_APPLY(program.MemorySize).Address;
                    if (-1 != program.MemorySpace)
                        if (null != program)
                            process = new ProcessAddPackage
                            {
                                Program_ID = id,
                                Program = program
                            };
                        else
                            FreeProgramID(id);
                    else
                    {
                        Print("虚拟机错误 内存溢出 停止载入文件");
                        FreeProgramID(id);
                    }
                }
                else
                {
                    Print("虚拟机错误 内存溢出 停止载入文件");
                    FreeProgramID(id);
                }
            }
            FileLoadUnLock();
            return process;
        } 
        private byte[] CO_KEY_IN(long size)
        {
            AppLock();
            byte[] vs = Application.Scan(size);
            AppUnLock();
            return vs;
        }
        private int DO_FILE_OPEN(string path, Write_Type type)
        {
            FileLock();
            int id = GetFileID();
            FileMode mode = FileMode.Append;
            switch (type)
            {
                case Write_Type.Append:
                    mode = FileMode.Open;
                    break;
                case Write_Type.Cover:
                    mode = FileMode.Create;
                    break;
                case Write_Type.Create:
                    mode = FileMode.OpenOrCreate;
                    break;
            }
            if (-1 != id)
            {
                try
                {
                    FileStream fs = new FileStream(path, mode)
                    {
                        Position = 0
                    };
                    FileTable.Add(id, fs);
                }
                catch (Exception)
                {
                    FreeFileID(id);
                    id = -1;
                }
            }
            FileUnLock();
            return id;
        }
        private void DO_FILE_NEW(string path, File_Type type)
        {
            FileLock();
            switch (type)
            {
                case File_Type.Entry:
                    if (!Directory.Exists(path))
                        Directory.CreateDirectory(path);
                    break;
                case File_Type.File:
                    try
                    {
                        FileStream fs = File.Create(path);
                        fs.Close();
                    }
                    catch (Exception)
                    {

                    }
                    break;
            }
            FileUnLock();
        }


        private void ShutDown()
        {
            AppLock();
            Application.ShutDown();
            AppUnLock();
            for (int i = 0; i < Cores.Length; i++)
                Cores[i].Stop();
        }
        private bool EndCore(int coreid, out int process_id)
        {
            process_id = Cores[coreid].ProcessID;
            if (-1 != process_id)
            {
                Cores[coreid].Stop();
                RingLock();
                EndProcess(process_id);
                RingUnLock();
                Cores[coreid].Start(Ring.GetProcess());
                return true;
            }
            return false;
        }
        private void StartCore(int coreid)
        {
            RingLock();
            Cores[coreid].Start(Ring.GetProcess());
            RingUnLock();
        }
        private void Print(string str)
        {
            AppLock();
            Application.Print(str);
            AppUnLock();
        }
    }

    /// <summary>
    /// 内存申请包
    /// </summary>
    public class ApplyMemoryPackage
    {
        /// <summary>
        /// 申请的实际地址
        /// </summary>
        public long Address;
        /// <summary>
        /// 申请的实际大小
        /// </summary>
        public long RealizeSize;
    }
}
