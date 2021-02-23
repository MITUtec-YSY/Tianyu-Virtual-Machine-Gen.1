using System;
using System.Collections.Generic;
using System.Text;

namespace SkyVM.ExModule
{
    /// <summary>
    /// 中断号包
    /// </summary>
    public class InterruptCode
    {
        /// <summary>
        /// 中断号
        /// </summary>
        public int IntCode { get; private set; }
        /// <summary>
        /// 中断号包构造函数
        /// </summary>
        /// <param name="code"></param>
        public InterruptCode(int code)
        {
            IntCode = code;
        }

        /// <summary>
        /// 中断号类型到int型的隐式转换
        /// </summary>
        /// <param name="interrupt">中断号类型</param>
        public static implicit operator int(InterruptCode interrupt)
        {
            return interrupt.IntCode;
        }
        /// <summary>
        /// 中断号类型到long型的隐式转换
        /// </summary>
        /// <param name="interrupt">中断号类型</param>
        public static implicit operator long(InterruptCode interrupt)
        {
            return interrupt.IntCode;
        }

        /// <summary>
        /// 数学异常中断-除零异常
        /// </summary>
        public static InterruptCode MathException_DivZero = new InterruptCode(CodeTable.MathException_DivZero);
        /// <summary>
        /// 数学异常中断-计算溢出
        /// </summary>
        public static InterruptCode MathException_OverFlow = new InterruptCode(CodeTable.MathException_OverFlow);

        /// <summary>
        /// 数据异常中断-数组越界
        /// </summary>
        public static InterruptCode DataException_ArraysOutOfBounds = new InterruptCode(CodeTable.DataException_ArraysOutOfBounds);
        /// <summary>
        /// 数据异常中断-空地址
        /// </summary>
        public static InterruptCode DataException_NullPoint = new InterruptCode(CodeTable.DataException_NullPoint);
        /// <summary>
        /// 数据操作中断-缺页中断
        /// </summary>
        public static InterruptCode DataOperation_PageMissing = new InterruptCode(CodeTable.DataOperation_PageMissing);
        /// <summary>
        /// 数据操作中断-内存申请中断
        /// </summary>
        public static InterruptCode DataOperation_MemoryApply = new InterruptCode(CodeTable.DataOperation_MemoryApply);
        /// <summary>
        /// 数据操作中断-内存释放中断
        /// </summary>
        public static InterruptCode DataOperation_MemoryRelease = new InterruptCode(CodeTable.DataOperation_MemoryRelease);
        /// <summary>
        /// 数据操作中断-获取时间中断
        /// </summary>
        public static InterruptCode DataOperation_TimeGet = new InterruptCode(CodeTable.DataOperation_TimeGet);
        /// <summary>
        /// 数据操作中断-设置时间中断
        /// </summary>
        public static InterruptCode DataOperation_TimeSet = new InterruptCode(CodeTable.DataOperation_TimeSet);
        /// <summary>
        /// 数据操作中断-时间同步中断
        /// </summary>
        public static InterruptCode DataOperation_TimeSynchronize = new InterruptCode(CodeTable.DataOperation_TimeSynchronize);
        /// <summary>
        /// 数据异常中断-堆栈溢出
        /// </summary>
        public static InterruptCode DataException_StackOverFlow = new InterruptCode(CodeTable.DataException_StackOverFlow);
        /// <summary>
        /// 数据异常中断-堆栈越界
        /// </summary>
        public static InterruptCode DataException_StackOutOfBounds = new InterruptCode(CodeTable.DataException_StackOutOfBounds);
        /// <summary>
        /// 数据异常中断-内存溢出
        /// </summary>
        public static InterruptCode DataException_MemoryOverFlow = new InterruptCode(CodeTable.DataException_MemoryOverFlow);

        /// <summary>
        /// 行为操作中断-键盘输入中断
        /// </summary>
        public static InterruptCode BehaciourOperation_KeyBoardInput = new InterruptCode(CodeTable.BehaciourOperation_KeyBoardInput);
        /// <summary>
        /// 行为操作中断-屏幕显示中断
        /// </summary>
        public static InterruptCode BehaciourOperation_ScreenPrint = new InterruptCode(CodeTable.BehaciourOperation_ScreenPrint);
        /// <summary>
        /// 行为异常中断-空数据源
        /// </summary>
        public static InterruptCode BehaciourException_DataSourceEmpty = new InterruptCode(CodeTable.BehaciourException_DataSourceEmpty);

