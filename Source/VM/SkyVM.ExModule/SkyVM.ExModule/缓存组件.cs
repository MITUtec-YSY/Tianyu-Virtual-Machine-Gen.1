using SkyVM.InterfaceModule;
using System;
using System.Collections.Generic;
using System.Text;

namespace SkyVM.ExModule
{
    internal class L1_Cache : ICache
    {
        private readonly List<Node> CacheTable;
        private readonly int CacheSize;

        /// <summary>
        /// 运行内存模块构造函数
        /// </summary>
        /// <param name="cache_size">缓存区大小</param>
        /// <param name="interrupt">内存控制器中断接口</param>
        public L1_Cache(IMemoryCacheInterrupt interrupt, int cache_size = 3) : base(interrupt)
        {
            CacheTable = new List<Node>();
            CacheSize = cache_size;
        }

        public override void Bad(long segment, long page)
        {
            long address = segment + page;
            for (int i = 0; i < CacheTable.Count; i++)
                if (CacheTable[i].Start == address)
                {
                    CacheTable.RemoveAt(i);
                    break;
                }
        }

        /// <summary>
        /// 读取内存中的数据
        /// </summary>
        /// <param name="segment">段地址</param>
        /// <param name="page">页地址</param>
        /// <param name="size">数据长度</param>
        /// <returns>返回数据包</returns>
        public override byte[] Get(long segment, long page, int size)
        {
            bool find = false;
            long address = segment + page;
            byte[] vs = null;
            for (int i = 0; i < CacheTable.Count; i++)
            {
                if (CacheTable[i].Start <= address && CacheTable[i].End > address)
                {
                    find = true;
                    if (address + size <= CacheTable[i].End)
                    {
                        vs = new byte[size];
                        for (long j = 0, k = address - CacheTable[i].Start; j < size; j++, k++)
                            vs[j] = CacheTable[i].Data[k];
                        CacheTable[i].AgeCount++;
                    }
                    else
                        Interrput.Interrupt(MemoryCache_Type.Out_Bounds, address, size);
                }
            }
            if (!find)
            {
                LRU(address);
                vs = Get(segment, page, size);
            }
            return vs;
        }
        public override void Set(long segment, long page, byte[] data)
        {
            bool find = false;
            long address = segment + page;
            for (int i = 0; i < CacheTable.Count; i++)
            {
                if (CacheTable[i].Start <= address && CacheTable[i].End > address)
                {
                    find = true;
                    if (address + data.LongLength <= CacheTable[i].End)
                    {
                        for (long j = 0, k = address - CacheTable[i].Start; j < data.LongLength; j++, k++)
                            CacheTable[i].Data[k] = data[j];
                        CacheTable[i].AgeCount++;
                    }
                    else
                        Interrput.Interrupt(MemoryCache_Type.Out_Bounds, address, data.Length);
                }
            }
            if (!find)
            {
                LRU(address);
                Set(segment, page, data);
            }
        }
        
        /// <summary>
        /// 缓存替换（最近最少使用算法）
        /// </summary>
        /// <param name="address">替换地址块</param>
        private void LRU(long address)
        {
            MemoryBlock memory = Interrput.PageMissingInterrupt(address, out long realize);
            if (-1 != realize)
            {
                if (3 == CacheTable.Count)
                {
                    int index = 0;
                    for (int i = 1; i < CacheTable.Count; i++)
                        if (CacheTable[index].AgeCount <= CacheTable[i].AgeCount)
                            index = i;
                    CacheTable.RemoveAt(index);
                }
                CacheTable.Add(new Node
                {
                    Data = memory,
                    Address = realize,
                    AgeCount = 0
                });
            }
            else
                Interrput.Interrupt(MemoryCache_Type.Over_Flow, address, -1);
        }
        class Node
        {
            public long Address;
            public MemoryBlock Data;
            public byte AgeCount;

            public long Start
            {
                get
                {
                    return Address;
                }
            }
            public long End
            {
                get
                {
                    return Address + Data.LongLength;
                }
            }
        }
    }
}
