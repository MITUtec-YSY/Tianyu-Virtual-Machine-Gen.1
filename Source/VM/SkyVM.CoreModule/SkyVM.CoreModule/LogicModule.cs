using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using SkyVM.InterfaceModule;

namespace SkyVM.CoreModule
{
    class LogicModule
    {
        public byte[] And(byte[] param1, byte[] param2, out Symbol_Flag flag)
        {
            byte[] result = new byte[(param1.Length > param2.Length ? param1.Length : param2.Length)];
            BigInteger integer1 = 0, integer2 = 0;
            if (null != param1)
                for (int i = param1.Length - 1; i >= 0; i--)
                    integer1 = integer1 * 256 + param1[i];
            if (null != param2)
                for (int i = param2.Length - 1; i >= 0; i--)
                    integer2 = integer2 * 256 + param2[i];
            integer1 &= integer2;
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
            return result;
        }

        public byte[] Or(byte[] param1, byte[] param2, out Symbol_Flag flag)
        {
            byte[] result = new byte[(param1.Length > param2.Length ? param1.Length : param2.Length)];
            BigInteger integer1 = 0, integer2 = 0;
            if (null != param1)
                for (int i = param1.Length - 1; i >= 0; i--)
                    integer1 = integer1 * 256 + param1[i];
            if (null != param2)
                for (int i = param2.Length - 1; i >= 0; i--)
                    integer2 = integer2 * 256 + param2[i];
            integer1 |= integer2;
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
            return result;
        }

        public byte[] Xor(byte[] param1, byte[] param2, out Symbol_Flag flag)
        {
            byte[] result = new byte[(param1.Length > param2.Length ? param1.Length : param2.Length)];
            BigInteger integer1 = 0, integer2 = 0;
            if (null != param1)
                for (int i = param1.Length - 1; i >= 0; i--)
                    integer1 = integer1 * 256 + param1[i];
            if (null != param2)
                for (int i = param2.Length - 1; i >= 0; i--)
                    integer2 = integer2 * 256 + param2[i];
            integer1 ^= integer2;
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
            return result;
        }

        public byte[] Not(byte[] param, out Symbol_Flag flag)
        {
            byte[] result = new byte[param.Length];
            BigInteger integer1 = 0;
            if (null != param)
                for (int i = param.Length - 1; i >= 0; i--)
                    integer1 = integer1 * 256 + param[i];
            integer1 = ~integer1;
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
            return result;
        }
    }
}
