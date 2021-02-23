using System;
using System.Collections.Generic;
using System.Text;

namespace SkyVM.InterfaceModule
{
    /// <summary>
    /// 内存基类
    /// </summary>
    public abstract class MemoryBase : IMemoryControl, IMemory
    {
        /// <summary>
        /// 内存段引用方法（将需要操作的内存块段整体读取-用于上层缓存使用）
        /// </summary>
        /// <param name="mem_id">读取的内存块号</param>
        /// <param name="realize_id">实际的内存段起始号</param>
        /// <returns>返回内存段</returns>
        public abstract MemoryBlock ReadMemory(long mem_id, out long realize_id);
    }
    /// <summary>
    /// 内存控制接口
    /// </summary>
    public abstract class IMemoryControl
    {
        /// <summary>
        /// 获取内存容量大小
        /// </summary>
        public abstract long MemorySize { get; }
        /// <summary>
        /// 获取正在使用的内存块
        /// </summary>
        public abstract MemoryNode[] Using { get; }
        /// <summary>
        /// 获取空闲的内存块
        /// </summary>
        public abstract MemoryNode[] Freeing { get; }
        /// <summary>
        /// 内存空间申请
        /// </summary>
        /// <param name="apply_size">请求内存大小</param>
        /// <param name="realize_size">返回实际内存大小</param>
        /// <returns>返回内存块号</returns>
        public abstract long ApplyMemory(long apply_size, out long realize_size);
        /// <summary>
        /// 释放内存
        /// </summary>
        /// <param name="mem_id">内存块号</param>
        public abstract void ReleaseMemory(long mem_id);
    }
    /// <summary>
    /// 内存接口
    /// </summary>
    public interface IMemory
    {
        /// <summary>
        /// 内存段引用方法（将需要操作的内存块段整体读取-用于上层缓存使用）
        /// </summary>
        /// <param name="mem_id">读取的内存块号</param>
        /// <param name="realize_id">实际的内存段起始号</param>
        /// <returns>返回内存段</returns>
        MemoryBlock ReadMemory(long mem_id, out long realize_id);

    }
    /// <summary>
    /// 内存分配块
    /// </summary>
    public class MemoryBlock
    {
        /// <summary>
        /// 分配块长度
        /// </summary>
        public long LongLength => Length;
        /// <summary>
        /// 分配块长度
        /// </summary>
        public readonly int Length;
        private readonly byte[] Memory;

        /// <summary>
        /// 内存分配块构造函数
        /// </summary>
        /// <param name="size">内存大小</param>
        public MemoryBlock(int size)
        {
            Length = size;
            Memory = new byte[size];
            for (int i = 0; i < Memory.Length; i++)
                Memory[i] = 0x00;
        }

        /// <summary>
        /// 本地索引器
        /// </summary>
        /// <param name="index">索引值</param>
        /// <returns>返回索引项</returns>
        public byte this[int index]
        {
            get
            {
                return Memory[index];
            }
            set
            {
                Memory[index] = value;
            }
        }
        /// <summary>
        /// 本地索引器
        /// </summary>
        /// <param name="index">索引值</param>
        /// <returns>返回索引项</returns>
        public byte this[long index]
        {
            get
            {
                return Memory[index];
            }
            set
            {
                Memory[index] = value;
            }
        }
    }
    /// <summary>
    /// 内存状态传递包
    /// </summary>
    public class MemoryNode
    {
        /// <summary>
        /// 内存起始块号
        /// </summary>
        public long Mem_ID;
        /// <summary>
        /// 内存长度
        /// </summary>
        public long Mem_Size;
    }
}
