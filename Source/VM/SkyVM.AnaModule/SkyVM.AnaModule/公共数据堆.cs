using System;
using System.Collections.Generic;
using System.Text;

namespace SkyVM.AnaModule
{
    /// <summary>
    /// 公共数据堆栈
    /// </summary>
    public class PublicDataStack
    {
        internal int Count => Stack.Count;

        private readonly Stack<int> Stack;

        /// <summary>
        /// 公共数据堆栈构造函数
        /// </summary>
        public PublicDataStack()
        {
            Stack = new Stack<int>();
        }

        /// <summary>
        /// 数据入栈
        /// </summary>
        /// <param name="d1">内存块1</param>
        /// <param name="d2">内存块2</param>
        /// <param name="d3">内存块3</param>
        /// <param name="d4">内存块4</param>
        public void Push(byte d1, byte d2, byte d3, byte d4)
        {
            Stack.Push((((d4 * 256) + d3) * 256 + d2) * 256 + d1);
        }
        /// <summary>
        /// 数据出栈
        /// </summary>
        /// <param name="d1">内存块1</param>
        /// <param name="d2">内存块2</param>
        /// <param name="d3">内存块3</param>
        /// <param name="d4">内存块4</param>
        public void Pop(out byte d1, out byte d2, out byte d3, out byte d4)
        {
            CalculateByte(0 < Stack.Count ? Stack.Pop() : 0, out d1, out d2, out d3, out d4);
        }
        /// <summary>
        /// 查看数据栈顶
        /// </summary>
        /// <param name="d1">内存块1</param>
        /// <param name="d2">内存块2</param>
        /// <param name="d3">内存块3</param>
        /// <param name="d4">内存块4</param>
        public void Peek(out byte d1, out byte d2, out byte d3, out byte d4)
        {
            CalculateByte(0 < Stack.Count ? Stack.Peek() : 0, out d1, out d2, out d3, out d4);
        }

        private void CalculateByte(int temp, out byte d1, out byte d2, out byte d3, out byte d4)
        {
            d1 = (byte)(temp % 256);
            temp /= 256;
            d2 = (byte)(temp % 256);
            temp /= 256;
            d3 = (byte)(temp % 256);
            temp /= 256;
            d4 = (byte)(temp % 256);
        }
    }
}
