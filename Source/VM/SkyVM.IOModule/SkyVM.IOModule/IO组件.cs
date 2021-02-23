using System;
using System.Collections.Generic;
using SkyVM.InterfaceModule;
using System.Collections;


namespace SkyVM.IOModule
{
    /// <summary>
    /// 设备控制器
    /// </summary>
    public class IOUnit : IDeviceControl, IDeviceInterrupt
    {
        /// <summary>
        /// 设备中断号-设备标识符对照表
        /// </summary>
        private readonly Dictionary<int, string> InterruptTables;
        /// <summary>
        /// 设备号-设备标识符对照表
        /// </summary>
        private readonly Dictionary<int, string> DeviceIDTables;
        /// <summary>
        /// 设备标识符-设备主体对照表
        /// </summary>
        private readonly Dictionary<string, DeviceTable> DeviceTables;
        /// <summary>
        /// 空闲中断表
        /// </summary>
        private readonly SortedList<int, ushort> FreeInterruptTable;
        /// <summary>
        /// IO组件构造函数
        /// </summary>
        /// <param name="interrupt">设备控制器中断接口</param>
        public IOUnit(IDeviceControlInterrupt interrupt):base(interrupt)
        {
            InterruptTables = new Dictionary<int, string>();
            DeviceIDTables = new Dictionary<int, string>();
            DeviceTables = new Dictionary<string, DeviceTable>();
            FreeInterruptTable = new SortedList<int, ushort>
            {
                { 0, ushort.MaxValue }
            };
        }

