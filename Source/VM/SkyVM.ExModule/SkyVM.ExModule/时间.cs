using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using SkyVM.InterfaceModule;

namespace SkyVM.ExModule
{
    /// <summary>
    /// 时间类
    /// </summary>
    public class Time : TimeBase
    {
        private readonly TimerUnit TimeUnit;

        /// <summary>
        /// 时间类构造函数
        /// </summary>
        /// <param name="path">时间文件路径</param>
        public Time(string path = null)
        {
            TimeUnit = new TimerUnit(path);
        }

        /// <summary>
        /// 读取时间方法
        /// </summary>
        /// <param name="type">读取时间类型</param>
        /// <returns>返回时间</returns>
        public short ReadTime(TimeOperation_Type type)
        {
            return TimeUnit.GetDate(type);
        }
        /// <summary>
        /// 写入时间方法
        /// </summary>
        /// <param name="data">时间数据</param>
        /// <param name="type">写入时间类型</param>
        public void WriteTime(short data, TimeOperation_Type type)
        {
            TimeUnit.SetDate(data, type);
        }
        /// <summary>
        /// 同步时间
        /// </summary>
        public void SyncTime()
        {
            TimeUnit.Sync();
        }
        /// <summary>
        /// 保存时间
        /// </summary>
        public void Save()
        {
            TimeUnit.Save();
        }

        /// <summary>
        /// 时间组件
        /// </summary>
        class TimerUnit
        {
            private readonly string Path;
            private TimeSpan SubTime;

            /// <summary>
            /// 时间组件构造函数
            /// </summary>
            public TimerUnit() : this(NormalPath) { }
            /// <summary>
            /// 时间组件构造函数
            /// </summary>
            /// <param name="path">时间文件保存地址</param>
            public TimerUnit(string path)
            {
                Path = path ?? NormalPath;
                if (File.Exists(Path))
                {
                    using (FileStream fs = new FileStream(Path, FileMode.Open))
                    {
                        try
                        {
                            BinaryFormatter bf = new BinaryFormatter();
                            SubTime = (TimeSpan)bf.Deserialize(fs);
                        }
                        catch (Exception) { }
                        finally
                        {
                            SubTime = new TimeSpan(0, 0, 0, 0, 0);
                            fs.Close();
                        }
                    }
                }
                else
                    SubTime = new TimeSpan(0, 0, 0, 0, 0);
            }

            /// <summary>
            /// 时间同步
            /// </summary>
            public void Sync()
            {
                SubTime -= SubTime;
            }
            /// <summary>
            /// 设置时间
            /// </summary>
            /// <param name="data">设置的数据</param>
            /// <param name="type">设置的数据类型</param>
            public void SetDate(short data, TimeOperation_Type type)
            {
                DateTime time = DateTime.Now - SubTime;
                switch (type)
                {
                    case TimeOperation_Type.Day:
                        time = time.AddDays(data);
                        break;
                    case TimeOperation_Type.Hour:
                        time = time.AddHours(data);
                        break;
                    case TimeOperation_Type.Minute:
                        time = time.AddMinutes(data);
                        break;
                    case TimeOperation_Type.Month:
                        time = time.AddMonths(data);
                        break;
                    case TimeOperation_Type.Second:
                        time = time.AddSeconds(data);
                        break;
                    case TimeOperation_Type.Year:
                    default:
                        time = time.AddYears(data);
                        break;
                }
                SubTime = DateTime.Now - time;
            }
            /// <summary>
            /// 获取时间
            /// </summary>
            /// <param name="type">获取的数据类型</param>
            /// <returns>返回时间</returns>
            public short GetDate(TimeOperation_Type type)
            {
                DateTime time = DateTime.Now - SubTime;
                int result;
                switch (type)
                {
                    case TimeOperation_Type.Day:
                        result = time.Day;
                        break;
                    case TimeOperation_Type.Hour:
                        result = time.Hour;
                        break;
                    case TimeOperation_Type.Minute:
                        result = time.Minute;
                        break;
                    case TimeOperation_Type.Month:
                        result = time.Month;
                        break;
                    case TimeOperation_Type.Second:
                        result = time.Second;
                        break;
                    case TimeOperation_Type.Year:
                    default:
                        result = time.Year;
                        break;
                }
                return (short)result;
            }
            /// <summary>
            /// 保存时间
            /// </summary>
            public void Save()
            {
                using (FileStream fs = new FileStream(Path, FileMode.Create))
                {
                    try
                    {
                        BinaryFormatter bf = new BinaryFormatter();
                        bf.Serialize(fs, SubTime);
                    }
                    catch (Exception) { }
                    finally
                    {
                        fs.Close();
                    }
                }
            }

            private static readonly string NormalPath = @"SkyTimer.txsw";
        }
    }
}
