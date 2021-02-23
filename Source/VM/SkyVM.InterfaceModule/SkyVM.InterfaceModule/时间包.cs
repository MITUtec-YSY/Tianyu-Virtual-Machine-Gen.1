using System;
using System.Collections.Generic;
using System.Text;

namespace SkyVM.InterfaceModule
{
    /// <summary>
    /// 时间操作接口
    /// </summary>
    public interface TimeBase
    {
        /// <summary>
        /// 同步时间
        /// </summary>
        void SyncTime();
        /// <summary>
        /// 读取时间方法
        /// </summary>
        /// <param name="type">读取时间类型</param>
        /// <returns>返回时间</returns>
        short ReadTime(TimeOperation_Type type);
        /// <summary>
        /// 写入时间方法
        /// </summary>
        /// <param name="data">时间数据</param>
        /// <param name="type">写入时间类型</param>
        void WriteTime(short data, TimeOperation_Type type);
        /// <summary>
        /// 保存时间
        /// </summary>
        void Save();
    }

    /// <summary>
    /// 时间操作类型
    /// </summary>
    public enum TimeOperation_Type
    {
        /// <summary>
        /// 年
        /// </summary>
        Year,
        /// <summary>
        /// 月
        /// </summary>
        Month,
        /// <summary>
        /// 日
        /// </summary>
        Day,
        /// <summary>
        /// 小时
        /// </summary>
        Hour,
        /// <summary>
        /// 分钟
        /// </summary>
        Minute,
        /// <summary>
        /// 秒
        /// </summary>
        Second,
    }
}
