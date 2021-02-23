using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using SkyVM.AnaModule;
using SkyVM.CoreModule;
using SkyVM.InterfaceModule;
using System.Numerics;
using static SkyVM.AnaModule.InstructionList;

namespace SkyVM.ExModule
{
    /// <summary>
    /// 核心程序执行单元
    /// </summary>
    public sealed class CoreCodeExecutorUnit : CodeExecutor
    {
        /// <summary>
        /// 核心程序执行单元构造函数
        /// </summary>
        /// <param name="control">核心控制器接口</param>
        /// <param name="complex">复杂运算单元接口</param>
        /// <param name="cache_size">缓存大小</param>
        public CoreCodeExecutorUnit(IExecutorControl control, IComplexOperation complex, int cache_size = 3) : base(-1, control, complex, cache_size)
        {
        }

        /// <summary>
        /// 执行方法
        /// </summary>
        protected override void Run()
        {
            var instr = Process.Next();
            switch (instr.Instructuion)
            {
                case Instructuions_Package.NOP:
                    Instruction_Nop();
                    break;
                case Instructuions_Package.Ret:
                    Instruction_Ret();
                    break;
                case Instructuions_Package.Jmp:
                    Instruction_Jmp((string)instr.Parameters[0].Parameter);
                    break;
                case Instructuions_Package.JNS:
                    Instruction_JNS((string)instr.Parameters[0].Parameter);
                    break;
                case Instructuions_Package.JS:
                    Instruction_JS((string)instr.Parameters[0].Parameter);
                    break;
                case Instructuions_Package.JZ:
                    Instruction_JZ((string)instr.Parameters[0].Parameter);
                    break;
                case Instructuions_Package.Sleep:
                    Instruction_Sleep(instr.Parameters[0]);
                    break;
                case Instructuions_Package.PUSH:
                    Instruction_Push(instr.Parameters[0]);
                    break;
                case Instructuions_Package.POP:
                    Instruction_Pop(instr.Parameters[0]);
                    break;
                case Instructuions_Package.Calculate:
                    Instruction_Calculate(instr.Parameters);
                    break;
                case Instructuions_Package.Rand:
                    Instruction_Rand(instr.Parameters);
                    break;
                case Instructuions_Package.Scan:
                    Instruction_Scan(instr.Parameters);
                    break;
                case Instructuions_Package.Print:
                    Instruction_Print(instr.Parameters);
                    break;
                case Instructuions_Package.Putc:
                    Instruction_Putc(instr.Parameters[0]);
                    break;
                case Instructuions_Package.Connect:
                    Instruction_Connect(instr.Parameters);
                    break;
                case Instructuions_Package.Send:
                    Instruction_Send(instr.Parameters);
                    break;
                case Instructuions_Package.Receive:
                    Instruction_Receive(instr.Parameters);
                    break;
                case Instructuions_Package.GetPort:
                    Instruction_GetPort(instr.Parameters);
                    break;
                case Instructuions_Package.Create:
                    Instruction_Create(instr.Parameters);
                    break;
                case Instructuions_Package.Delete:
                    Instruction_Delete(instr.Parameters);
                    break;
                case Instructuions_Package.Position:
                    Instruction_Position(instr.Parameters);
                    break;
                case Instructuions_Package.New_Memory:
                    Instruction_NewMemory(instr.Parameters);
                    break;
                case Instructuions_Package.Free_Memory:
                    Instruction_FreeMemory(instr.Parameters);
                    break;
                case Instructuions_Package.Init_Memory:
                    Instruction_InitMemory(instr.Parameters);
                    break;
                case Instructuions_Package.Read_Time:
                    Instruction_ReadTime(instr.Parameters);
                    break;
                case Instructuions_Package.Write_Time:
                    Instruction_WriteTime(instr.Parameters);
                    break;
                case Instructuions_Package.Sync_Time:
                    Instruction_SyncTime();
                    break;
                case Instructuions_Package.Act_Device:
                    Instruction_ActDevice(instr.Parameters[0]);
                    break;
                case Instructuions_Package.Pause_Device:
                    Instruction_PauseDevice(instr.Parameters[0]);
                    break;
                case Instructuions_Package.Reset_Device:
                    Instruction_ResetDevice(instr.Parameters[0]);
                    break;
                case Instructuions_Package.Close_Device:
                    Instruction_CloseDevice(instr.Parameters[0]);
                    break;
                case Instructuions_Package.Read_Device:
                    Instruction_ReadDevice(instr.Parameters);
                    break;
                case Instructuions_Package.Write_Device:
                    Instruction_WriteDevice(instr.Parameters);
                    break;
                case Instructuions_Package.Pause_Net:
                    Instruction_PauseNet(instr.Parameters[0]);
                    break;
                case Instructuions_Package.Reset_Net:
                    Instruction_ResetNet(instr.Parameters[0]);
                    break;
                case Instructuions_Package.Close_Net:
                    Instruction_CloseNet(instr.Parameters[0]);
                    break;
                case Instructuions_Package.Open_File:
                    Instruction_OpenFile(instr.Parameters);
                    break;
                case Instructuions_Package.Close_File:
                    Instruction_CloseFile(instr.Parameters[0]);
                    break;
                case Instructuions_Package.Read_File:
                    Instruction_ReadFile(instr.Parameters);
                    break;
                case Instructuions_Package.Write_File:
                    Instruction_WriteFile(instr.Parameters);
                    break;
                case Instructuions_Package.Copy_File:
                    Instruction_CopyFile(instr.Parameters);
                    break;
                case Instructuions_Package.Init_Process:
                    Instruction_InitProcess(instr.Parameters[0]);
                    break;
                case Instructuions_Package.End_Process:
                    Instruction_EndProcess();
                    break;
                case Instructuions_Package.Switch_Process:
                    Instruction_SwitchProcess();
                    break;
                case Instructuions_Package.Init_Thread:
                    Instruction_InitThread(instr.Parameters);
                    break;
                case Instructuions_Package.End_Thread:
                    Instruction_EndThread(instr.Parameters[0]);
                    break;
                case Instructuions_Package.Outside_Call:
                    Instruction_OutsideCall(instr.Parameters);
                    break;
                case Instructuions_Package.Local_Call:
                    Instruction_LocalCall(instr.Parameters[0]);
                    break;
                case Instructuions_Package.END:
                    Instruction_End();
                    break;
                case Instructuions_Package.Set_TimeSlice:
                    Instruction_SetTimeSlice(instr.Parameters);
                    break;
                case Instructuions_Package.Get_TimeSlice:
                    Instruction_GetTimeSlice(instr.Parameters);
                    break;
                case Instructuions_Package.Get_Core:
                    Instruction_GetCore(instr.Parameters[0]);
                    break;
                default:
                    goto case Instructuions_Package.NOP;
            }
        }
    }
}
