using System;
using System.Collections.Generic;
using System.Text;

namespace SkyVM.AnaModule
{
    /// <summary>
    /// 文件载入程序上层需求接口
    /// </summary>
    public interface IToFileLoad
    {
        /// <summary>
        /// 数学运算符检测
        /// </summary>
        /// <param name="str">数学运算标签</param>
        /// <returns>返回检测结果</returns>
        bool CheckCalculateString(string str);
        /// <summary>
        /// 文件载入程序中断函数
        /// 异常：0x00-错误的语法
        /// 异常：0x01-错误的方法类型
        /// 异常：0x10-错误的字符
        /// 异常：0x20-错误的运算
        /// 异常：0x21-表达式中存在错误的符号
        /// 异常：0x22-表达式中存在错误的运算符
        /// 异常：0x30-文件读取异常
        /// 异常：0x31-系统异常
        /// </summary>
        /// <param name="exception_code">中断号</param>
        /// <param name="line">操作行数</param>
        /// <param name="message">消息</param>
        /// <param name="path">文件路径</param>
        void Interrupt(int exception_code, int line, string message, string path);
    }
}
