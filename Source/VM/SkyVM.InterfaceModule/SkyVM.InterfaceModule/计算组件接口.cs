using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace SkyVM.InterfaceModule
{
    /// <summary>
    /// 运算逻辑单元接口
    /// </summary>
    public abstract class IALUnit
    {
        /// <summary>
        /// 复杂运算接口
        /// </summary>
        protected IComplexOperation Complex;
        /// <summary>
        /// 运算结果符号
        /// </summary>
        public Symbol_Flag Symbol { get; protected set; }

        /// <summary>
        /// 运算逻辑单元构造函数
        /// </summary>
        /// <param name="complex">复杂运算单元接口</param>
        public IALUnit(IComplexOperation complex)
        {
            Complex = complex;
            Symbol = Symbol_Flag.Zero;
        }

        /// <summary>
        /// 运算方法
        /// </summary>
        /// <param name="overflow">溢出标志位</param>
        /// <param name="operation">运算符</param>
        /// <param name="parameters">参数表</param>
        /// <returns>返回运算结果</returns>
        public abstract byte[] Calculate(out bool overflow, string operation, CalculateParameter[] parameters);
        /// <summary>
        /// 运算符检测
        /// </summary>
        /// <param name="operation">运算名</param>
        /// <returns>返回检测结果</returns>
        public abstract bool OperateTest(string operation);
    }

    /// <summary>
    /// 复杂运算模块接口
    /// </summary>
    public abstract class IComplexOperation : IComplexOperation_Base
    {
        private int ThreadLock;

        /// <summary>
        /// 复杂运算模块接口构造函数
        /// </summary>
        public IComplexOperation()
        {
            ThreadLock = 0;
        }

        /// <summary>
        /// 复杂运算通用函数接口
        /// </summary>
        /// <param name="flag">运算结果标志位</param>
        /// <param name="overflow">运算溢出标志位</param>
        /// <param name="operation">运算类型</param>
        /// <param name="parameters">传入参数列表</param>
        /// <returns>返回运算结果</returns>
        public sealed override byte[] ComplexCalculate(out Symbol_Flag flag, out bool overflow, string operation, CalculateParameter[] parameters)
        {
            Lock();
            byte[] vs = Calculate(out flag, out overflow, operation, parameters);
            Unlock();
            return vs;
        }
        /// <summary>
        /// 操作检测
        /// </summary>
        /// <param name="operation">运算名</param>
        /// <returns>返回检测结果</returns>
        public abstract bool OperateTest(string operation);
        /// <summary>
        /// 复杂运算函数
        /// </summary>
        /// <param name="flag">运算结果标志位</param>
        /// <param name="overflow">运算溢出标志位</param>
        /// <param name="operation">运算类型</param>
        /// <param name="parameters">传入参数列表</param>
        /// <returns>返回运算结果</returns>
        protected abstract byte[] Calculate(out Symbol_Flag flag, out bool overflow, string operation, CalculateParameter[] parameters);

        private void Lock()
        {
            while ((Interlocked.Exchange(ref ThreadLock, 1) != 0))
                Thread.Sleep(10);
        }
        private void Unlock()
        {
            Interlocked.Exchange(ref ThreadLock, 0);
        }
    }
    /// <summary>
    /// 复杂运算模块接口底层包
    /// </summary>
    public abstract class IComplexOperation_Base
    {
        /// <summary>
        /// 复杂运算通用函数接口
        /// </summary>
        /// <param name="flag">运算结果标志位</param>
        /// <param name="overflow">运算溢出标志位</param>
        /// <param name="operation">运算类型</param>
        /// <param name="parameters">传入参数列表</param>
        /// <returns>返回运算结果</returns>
        public abstract byte[] ComplexCalculate(out Symbol_Flag flag, out bool overflow, string operation, CalculateParameter[] parameters);
    }

    /// <summary>
    /// 运算参数类
    /// </summary>
    public class CalculateParameter
    {
        /// <summary>
        /// 运算数字节数组
        /// </summary>
        public byte[] Data;
    }

    /// <summary>
    /// 运算器符号位标识
    /// </summary>
    public enum Symbol_Flag
    {
        /// <summary>
        /// 正数标志
        /// </summary>
        Positive,
        /// <summary>
        /// 负数标志
        /// </summary>
        Negative,
        /// <summary>
        /// 零标识
        /// </summary>
        Zero,
    }
}