        /// <summary>
        /// 网络操作中断-网络连接创建中断
        /// </summary>
        public static InterruptCode NetOperation_NetConnetionCreate = new InterruptCode(CodeTable.NetOperation_NetConnetionCreate);
        /// <summary>
        /// 网络操作中断-网络数据发送中断
        /// </summary>
        public static InterruptCode NetOperation_NetDataSend = new InterruptCode(CodeTable.NetOperation_NetDataSend);
        /// <summary>
        /// 网络操作中断-网络数据接收中断
        /// </summary>
        public static InterruptCode NetOperation_NetDataReceive = new InterruptCode(CodeTable.NetOperation_NetDataReceive);
        /// <summary>
        /// 网络操作中断-网络端口获取中断
        /// </summary>
        public static InterruptCode NetOperation_NetConnetionPortGet = new InterruptCode(CodeTable.NetOperation_NetConnetionPortGet);
        /// <summary>
        /// 网络操作中断-网络连接暂停中断
        /// </summary>
        public static InterruptCode NetOperation_NetConnetionPause = new InterruptCode(CodeTable.NetOperation_NetConnetionPause);
        /// <summary>
        /// 网络操作中断-网络连接复位中断
        /// </summary>
        public static InterruptCode NetOperation_NetConnetionReset = new InterruptCode(CodeTable.NetOperation_NetConnetionReset);
        /// <summary>
        /// 网络操作中断-网络连接关闭中断
        /// </summary>
        public static InterruptCode NetOperation_NetConnetionClose = new InterruptCode(CodeTable.NetOperation_NetConnetionClose);

        /// <summary>
        /// 设备操作中断-设备激活中断
        /// </summary>
        public static InterruptCode DeviceOperation_DeviceActive = new InterruptCode(CodeTable.DeviceOperation_DeviceActive);
        /// <summary>
        /// 设备操作中断-设备暂停中断
        /// </summary>
        public static InterruptCode DeviceOperation_DevicePause = new InterruptCode(CodeTable.DeviceOperation_DevicePause);
        /// <summary>
        /// 设备操作中断-设备复位中断
        /// </summary>
        public static InterruptCode DeviceOperation_DeviceReset = new InterruptCode(CodeTable.DeviceOperation_DeviceReset);
        /// <summary>
        /// 设备操作中断-设备关闭中断
        /// </summary>
        public static InterruptCode DeviceOperation_DeviceClose = new InterruptCode(CodeTable.DeviceOperation_DeviceClose);
        /// <summary>
        /// 设备操作中断-设备读中断
        /// </summary>
        public static InterruptCode DeviceOperation_DeviceRead = new InterruptCode(CodeTable.DeviceOperation_DeviceRead);
        /// <summary>
        /// 设备操作中断-设备写中断
        /// </summary>
        public static InterruptCode DeviceOperation_DeviceWrite = new InterruptCode(CodeTable.DeviceOperation_DeviceWrite);

        /// <summary>
        /// 文件操作中断-打开文件中断
        /// </summary>
        public static InterruptCode FileOperation_FileOpen = new InterruptCode(CodeTable.FileOperation_FileOpen);
        /// <summary>
        /// 文件操作中断-关闭文件中断
        /// </summary>
        public static InterruptCode FileOperation_FileClose = new InterruptCode(CodeTable.FileOperation_FileClose);
        /// <summary>
        /// 文件操作中断-文件读中断
        /// </summary>
        public static InterruptCode FileOperation_FileRead = new InterruptCode(CodeTable.FileOperation_FileRead);
        /// <summary>
        /// 文件操作中断-文件写中断
        /// </summary>
        public static InterruptCode FileOperation_FileWrite = new InterruptCode(CodeTable.FileOperation_FileWrite);
        /// <summary>
        /// 文件操作中断-文件拷贝中断
        /// </summary>
        public static InterruptCode FileOperation_FileCopy = new InterruptCode(CodeTable.FileOperation_FileCopy);
        /// <summary>
        /// 文件操作中断-文件指针操作中断
        /// </summary>
        public static InterruptCode FileOperation_FilePositionMove = new InterruptCode(CodeTable.FileOperation_FilePositionMove);
        /// <summary>
        /// 文件操作中断-文件创建中断
        /// </summary>
        public static InterruptCode FileOperation_FileCreate = new InterruptCode(CodeTable.FileOperation_FileCreate);
        /// <summary>
        /// 文件操作中断-文件删除中断
        /// </summary>
        public static InterruptCode FileOperation_FileDelete = new InterruptCode(CodeTable.FileOperation_FileDelete);

