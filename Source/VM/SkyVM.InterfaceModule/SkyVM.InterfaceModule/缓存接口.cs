using System;
using System.Collections.Generic;
using System.Text;

namespace SkyVM.InterfaceModule
{
    /// <summary>
    /// 虚拟机缓存接口
    /// </summary>
    public  abstract class ICache
    {
        /// <summary>
        /// 缓存中断接口
        /// </summary>
        protected IMemoryCacheInterrupt Interrput;

        /// <summary>
        /// 缓存接口构造函数
        /// </summary>
        /// <param name="interrupt"></param>
        public ICache(IMemoryCacheInterrupt interrupt)
        {
            Interrput = interrupt;
        }

        /// <summary>
        /// 读数据
        /// </summary>
        /// <param name="segment">段地址</param>
        /// <param name="page">页地址</param>
        /// <param name="size">读取长度</param>
        /// <returns>返回数据</returns>
        public abstract byte[] Get(long segment, long page, int size);

        /// <summary>
        /// 写数据
        /// </summary>
        /// <param name="segment">段地址</param>
        /// <param name="page">页地址</param>
        /// <param name="data">写入数据</param>
        public abstract void Set(long segment, long page, byte[] data);
        /// <summary>
        /// 块损坏-应释放的块
        /// </summary>
        /// <param name="segment">段地址</param>
        /// <param name="page">页地址</param>
        public abstract void Bad(long segment, long page);
    }
}
