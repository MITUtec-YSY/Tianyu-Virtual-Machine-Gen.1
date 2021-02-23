using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace SkyVM.AnaModule
{
    /// <summary>
    /// 程序加载表
    /// </summary>
    public class ProgramLoadTable
    {
        private readonly Dictionary<string, int> LoadTable;
        
        /// <summary>
        /// 程序加载表构造函数
        /// </summary>
        public ProgramLoadTable()
        {
            LoadTable = new Dictionary<string, int>();
        }

        /// <summary>
        /// 新加载一个程序
        /// </summary>
        /// <param name="key">程序别名</param>
        /// <param name="value">程序编号</param>
        /// <returns>返回添加状态</returns>
        public bool Add(string key, int value)
        {
            if (!LoadTable.ContainsKey(key))
            {
                LoadTable.Add(key, value);
                return true;
            }
            return false;
        }
        /// <summary>
        /// 删除一个程序
        /// </summary>
        /// <param name="key">程序别名</param>
        public void Del(string key)
        {
            LoadTable.Remove(key);
        }
        /// <summary>
        /// 获取程序编号
        /// </summary>
        /// <param name="key">程序别名</param>
        /// <returns>返回程序编号</returns>
        public int GetProgram(string key)
        {
            int result = -1;
            if (LoadTable.ContainsKey(key))
                result = LoadTable[key];
            return result;
        }
        /// <summary>
        /// 获取程序别名
        /// </summary>
        /// <param name="id">程序ID</param>
        /// <returns>返回别名</returns>
        public string GetProgram(int id)
        {
            string str = null;
            foreach (var i in LoadTable) 
                if (i.Value == id)
                {
                    str = i.Key;
                    break;
                }
            return str;
        }
    }
}