        /// <summary>
        /// 核心中断-进程切换中断
        /// </summary>
        public static InterruptCode CoreInterrupt_ProcessSwitch = new InterruptCode(CodeTable.CoreInterrupt_ProcessSwitch);
        /// <summary>
        /// 核心中断-进程结束中断
        /// </summary>
        public static InterruptCode CoreInterrupt_ProcessEnd = new InterruptCode(CodeTable.CoreInterrupt_ProcessEnd);
        /// <summary>
        /// 核心中断-线程结束中断
        /// </summary>
        public static InterruptCode CoreInterrupt_ThreadEnd = new InterruptCode(CodeTable.CoreInterrupt_ThreadEnd);
        /// <summary>
        /// 核心中断-进程载入中断
        /// </summary>
        public static InterruptCode CoreInterrupt_ProcessLoad = new InterruptCode(CodeTable.CoreInterrupt_ProcessLoad);
        /// <summary>
        /// 核心中断-线程载入中断
        /// </summary>
        public static InterruptCode CoreInterrupt_ThreadLoad = new InterruptCode(CodeTable.CoreInterrupt_ThreadLoad);

        /// <summary>
        /// 核心中断-设置时间片
        /// </summary>
        public static InterruptCode CoreInterrupt_TimeSliceSet = new InterruptCode(CodeTable.CoreInterrupt_TimeSliceSet);
        /// <summary>
        /// 核心中断-获取时间片
        /// </summary>
        public static InterruptCode CoreInterrupt_TimeSliceGet = new InterruptCode(CodeTable.CoreInterrupt_TimeSliceGet);
        /// <summary>
        /// 核心中断-获取核心数
        /// </summary>
        public static InterruptCode CoreInterrupt_CoreCountGet = new InterruptCode(CodeTable.CoreInterrupt_CoreCountGet);
    }

    /// <summary>
    /// 中断号表
    /// </summary>
    public class CodeTable
    {
        /// <summary>
        /// 数学异常中断-除零异常
        /// </summary>
        public const int MathException_DivZero = 0x00;
        /// <summary>
        /// 数学异常中断-计算溢出
        /// </summary>
        public const int MathException_OverFlow = 0x01;

        /// <summary>
        /// 数据异常中断-数组越界
        /// </summary>
        public const int DataException_ArraysOutOfBounds = 0x10;
        /// <summary>
        /// 数据异常中断-空地址
        /// </summary>
        public const int DataException_NullPoint = 0x11;
        /// <summary>
        /// 数据操作中断-缺页中断
        /// </summary>
        public const int DataOperation_PageMissing = 0x12;
        /// <summary>
        /// 数据操作中断-内存申请中断
        /// </summary>
        public const int DataOperation_MemoryApply = 0x13;
        /// <summary>
        /// 数据操作中断-内存释放中断
        /// </summary>
        public const int DataOperation_MemoryRelease = 0x14;
        /// <summary>
        /// 数据操作中断-获取时间中断
        /// </summary>
        public const int DataOperation_TimeGet = 0x15;
        /// <summary>
        /// 数据操作中断-设置时间中断
        /// </summary>
        public const int DataOperation_TimeSet = 0x16;
        /// <summary>
        /// 数据操作中断-时间同步中断
        /// </summary>
        public const int DataOperation_TimeSynchronize = 0x17;
        /// <summary>
        /// 数据异常中断-堆栈溢出
        /// </summary>
        public const int DataException_StackOverFlow = 0x18;
        /// <summary>
        /// 数据异常中断-堆栈越界
        /// </summary>
        public const int DataException_StackOutOfBounds = 0x19;
        /// <summary>
        /// 数据异常中断-内存溢出
        /// </summary>
        public const int DataException_MemoryOverFlow = 0x1A;

        /// <summary>
        /// 行为操作中断-键盘输入中断
        /// </summary>
        public const int BehaciourOperation_KeyBoardInput = 0x30;
        /// <summary>
        /// 行为操作中断-屏幕显示中断
        /// </summary>
        public const int BehaciourOperation_ScreenPrint = 0x31;
        /// <summary>
        /// 行为异常中断-空数据源
        /// </summary>
        public const int BehaciourException_DataSourceEmpty = 0x33;