        /// <summary>
        /// 添加一个设备
        /// </summary>
        /// <param name="interrupt_id">设备中断号</param>
        /// <param name="device">设备实例</param>
        public override void AddDevice(int interrupt_id, IOBase device)
        {
            if (!DeviceTables.ContainsKey(device.Property.DeviceID))
            {
                int interrupt = -1 == interrupt_id ? GetInterruptID() : interrupt_id;
                InterruptTables.Add(interrupt, device.Property.DeviceID);
                DeviceTables.Add(device.Property.DeviceID, new DeviceTable(DeviceIDTables.Count,interrupt, new List<string>(), device));
                DeviceIDTables.Add(DeviceIDTables.Count, device.Property.DeviceID);
            }
            else
                //无设备ID则用-1
                ControlInterrupt.Interrupt(DeviceException_Type.Add_Failed, -1, device);
        }
        /// <summary>
        /// 移除一个设备
        /// </summary>
        /// <param name="device_id">设备中断号</param>
        public override void DeleteDevice(int device_id)
        {
            if (DeviceIDTables.TryGetValue(device_id, out string label))
                if (DeviceTables.TryGetValue(label, out DeviceTable device))
                {
                    InterruptTables.Remove(device.InterruptID);
                    DeviceTables.Remove(label);
                    DeviceIDTables.Remove(device_id);
                }
                else
                    DeviceIDTables.Remove(device_id);
        }
        /// <summary>
        /// 激活设备
        /// </summary>
        /// <param name="interrupt_id">设备中断号</param>
        public override void DeviceActive(int interrupt_id)
        {
            if(InterruptTables.TryGetValue(interrupt_id,out string label))
                if (DeviceTables.TryGetValue(label, out DeviceTable table))
                    if (table.RunState)
                        table.DevicePointer.Run();
                    else
                        ControlInterrupt.Interrupt(DeviceException_Type.Not_Exist, table.Device_ID, table.DevicePointer);
        }
        /// <summary>
        /// 关闭设备
        /// </summary>
        /// <param name="interrupt_id">设备中断号</param>
        public override void DeviceClose(int interrupt_id)
        {
            if (InterruptTables.TryGetValue(interrupt_id, out string label))
                if (DeviceTables.TryGetValue(label, out DeviceTable table))
                    if (table.RunState)
                        table.DevicePointer.Stop();
                    else
                        ControlInterrupt.Interrupt(DeviceException_Type.Not_Exist, table.Device_ID, table.DevicePointer);
        }
        /// <summary>
        /// 暂停设备
        /// </summary>
        /// <param name="interrupt_id">设备中断号</param>
        public override void DevicePause(int interrupt_id)
        {
            if (InterruptTables.TryGetValue(interrupt_id, out string label))
                if (DeviceTables.TryGetValue(label, out DeviceTable table))
                    if (table.RunState)
                        table.DevicePointer.Pause();
                    else
                        ControlInterrupt.Interrupt(DeviceException_Type.Not_Exist, table.Device_ID, table.DevicePointer);
        }
        /// <summary>
        /// 读设备
        /// </summary>
        /// <param name="interrupt_id">设备中断号</param>
        /// <returns>返回数据</returns>
        public override byte[] DeviceRead(int interrupt_id)
        {
            if (InterruptTables.TryGetValue(interrupt_id, out string label))
                if (DeviceTables.TryGetValue(label, out DeviceTable table))
                    if (table.RunState)
                        return table.DevicePointer.Get();
                    else
                        ControlInterrupt.Interrupt(DeviceException_Type.Not_Exist, table.Device_ID, table.DevicePointer);
            return null;
        }
        /// <summary>
        /// 复位设备
        /// </summary>
        /// <param name="interrupt_id">设备中断号</param>
        public override void DeviceReset(int interrupt_id)
        {
            if (InterruptTables.TryGetValue(interrupt_id, out string label))
                if (DeviceTables.TryGetValue(label, out DeviceTable table))
                    if (table.RunState)
                        table.DevicePointer.Reset();
                    else
                        ControlInterrupt.Interrupt(DeviceException_Type.Not_Exist, table.Device_ID, table.DevicePointer);
        }
        /// <summary>
        /// 写设备
        /// </summary>
        /// <param name="interrupt_id">设备中断号</param>
        /// <param name="data">数据包</param>
        public override void DeviceWrite(int interrupt_id, byte[] data)
        {
            if (InterruptTables.TryGetValue(interrupt_id, out string label))
                if (DeviceTables.TryGetValue(label, out DeviceTable table))
                    if (table.RunState)
                        table.DevicePointer.Set(data);
                    else
                        ControlInterrupt.Interrupt(DeviceException_Type.Not_Exist, table.Device_ID, table.DevicePointer);
        }
        /// <summary>
        /// 获取指定设备的信息
        /// </summary>
        /// <param name="interrupt_id">设备中断号</param>
        /// <returns>返回设备数据</returns>
        public override object GetDevice(int interrupt_id)
        {
            if (InterruptTables.TryGetValue(interrupt_id, out string label))
                if (DeviceTables.TryGetValue(label, out DeviceTable table))
                    return table;
            return null;
        }
        /// <summary>
        /// 获取所有设备的信息
        /// </summary>
        /// <returns>返回设备数据组</returns>
        public override object[] GetDevices()
        {
            return new List<DeviceTable>(DeviceTables.Values).ToArray();
        }
        /// <summary>
        /// 载入一个设备
        /// </summary>
        /// <param name="interrupt_id">设备中断号</param>
        /// <param name="device">设备实例</param>
        public override void LoadDevice(int interrupt_id, IOBase device)
        {
            if(null == device)
                if (InterruptTables.TryGetValue(interrupt_id, out string label))
                    if (DeviceTables.TryGetValue(label, out DeviceTable table))
                        table.RunState = true;
                    else
                        ControlInterrupt.Interrupt(DeviceException_Type.Not_Exist, -1, null);
                else
                    ControlInterrupt.Interrupt(DeviceException_Type.Not_Exist, -1, null);
            else
                AddDevice(interrupt_id, device);
        }
        /// <summary>
        /// 卸载一个设备
        /// </summary>
        /// <param name="interrupt_id">设备中断号</param>
        public override void UnloadDevice(int interrupt_id)
        {
            if (InterruptTables.TryGetValue(interrupt_id, out string label))
                if (DeviceTables.TryGetValue(label, out DeviceTable table))
                    table.RunState = false;
        }


