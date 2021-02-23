using System;
using System.Collections.Generic;
using System.Text;
using SkyVM.InterfaceModule;

namespace SkyVM.CoreModule
{
    /// <summary>
    /// 核心模块
    /// </summary>
    public class CoreModule : IALUnit
    {
        private readonly MathModule Math;
        private readonly LogicModule Logic;

        /// <summary>
        /// 核心模块构造函数
        /// </summary>
        /// <param name="complex">复杂运算模块</param>
        public CoreModule(IComplexOperation complex) : base(complex)
        {
            Math = new MathModule();
            Logic = new LogicModule();
        }

        /// <summary>
        /// 运算方法
        /// </summary>
        /// <param name="overflow">溢出标志位</param>
        /// <param name="operation">运算符</param>
        /// <param name="parameters">参数表</param>
        /// <returns>返回运算结果</returns>
        public override byte[] Calculate(out bool overflow, string operation, CalculateParameter[] parameters)
        {
            byte[] result = null;
            Symbol_Flag flag = Symbol_Flag.Zero;
            overflow = false;
            if (null != parameters)
            {
                switch (operation.ToUpper())
                {
                    case "ADD":
                        if (2 <= parameters.Length)
                            result = Math.Add(parameters[0].Data, parameters[1].Data, out flag, out overflow);
                        break;
                    case "SUB":
                        if (2 <= parameters.Length)
                            result = Math.Sub(parameters[0].Data, parameters[1].Data, out flag, out overflow);
                        break;
                    case "MUL":
                        if (2 <= parameters.Length)
                            result = Math.Mul(parameters[0].Data, parameters[1].Data, out flag, out overflow);
                        break;
                    case "DIV":
                        if (2 <= parameters.Length)
                            result = Math.Div(parameters[0].Data, parameters[1].Data, out flag, out overflow);
                        break;
                    case "MOD":
                        if (2 <= parameters.Length)
                            result = Math.Mod(parameters[0].Data, parameters[1].Data, out flag, out overflow);
                        break;
                    case "AND":
                        if (2 <= parameters.Length)
                            result = Logic.And(parameters[0].Data, parameters[1].Data, out flag);
                        break;
                    case "OR":
                        if (2 <= parameters.Length)
                            result = Logic.Or(parameters[0].Data, parameters[1].Data, out flag);
                        break;
                    case "NOT":
                        if (1 <= parameters.Length)
                            result = Logic.Not(parameters[0].Data, out flag);
                        break;
                    case "XOR":
                        if (2 <= parameters.Length)
                            result = Logic.Xor(parameters[0].Data, parameters[1].Data, out flag);
                        break;
                    default:
                        if (Complex.OperateTest(operation))
                            result = Complex.ComplexCalculate(out flag, out overflow, operation, parameters);
                        break;
                }
            }
            Symbol = flag;
            return result;
        }
        /// <summary>
        /// 运算符检测
        /// </summary>
        /// <param name="operation">运算名</param>
        /// <returns>返回检测结果</returns>
        public override bool OperateTest(string operation)
        {
            bool result;
            switch (operation.ToUpper())
            {
                case "ADD":
                case "加":
                case "SUB":
                case "减":
                case "MUL":
                case "乘":
                case "DIV":
                case "除":
                case "AND":
                case "与":
                case "OR":
                case "或":
                case "NOT":
                case "非":
                    result = true;
                    break;
                default:
                    result = Complex.OperateTest(operation);
                    break;
            }
            return result;
        }
    }
}