        /// <summary>
        /// 网络操作中断-网络连接创建中断
        /// </summary>
        public const int NetOperation_NetConnetionCreate = 0x40;
        /// <summary>
        /// 网络操作中断-网络数据发送中断
        /// </summary>
        public const int NetOperation_NetDataSend = 0x41;
        /// <summary>
        /// 网络操作中断-网络数据接收中断
        /// </summary>
        public const int NetOperation_NetDataReceive = 0x42;
        /// <summary>
        /// 网络操作中断-网络端口获取中断
        /// </summary>
        public const int NetOperation_NetConnetionPortGet = 0x43;
        /// <summary>
        /// 网络操作中断-网络连接暂停中断
        /// </summary>
        public const int NetOperation_NetConnetionPause = 0x45;
        /// <summary>
        /// 网络操作中断-网络连接复位中断
        /// </summary>
        public const int NetOperation_NetConnetionReset = 0x46;
        /// <summary>
        /// 网络操作中断-网络连接关闭中断
        /// </summary>
        public const int NetOperation_NetConnetionClose = 0x47;

        /// <summary>
        /// 设备操作中断-设备激活中断
        /// </summary>
        public const int DeviceOperation_DeviceActive = 0x50;
        /// <summary>
        /// 设备操作中断-设备暂停中断
        /// </summary>
        public const int DeviceOperation_DevicePause = 0x51;
        /// <summary>
        /// 设备操作中断-设备复位中断
        /// </summary>
        public const int DeviceOperation_DeviceReset = 0x52;
        /// <summary>
        /// 设备操作中断-设备关闭中断
        /// </summary>
        public const int DeviceOperation_DeviceClose = 0x53;
        /// <summary>
        /// 设备操作中断-设备读中断
        /// </summary>
        public const int DeviceOperation_DeviceRead = 0x54;
        /// <summary>
        /// 设备操作中断-设备写中断
        /// </summary>
        public const int DeviceOperation_DeviceWrite = 0x55;

        /// <summary>
        /// 文件操作中断-打开文件中断
        /// </summary>
        public const int FileOperation_FileOpen = 0x60;
        /// <summary>
        /// 文件操作中断-关闭文件中断
        /// </summary>
        public const int FileOperation_FileClose = 0x61;
        /// <summary>
        /// 文件操作中断-文件读中断
        /// </summary>
        public const int FileOperation_FileRead = 0x62;
        /// <summary>
        /// 文件操作中断-文件写中断
        /// </summary>
        public const int FileOperation_FileWrite = 0x63;
        /// <summary>
        /// 文件操作中断-文件拷贝中断
        /// </summary>
        public const int FileOperation_FileCopy = 0x64;
        /// <summary>
        /// 文件操作中断-文件指针操作中断
        /// </summary>
        public const int FileOperation_FilePositionMove = 0x65;
        /// <summary>
        /// 文件操作中断-文件创建中断
        /// </summary>
        public const int FileOperation_FileCreate = 0x67;
        /// <summary>
        /// 文件操作中断-文件删除中断
        /// </summary>
        public const int FileOperation_FileDelete = 0x68;

        /// <summary>
        /// 核心中断-进程切换中断
        /// </summary>
        public const int CoreInterrupt_ProcessSwitch = 0x20;
        /// <summary>
        /// 核心中断-进程结束中断
        /// </summary>
        public const int CoreInterrupt_ProcessEnd = 0x21;
        /// <summary>
        /// 核心中断-线程结束中断
        /// </summary>
        public const int CoreInterrupt_ThreadEnd = 0x22;
        /// <summary>
        /// 核心中断-进程载入中断
        /// </summary>
        public const int CoreInterrupt_ProcessLoad = 0x23;
        /// <summary>
        /// 核心中断-线程载入中断
        /// </summary>
        public const int CoreInterrupt_ThreadLoad = 0x24;

        /// <summary>
        /// 核心中断-设置时间片
        /// </summary>
        public const int CoreInterrupt_TimeSliceSet = 0xE0;
        /// <summary>
        /// 核心中断-获取时间片
        /// </summary>
        public const int CoreInterrupt_TimeSliceGet = 0xE1;
        /// <summary>
        /// 核心中断-获取核心数
        /// </summary>
        public const int CoreInterrupt_CoreCountGet = 0xE2;
    }
}
