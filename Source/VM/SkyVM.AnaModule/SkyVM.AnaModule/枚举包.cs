using System;
using System.Collections.Generic;
using System.Text;

namespace SkyVM.AnaModule
{
    /// <summary>
    /// 操作指令类型包
    /// </summary>
    public enum Instructuions_Package
    {
        /// <summary>
        /// 程序休眠
        /// </summary>
        Sleep,
        /// <summary>
        /// 空程序指令
        /// </summary>
        NOP,
        /// <summary>
        /// 本地结束指令
        /// </summary>
        END,
        /// <summary>
        /// 过程调用返回
        /// </summary>
        Ret,
        /// <summary>
        /// 无条件跳转语句
        /// </summary>
        Jmp,
        /// <summary>
        /// 运算标识为正时跳转
        /// </summary>
        JNS,
        /// <summary>
        /// 运算标识为负时跳转
        /// </summary>
        JS,
        /// <summary>
        /// 运算标识为零时跳转
        /// </summary>
        JZ,
        /// <summary>
        /// 数据入栈
        /// </summary>
        PUSH,
        /// <summary>
        /// 数据出栈
        /// </summary>
        POP,
        /// <summary>
        /// 运算指令组（运算指令无需解析，只需读取后交由运算接口进行处理）
        /// </summary>
        Calculate,
        /// <summary>
        /// 随机数生成
        /// </summary>
        Rand,
        /// <summary>
        /// 键盘读取
        /// </summary>
        Scan,
        /// <summary>
        /// 输出字符串
        /// </summary>
        Print,
        /// <summary>
        /// 输出字符
        /// </summary>
        Putc,
        /// <summary>
        /// 创建一个网络连接
        /// </summary>
        Connect,
        /// <summary>
        /// 通过网络发送数据
        /// </summary>
        Send,
        /// <summary>
        /// 从网络接收数据
        /// </summary>
        Receive,
        /// <summary>
        /// 获取一个网络的端口
        /// </summary>
        GetPort,
        /// <summary>
        /// 创建一个文件
        /// </summary>
        Create,
        /// <summary>
        /// 删除一个文件
        /// </summary>
        Delete,
        /// <summary>
        /// 修改文件操作指针
        /// </summary>
        Position,
        /// <summary>
        /// 设置内存大小
        /// </summary>
        Set_Memory,
        /// <summary>
        /// 申请新内存空间
        /// </summary>
        New_Memory,
        /// <summary>
        /// 释放一个内存空间
        /// </summary>
        Free_Memory,
        /// <summary>
        /// 初始化内存（赋值操作）
        /// </summary>
        Init_Memory,
        /// <summary>
        /// 获取系统时间
        /// </summary>
        Read_Time,
        /// <summary>
        /// 修改系统时间
        /// </summary>
        Write_Time,
        /// <summary>
        /// 同步系统时间
        /// </summary>
        Sync_Time,
        /// <summary>
        /// 激活设备
        /// </summary>
        Act_Device,
        /// <summary>
        /// 暂停设备
        /// </summary>
        Pause_Device,
        /// <summary>
        /// 重启设备
        /// </summary>
        Reset_Device,
        /// <summary>
        /// 关闭设备
        /// </summary>
        Close_Device,
        /// <summary>
        /// 从设备读取数据
        /// </summary>
        Read_Device,
        /// <summary>
        /// 向设备写入数据
        /// </summary>
        Write_Device,
        /// <summary>
        /// 暂停一个网络连接
        /// </summary>
        Pause_Net,
        /// <summary>
        /// 恢复一个网络连接
        /// </summary>
        Reset_Net,
        /// <summary>
        /// 关闭一个网络连接
        /// </summary>
        Close_Net,
        /// <summary>
        /// 打开文件
        /// </summary>
        Open_File,
        /// <summary>
        /// 关闭文件
        /// </summary>
        Close_File,
        /// <summary>
        /// 读取文件
        /// </summary>
        Read_File,
        /// <summary>
        /// 写入文件
        /// </summary>
        Write_File,
        /// <summary>
        /// 拷贝文件
        /// </summary>
        Copy_File,
        /// <summary>
        /// 新建一个进程
        /// </summary>
        Init_Process,
        /// <summary>
        /// 结束本进程
        /// </summary>
        End_Process,
        /// <summary>
        /// 进程切换
        /// </summary>
        Switch_Process,
        /// <summary>
        /// 加载一个外部文件
        /// </summary>
        Init_Thread,
        /// <summary>
        /// 结束加载文件
        /// </summary>
        End_Thread,
        /// <summary>
        /// 外部调用
        /// </summary>
        Outside_Call,
        /// <summary>
        /// 本地调用
        /// </summary>
        Local_Call,
        /// <summary>
        /// 读取基础缓冲区
        /// </summary>
        Read_BasePool,
        /// <summary>
        /// 写入基础缓冲区
        /// </summary>
        Write_BasePool,
        /// <summary>
        /// 读取专用缓冲区
        /// </summary>
        Read_SpecialPool,
        /// <summary>
        /// 写入专用缓冲区
        /// </summary>
        Write_SpecialPool,
        /// <summary>
        /// 设置时间片
        /// </summary>
        Set_TimeSlice,
        /// <summary>
        /// 获取时间片
        /// </summary>
        Get_TimeSlice,
        /// <summary>
        /// 获取核心数
        /// </summary>
        Get_Core,
    }

    /// <summary>
    /// 指令操作数类型
    /// </summary>
    public enum Parameter_Type
    {
        /// <summary>
        /// 表达式类型
        /// </summary>
        Expression,
        /// <summary>
        /// 立即数类型-大整数
        /// </summary>
        BigNumber,
        /// <summary>
        /// 立即数类型-长整数
        /// </summary>
        Long,
        /// <summary>
        /// 字符串类型
        /// </summary>
        STR,
    }
}
