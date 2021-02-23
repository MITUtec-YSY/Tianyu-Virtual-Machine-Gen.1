using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using SkyVM.InterfaceModule;

namespace SkyVM.ExModule
{
    /// <summary>
    /// 内存组件
    /// </summary>
    public class MemoryUnit : MemoryBase
    {
        /// <summary>
        /// 正在使用的内存
        /// </summary>
        public override MemoryNode[] Using
        {
            get
            {
                MemoryNode[] nodes = new MemoryNode[OccupyTable.Count];
                int i = 0;
                foreach (var temp in OccupyTable)
                    nodes[i++] = new MemoryNode
                    {
                        Mem_ID = temp.Key.Start,
                        Mem_Size = temp.Value.LongLength
                    };
                return nodes;
            }
        }
        /// <summary>
        /// 空闲的内存
        /// </summary>
        public override MemoryNode[] Freeing
        {
            get
            {
                MemoryNode[] nodes = new MemoryNode[FreeTable.Count];
                int i = 0;
                foreach (var temp in FreeTable)
                    nodes[i++] = new MemoryNode
                    {
                        Mem_ID = temp.Key,
                        Mem_Size = temp.Value
                    };
                return nodes;
            }
        }

        /// <summary>
        /// 内存大小
        /// </summary>
        public override long MemorySize => MemSize;

        private readonly long MemSize;
        
        private readonly short Alignment;
        private readonly short MemOffset;
        private readonly SortedList<long, long> FreeTable;
        private readonly SortedList<AddressBlock, MemoryBlock> OccupyTable;

        /// <summary>
        /// 内存组件构造函数
        /// </summary>
        /// <param name="mem_size">内存大小</param>
        /// <param name="mem_align">内存对齐大小</param>
        /// <param name="mem_offset">内存分配允许偏移量</param>
        public MemoryUnit(long mem_size, short mem_align, short mem_offset)
        {
            MemSize = mem_size;
            Alignment = mem_align;
            MemOffset = mem_offset;
            FreeTable = new SortedList<long, long>
            {
                { 0, mem_size }
            };
            OccupyTable = new SortedList<AddressBlock, MemoryBlock>();
        }

        /// <summary>
        /// 内存空间申请
        /// </summary>
        /// <param name="apply_size">请求内存大小</param>
        /// <param name="realize_size">返回实际内存大小</param>
        /// <returns>返回内存块号</returns>
        public override long ApplyMemory(long apply_size, out long realize_size)
        {
            long result = Apply(apply_size, out realize_size);
            if (-1 == result)
            {
                MemoryFreeTrim();
                result = Apply(apply_size, out realize_size);
            }
            if (-1 != result)
                OccupyTable.Add(new AddressBlock
                {
                    Start = result,
                    End = result + realize_size
                }, new MemoryBlock((int)realize_size));
            return result;
        }
        /// <summary>
        /// 释放内存
        /// </summary>
        /// <param name="mem_id">内存块号</param>
        public override void ReleaseMemory(long mem_id)
        {
            for (int i = 0; i < OccupyTable.Count; i++)
            {
                AddressBlock temp = OccupyTable.Keys[i];
                if (mem_id == temp.Start)
                {
                    FreeTable.Add(mem_id, OccupyTable.Values[i].LongLength);
                    OccupyTable.Remove(new AddressBlock
                    {
                        Start = mem_id,
                        End = mem_id
                    });
                    break;
                }
            }
        }
        /// <summary>
        /// 内存段引用方法（将需要操作的内存块段整体读取-用于上层缓存使用）
        /// </summary>
        /// <param name="mem_id">读取的内存块号</param>
        /// <param name="realize_id">实际的内存段起始号</param>
        /// <returns>返回内存段</returns>
        public override MemoryBlock ReadMemory(long mem_id, out long realize_id)
        {
            MemoryBlock block = null;
            realize_id = -1;

            for(int i = 0; i < OccupyTable.Count; i++)
            {
                AddressBlock temp = OccupyTable.Keys[i];
                if (mem_id >= temp.Start && mem_id < temp.End)
                {
                    block = OccupyTable.Values[i];
                    realize_id = temp.Start;
                    break;
                }
            }

            return block;
        }
        

        #region 内部方法组
        /// <summary>
        /// 内存申请后台方法
        /// </summary>
        /// <param name="apply_size">请求内存大小</param>
        /// <param name="realize_size">返回实际分配内存大小</param>
        /// <returns>返回内存块号</returns>
        private long Apply(long apply_size, out long realize_size)
        {
            long result = -1;
            realize_size = 0;
            if (0 < apply_size)
            {
                long size = (apply_size % Alignment == 0) ? apply_size : ((apply_size / Alignment) + 1) * Alignment;
                bool find = false;
                foreach (var i in FreeTable)
                {
                    if (i.Value - size >= 0 && i.Value - size <= MemOffset)
                    {
                        result = i.Key;
                        find = true;
                        realize_size = size;
                        FreeTable.Remove(result);
                        break;
                    }
                }
                if (!find)
                {
                    long key = 0, value = 0;
                    foreach (var i in FreeTable)
                        if (i.Value > value)
                        {
                            key = i.Key;
                            value = i.Value;
                        }
                    if (value > size)
                    {
                        result = key;
                        realize_size = size;
                        FreeTable.Remove(key);
                        FreeTable.Add(key + size, value);
                    }
                }
            }
            return result;
        }
        /// <summary>
        /// 空闲内存整理方法（当申请内存失败时，进行零碎空间合并）
        /// </summary>
        private void MemoryFreeTrim()
        {
            for (int i = 0; i < FreeTable.Count;)
            {
                long id = FreeTable.Keys[i];
                long size = FreeTable.Values[i];
                if (FreeTable.ContainsKey(id + size))
                {
                    size += FreeTable[id + size];
                    FreeTable.Remove(id + size);
                    FreeTable.Remove(id);
                    FreeTable.Add(id, size);
                }
                else
                    i++;
            }
        }
        #endregion


        #region 内部类组
        class AddressBlock : IComparable
        {
            public long Start;
            public long End;

            public int CompareTo(object obj)
            {
                int result;
                if (null != obj)
                    if (obj.GetType() == GetType())
                        if ((AddressBlock)obj == this)
                            result = 0;
                        else if ((obj as AddressBlock).Start > Start)
                            result = -1;
                        else
                            result = 1;
                    else
                        result = -1;
                else
                    result = 1;

                return result;
            }

            public override bool Equals(object obj)
            {
                bool result = false;

                if (null != obj)
                    if (obj.GetType() == GetType())
                        result = (AddressBlock)obj == this;

                return result;
            }
            public override int GetHashCode()
            {
                return base.GetHashCode();
            }

            public static bool operator ==(AddressBlock left, AddressBlock right)
            {
                bool result;

                if(!(left is null))
                {
                    if (!(right is null))
                    {
                        if (left.Start == right.Start)
                            result = true;
                        else
                            result = false;
                    }
                    else
                        result = false;
                }
                else
                {
                    if (!(right is null))
                        result = false;
                    else
                        result = true;
                }

                return result;
            }
            public static bool operator !=(AddressBlock left, AddressBlock right)
            {
                return !(left == right);
            }
        }
        #endregion
    }
}