        /// <summary>
        /// 设备管理器异常处理方法
        /// </summary>
        /// <param name="exception">设备异常类型</param>
        /// <param name="device_id">设备中断号</param>
        void IDeviceInterrupt.Interrupt(DeviceException_Type exception, string device_id)
        {
            switch (exception)
            {
                case DeviceException_Type.Add_Failed:
                    break;
                case DeviceException_Type.Not_Exist:
                    break;
                case DeviceException_Type.Off_Line:
                    break;
                case DeviceException_Type.Work_Failed:
                    if(DeviceTables.TryGetValue(device_id, out DeviceTable table))
                        base.ControlInterrupt.Interrupt(DeviceException_Type.Work_Failed, table.Device_ID, table.DevicePointer);
                    break;
            }
        }

        
        /// <summary>
        /// 添加中断服务程序
        /// </summary>
        /// <param name="interrupt_id">设备中断号</param>
        /// <param name="programs">服务程序</param>
        public override void AddInterruptService(int interrupt_id, params string[] programs)
        {
            if(InterruptTables.TryGetValue(interrupt_id,out string label))
                if (DeviceTables.TryGetValue(label, out DeviceTable table))
                    for (int i = 0; i < programs.Length; i++)
                        table.InterruptService.Add(programs[i]);
        }
        /// <summary>
        /// 移除设备的指定中断服务程序
        /// </summary>
        /// <param name="interrupt_id">设备中断号</param>
        /// <param name="program">中断服务程序</param>
        public override void DelInterruptService(int interrupt_id, string program)
        {
            if (InterruptTables.TryGetValue(interrupt_id, out string label))
                if (DeviceTables.TryGetValue(label, out DeviceTable table))
                    table.InterruptService.Remove(program);
        }
        /// <summary>
        /// 清除设备中断服务程序
        /// </summary>
        /// <param name="interrupt_id">设备中断号</param>
        public override void CleanInterruptService(int interrupt_id)
        {
            if (InterruptTables.TryGetValue(interrupt_id, out string label))
                if (DeviceTables.TryGetValue(label, out DeviceTable table))
                    table.InterruptService.Clear();
        }
        /// <summary>
        /// 设置新的设备中断号
        /// </summary>
        /// <param name="device_id">设备号</param>
        /// <param name="new_interrupt">中断号</param>
        public override void SetInterrupt(int device_id, int new_interrupt)
        {
            if(DeviceIDTables.TryGetValue(device_id,out string label))
                if (DeviceTables.TryGetValue(label, out DeviceTable device))
                {
                    InterruptTables.Remove(device.InterruptID);
                    FreeInterruptID((ushort)device.InterruptID);
                    if (SetInterruptID((ushort)new_interrupt))
                        device.InterruptID = new_interrupt;
                    else
                        ControlInterrupt.Interrupt(DeviceException_Type.Interrupt_Error, device_id, device.DevicePointer);
                }
                else
                    ControlInterrupt.Interrupt(DeviceException_Type.Interrupt_Error, device_id, null);
            else
                ControlInterrupt.Interrupt(DeviceException_Type.Interrupt_Error, device_id, null);
        }


        private int GetInterruptID()
        {
            int result = -1;

            if (0 < FreeInterruptTable.Count)
            {
                int key = FreeInterruptTable.Keys[0];
                ushort value = FreeInterruptTable[key];
                FreeInterruptTable.Remove(key);
                result = key;
                key++;
                if (key <= value)
                    FreeInterruptTable.Add(key, value);
            }

            return result;
        }
        private bool SetInterruptID(ushort interrupt_id)
        {
            bool result = false;
            for (int i = 0; i < FreeInterruptTable.Count; i++)
            {
                int key = FreeInterruptTable.Keys[i];
                ushort value = FreeInterruptTable[key];
                if (interrupt_id >= key && interrupt_id <= value)
                {
                    FreeInterruptTable.Remove(key);
                    if (interrupt_id - 1 >= key)
                        FreeInterruptTable.Add(key, (ushort)(interrupt_id - 1));
                    if (interrupt_id + 1 <= value)
                        FreeInterruptTable.Add((ushort)(interrupt_id + 1), value);
                    result = true;
                    break;
                }
                else if (interrupt_id < key)
                    break;
            }
            return result;
        }
        private void FreeInterruptID(ushort interrupt_id)
        {
            if (!FreeInterruptTable.ContainsKey(interrupt_id))
            {
                FreeInterruptTable.Add(interrupt_id, interrupt_id);
                for(int i = 0; i < FreeInterruptTable.Count - 1; i++)
                {
                    int key = FreeInterruptTable.Keys[i];
                    ushort value = FreeInterruptTable.Values[i];
                    if (value + 1 == FreeInterruptTable.Keys[i + 1])
                    {
                        value = FreeInterruptTable.Values[i + 1];
                        int k2 = FreeInterruptTable.Keys[i + 1];
                        FreeInterruptTable.Remove(key);
                        FreeInterruptTable.Remove(k2);
                        FreeInterruptTable.Add(key, value);
                        break;
                    }
                }
            }
        }


        class DeviceTable
        {
            public readonly int Device_ID;
            public int InterruptID;
            public List<string> InterruptService;
            public IOBase DevicePointer;
            /// <summary>
            /// 设备运行时状态-true：设备在线 | false：设备已卸载
            /// </summary>
            public bool RunState;

            public DeviceTable(int device_id, int interrupt_id, List<string> InterruptService,IOBase DevicePointer)
            {
                RunState = true;
                Device_ID = device_id;
                InterruptID = interrupt_id;
                this.InterruptService = InterruptService;
                this.DevicePointer = DevicePointer;
            }
        }
    }
}
