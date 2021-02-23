using System;
using System.Collections.Generic;
using System.Text;
using System.Numerics;
using SkyVM.InterfaceModule;

namespace SkyVM.CoreModule
{
    /// <summary>
    /// 数学运算计算模块
    /// </summary>
    class MathModule
    {
        /// <summary>
        /// 加法运算
        /// </summary>
        /// <param name="param1">参数1</param>
        /// <param name="param2">参数2</param>
        /// <param name="flag">运算结果符号标志</param>
        /// <param name="overflow">运算溢出标志位</param>
        /// <returns>返回运算结果</returns>
        public byte[] Add(byte[] param1, byte[] param2, out Symbol_Flag flag, out bool overflow)
        {
            overflow = false;
            byte[] result = new byte[(param1.Length > param2.Length ? param1.Length : param2.Length)];
            BigInteger integer1 = 0, integer2 = 0;
            if (null != param1)
                for (int i = param1.Length - 1; i >= 0; i--)
                    integer1 = integer1 * 256 + param1[i];
            if (null != param2)
                for (int i = param2.Length - 1; i >= 0; i--)
                    integer2 = integer2 * 256 + param2[i];
            integer1 += integer2;
            if (0 == integer1)
            {
                flag = Symbol_Flag.Zero;
                for (int i = 0; i < result.Length; i++)
                    result[i] = 0x00;
            }
            else
            {
                if (0 < integer1)
                    flag = Symbol_Flag.Positive;
                else
                    flag = Symbol_Flag.Negative;
                for (int i = 0; i < result.Length && 0 != integer1; i++, integer1 /= 256)
                    result[i] = (byte)(int)(integer1 % 256);
            }
            if (0 != integer1)
                overflow = true;
            return result;
        }
        /// <summary>
        /// 减法运算
        /// </summary>
        /// <param name="param1">参数1</param>
        /// <param name="param2">参数2</param>
        /// <param name="flag">运算结果符号标志</param>
        /// <param name="overflow">运算溢出标志位</param>
        /// <returns>返回运算结果</returns>
        public byte[] Sub(byte[] param1, byte[] param2, out Symbol_Flag flag, out bool overflow)
        {
            overflow = false;
            byte[] result = new byte[(param1.Length > param2.Length ? param1.Length : param2.Length)];
            BigInteger integer1 = 0, integer2 = 0;
            if (null != param1)
                for (int i = param1.Length - 1; i >= 0; i--)
                    integer1 = integer1 * 256 + param1[i];
            if (null != param2)
                for (int i = param2.Length - 1; i >= 0; i--)
                    integer2 = integer2 * 256 + param2[i];
            integer1 -= integer2;
            if (0 == integer1)
            {
                flag = Symbol_Flag.Zero;
                for (int i = 0; i < result.Length; i++)
                    result[i] = 0x00;
            }
            else
            {
                if (0 < integer1)
                    flag = Symbol_Flag.Positive;
                else
                    flag = Symbol_Flag.Negative;
                for (int i = 0; i < result.Length && 0 != integer1; i++, integer1 /= 256)
                    result[i] = (byte)(int)(integer1 % 256);
            }
            if (0 != integer1)
                overflow = true;
            return result;
        }
        /// <summary>
        /// 乘法运算
        /// </summary>
        /// <param name="param1">参数1</param>
        /// <param name="param2">参数2</param>
        /// <param name="flag">运算结果符号标志</param>
        /// <param name="overflow">运算溢出标志位</param>
        /// <returns>返回运算结果</returns>
        public byte[] Mul(byte[] param1, byte[] param2, out Symbol_Flag flag, out bool overflow)
        {
            overflow = false;
            byte[] result = new byte[(param1.Length > param2.Length ? param1.Length : param2.Length)];
            BigInteger integer1 = 0, integer2 = 0;
            if (null != param1)
                for (int i = param1.Length - 1; i >= 0; i--)
                    integer1 = integer1 * 256 + param1[i];
            if (null != param2)
                for (int i = param2.Length - 1; i >= 0; i--)
                    integer2 = integer2 * 256 + param2[i];
            integer1 *= integer2;
            if (0 == integer1)
            {
                flag = Symbol_Flag.Zero;
                for (int i = 0; i < result.Length; i++)
                    result[i] = 0x00;
            }
            else
            {
                if (0 < integer1)
                    flag = Symbol_Flag.Positive;
                else
                    flag = Symbol_Flag.Negative;
                for (int i = 0; i < result.Length && 0 != integer1; i++, integer1 /= 256)
                    result[i] = (byte)(int)(integer1 % 256);
            }
            if (0 != integer1)
                overflow = true;
            return result;
        }
        /// <summary>
        /// 除法运算
        /// </summary>
        /// <param name="param1">参数1</param>
        /// <param name="param2">参数2</param>
        /// <param name="flag">运算结果符号标志</param>
        /// <param name="overflow">运算溢出标志位</param>
        /// <returns>返回运算结果</returns>
        public byte[] Div(byte[] param1, byte[] param2, out Symbol_Flag flag, out bool overflow)
        {
            overflow = false;
            byte[] result = new byte[(param1.Length > param2.Length ? param1.Length : param2.Length)];
            BigInteger integer1 = 0, integer2 = 0;
            if (null != param1)
                for (int i = param1.Length - 1; i >= 0; i--)
                    integer1 = integer1 * 256 + param1[i];
            if (null != param2)
                for (int i = param2.Length - 1; i >= 0; i--)
                    integer2 = integer2 * 256 + param2[i];
            integer1 /= integer2;
            if (0 == integer1)
            {
                flag = Symbol_Flag.Zero;
                for (int i = 0; i < result.Length; i++)
                    result[i] = 0x00;
            }
            else
            {
                if (0 < integer1)
                    flag = Symbol_Flag.Positive;
                else
                    flag = Symbol_Flag.Negative;
                for (int i = 0; i < result.Length && 0 != integer1; i++, integer1 /= 256)
                    result[i] = (byte)(int)(integer1 % 256);
            }
            if (0 != integer1)
                overflow = true;
            return result;
        }
        /// <summary>
        /// 求余运算
        /// </summary>
        /// <param name="param1">参数1</param>
        /// <param name="param2">参数2</param>
        /// <param name="flag">运算结果符号标志</param>
        /// <param name="overflow">运算溢出标志位</param>
        /// <returns>返回运算结果</returns>
        public byte[] Mod(byte[] param1, byte[] param2, out Symbol_Flag flag, out bool overflow)
        {
            overflow = false;
            byte[] result = new byte[(param1.Length > param2.Length ? param1.Length : param2.Length)];
            BigInteger integer1 = 0, integer2 = 0;
            if (null != param1)
                for (int i = param1.Length - 1; i >= 0; i--)
                    integer1 = integer1 * 256 + param1[i];
            if (null != param2)
                for (int i = param2.Length - 1; i >= 0; i--)
                    integer2 = integer2 * 256 + param2[i];
            integer1 %= integer2;
            if (0 == integer1)
            {
                flag = Symbol_Flag.Zero;
                for (int i = 0; i < result.Length; i++)
                    result[i] = 0x00;
            }
            else
            {
                if (0 < integer1)
                    flag = Symbol_Flag.Positive;
                else
                    flag = Symbol_Flag.Negative;
                for (int i = 0; i < result.Length && 0 != integer1; i++, integer1 /= 256)
                    result[i] = (byte)(int)(integer1 % 256);
            }
            if (0 != integer1)
                overflow = true;
            return result;
        }
    }
}
