using System;
using System.Collections.Generic;
using System.Text;

namespace SkyVM.ExModule
{
    /// <summary>
    /// 文件操作类型
    /// </summary>
    public enum File_Type
    {
        /// <summary>
        /// 目录
        /// </summary>
        Entry,
        /// <summary>
        /// 文件
        /// </summary>
        File,
    }
    /// <summary>
    /// 文件写入方式
    /// </summary>
    public enum Write_Type
    {
        /// <summary>
        /// 追加方式
        /// </summary>
        Append,
        /// <summary>
        /// 覆盖方式
        /// </summary>
        Cover,
        /// <summary>
        /// 新建文件并写入
        /// </summary>
        Create
    }
}
