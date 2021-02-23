using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace PL0_Compiler
{
    class SyntaxAnalysisUnit
    {
        private SignTree SignTree;
        private readonly ProcedureTable Procedure;

        private readonly StreamWriter Writer;
        private readonly IComiler Comiler;

        public SyntaxAnalysisUnit(StreamWriter writer, IComiler comiler)
        {
            Writer = writer;
            Comiler = comiler;
            SignTree = new SignTree("Global", null, "0");
            Procedure = new ProcedureTable(SignTree);
        }

        public void SyntaxAnalysis()
        {
            加载寄存器();
            分程序();
            WriteToFile("end");
        }
        private void 加载寄存器()
        {
            SignTree.AddVar("RAX");
            SignTree.AddVar("RBX");
            SignTree.AddVar("RCX");
            SignTree.AddVar("RDX");
        }
        private void 分程序()
        {
            常量说明部分();
            变量说明部分();
            全局预处理();
            过程说明部分();
            WriteToFile(SignTree.Name, false, false);
            语句();
        }
        private void 全局预处理()
        {
            if ("Global" == SignTree.Name)
            {
                WriteToFile("set(" + SignTree.GetMemSize().ToString() + ")");
                WriteToFile("call(Global)");
            }
        }

        #region 常量处理区
        private void 常量说明部分()
        {
            if (Comiler.Peek().LexicalLabel == LexicalLabel.PL0_ConstSym) 
            {
                Comiler.Read();
                while (true)
                {
                    常量定义();
                    if (Comiler.Peek().LexicalLabel == LexicalLabel.PL0_Semicolon)
                    {
                        Comiler.Read();
                        return;
                    }
                    else if (Comiler.Peek().LexicalLabel == LexicalLabel.PL0_Comma)
                        Comiler.Read();
                    else
                        ThrowException("','、';'");
                }
            }
        }
        private void 常量定义()
        {
            string label = "";
            if (Comiler.Peek().LexicalLabel == LexicalLabel.PL0_ID)
                label = Comiler.Read().GetString();
            else
                ThrowException("标识符");
            if (Comiler.Peek().LexicalLabel == LexicalLabel.PL0_Equal)
                Comiler.Read();
            else
                ThrowException("'='");
            if (Comiler.Peek().LexicalLabel == LexicalLabel.PL0_Number)
                SignTree.AddConst(label, Comiler.Read().GetInt());
            else
                ThrowException("数字");
        }
        #endregion

        #region 变量处理区
        private void 变量说明部分()
        {
            if (Comiler.Peek().LexicalLabel == LexicalLabel.PL0_VarSym)
            {
                Comiler.Read();
                while (true)
                {
                    变量定义();
                    if (Comiler.Peek().LexicalLabel == LexicalLabel.PL0_Semicolon)
                    {
                        Comiler.Read();
                        return;
                    }
                    else if (Comiler.Peek().LexicalLabel == LexicalLabel.PL0_Comma)
                        Comiler.Read();
                    else
                        ThrowException("','、';'");
                }
            }
        }
        private void 变量定义()
        {
            if (Comiler.Peek().LexicalLabel == LexicalLabel.PL0_ID)
                SignTree.AddVar(Comiler.Read().GetString());
            else
                ThrowException("标识符");
        }
        #endregion

        #region 过程处理区
        private void 过程说明部分()
        {
            while (过程首部())
            {
                分程序();
                if (Comiler.Peek().LexicalLabel == LexicalLabel.PL0_Semicolon)
                    Comiler.Read();
                else
                    ThrowException("';'");
                WriteToFile("ret");
                SignTree = SignTree.Father;
            }
        }
        private bool 过程首部()
        {
            if (Comiler.Peek().LexicalLabel == LexicalLabel.PL0_ProcedureSym)
            {
                Comiler.Read();
                if (Comiler.Peek().LexicalLabel == LexicalLabel.PL0_ID)
                {
                    string proce = Comiler.Read().GetString();
                    if (!Procedure.Contains(proce))
                    {
                        SignTree = SignTree.AddItem(proce);
                        Procedure.AddItem(proce, SignTree);
                    }
                    else
                        ThrowException("重复的段：" + proce, true);
                }
                else
                    ThrowException("标识符");
                if (Comiler.Peek().LexicalLabel == LexicalLabel.PL0_Semicolon)
                    Comiler.Read();
                else
                    ThrowException("';'");
                    return true;
            }
            return false;
        }
        #endregion

        #region 语句处理区
        private void 语句()
        {
            switch (Comiler.Peek().LexicalLabel)
            {
                case LexicalLabel.PL0_BeginSym:
                    复合语句();
                    break;
                case LexicalLabel.PL0_IfSym:
                    条件语句();
                    break;
                case LexicalLabel.PL0_CallSym:
                    过程调用();
                    break;
                case LexicalLabel.PL0_WhileSym:
                    循环语句();
                    break;
                case LexicalLabel.PL0_ReadSym:
                    读语句();
                    break;
                case LexicalLabel.PL0_WriteSym:
                    写语句();
                    break;
                case LexicalLabel.PL0_ID:
                    赋值语句();
                    break;
                default:
                    空语句();
                    break;
            }
        }
        private void 赋值语句()
        {
            string source_addr = SignTree.GetVarAddress(Comiler.Peek().GetString());
            if (null == source_addr)
                ThrowException("应为变量", true);
            Comiler.Read();
            if (Comiler.Peek().LexicalLabel == LexicalLabel.PL0_Become)
                Comiler.Read();
            else
                ThrowException("':='");
            表达式();
            WriteToFile("or(4, {" + source_addr + "}, {[0]}, 0)");
        }
        private void 条件语句()
        {
            Comiler.Read();
            string[] condition = 条件();
            if (Comiler.Peek().LexicalLabel == LexicalLabel.PL0_ThenSym)
                Comiler.Read();
            else
                ThrowException("'then'");
            string[] label = SignTree.GetIfLabel();
            for (int i = 0; i < condition.Length; i++)
                WriteToFile(condition[i] + "(" + label[1] + ")");
            语句();
            WriteToFile(label[1], false, false);
        }
        private void 循环语句()
        {
            Comiler.Read();
            string[] label = SignTree.GetWhileLable();
            WriteToFile(label[0], false, false);
            string[] condition = 条件();
            if (Comiler.Peek().LexicalLabel == LexicalLabel.PL0_DoSym)
                Comiler.Read();
            else
                ThrowException("'do'");
            for (int i = 0; i < condition.Length; i++)
                WriteToFile(condition[i] + "(" + label[1] + ")");
            语句();
            WriteToFile("jmp(" + label[0] + ")");
            WriteToFile(label[1], false, false);
        }
        private void 过程调用()
        {
            Comiler.Read();
            if (Comiler.Peek().LexicalLabel == LexicalLabel.PL0_ID)
            {
                string label = Comiler.Peek().GetString();
                if (!Procedure.Contains(label))
                    ThrowException("应为变量", true);
                Comiler.Read();
                string[] path = 过程调用_寻找创建列表(label);
                string save_addr = Procedure.GetItem(path[0]).GetProcedureAddress();
                WriteToFile("push({[" + save_addr + "]})");
                过程调用_内存处理与调用(path, label);
                WriteToFile("pop({" + save_addr + "})");
            }
            else
                ThrowException(" 标识符 ");
        }
        private string[] 过程调用_寻找创建列表(string proce)
        {
            SignTree.GetPath(SignTree, Procedure.GetItem(proce), out string[] call, out string[] into);
            int index = 0;
            for (int i = 0; i < call.Length - 1 && i < into.Length - 1; i++)
                if (call[i] == into[i])
                    index = i;
                else
                    break;
            string[] target = new string[into.Length - index];
            for (int i = 0; i < target.Length; i++)
                target[i] = into[i + index];
            return target;
        }
        private void 过程调用_内存处理与调用(string[] path, string label)
        {
            Stack<string> stack = new Stack<string>();
            for (int i = 0; i < path.Length - 1; i++) 
            {
                string addr = Procedure.GetItem(path[i]).GetProcedureAddress();
                int size = Procedure.GetItem(path[i + 1]).GetMemSize();
                WriteToFile("new({" + addr + "}, " + size.ToString() + ")");
                stack.Push(addr);
            }
            WriteToFile("call(" + label + ")");
            foreach (var i in stack)
                WriteToFile("free({[" + i + "]})");
        }
        private void 读语句()
        {
            Comiler.Read();
            if (Comiler.Peek().LexicalLabel == LexicalLabel.PL0_LBrace)
                Comiler.Read();
            else
                ThrowException("'('");
            while (true)
            {
                if (Comiler.Peek().LexicalLabel == LexicalLabel.PL0_ID)
                {
                    string var_addr = SignTree.GetVarAddress(Comiler.Peek().GetString());
                    if (null == var_addr)
                        ThrowException("应为变量", true);
                    Comiler.Read();
                    WriteToFile("scan({" + var_addr + "}, 4)");
                }
                else
                    ThrowException(" 标识符 ");
                switch (Comiler.Peek().LexicalLabel)
                {
                    case LexicalLabel.PL0_Comma:
                        Comiler.Read();
                        break;
                    case LexicalLabel.PL0_RBrace:
                        Comiler.Read();
                        return;
                    default:
                        ThrowException("','、')'");
                        break;
                }
            }
        }
        private void 写语句()
        {
            Comiler.Read();
            if (Comiler.Peek().LexicalLabel == LexicalLabel.PL0_LBrace)
                Comiler.Read();
            else
                ThrowException("'('");
            WriteToFile("push({[0]})");
            while (true)
            {
                表达式();
                WriteToFile("print(0, 4)");
                switch (Comiler.Peek().LexicalLabel)
                {
                    case LexicalLabel.PL0_Comma:
                        Comiler.Read();
                        break;
                    case LexicalLabel.PL0_RBrace:
                        Comiler.Read();
                        WriteToFile("pop(0)");
                        return;
                    default:
                        ThrowException("','、')'");
                        break;
                }
            }
        }
        private void 复合语句()
        {
            Comiler.Read();
            while (true)
            {
                语句();
                switch (Comiler.Peek().LexicalLabel)
                {
                    case LexicalLabel.PL0_EndSym:
                        Comiler.Read();
                        return;
                    case LexicalLabel.PL0_Semicolon:
                        Comiler.Read();
                        break;
                    default:
                        ThrowException("';'、'end'");
                        break;
                }
            }
        }
        private void 空语句()
        {
            if (Comiler.Peek().LexicalLabel == LexicalLabel.PL0_Semicolon
                || Comiler.Peek().LexicalLabel == LexicalLabel.PL0_Dot
                 || Comiler.Peek().LexicalLabel == LexicalLabel.PL0_EndSym)
                return;
            else
                ThrowException("';'、'.'");
        }
        #endregion

        #region 表达式组
        private void 表达式()
        {
            //表达式终结符：+ - * / ID Number （ ）
            StringBuilder builder = new StringBuilder();
            表达式(builder);
            WriteToFile("or(4, 0, {" + builder.ToString() + "}, 0)");
        }
        private void 表达式(StringBuilder builder)
        {
            表达式_前项处理(builder);
            while (true)
            {
                表达式_项(builder);
                switch (Comiler.Peek().LexicalLabel)
                {
                    case LexicalLabel.PL0_Plus:
                        builder.Append("+");
                        break;
                    case LexicalLabel.PL0_Sub:
                        builder.Append("-");
                        break;
                    default:
                        return;
                }
                Comiler.Read();
            }
        }
        private void 表达式_前项处理(StringBuilder builder)
        {
            switch (Comiler.Peek().LexicalLabel)
            {
                case LexicalLabel.PL0_Sub:
                    builder.Append("0-");
                    goto case LexicalLabel.PL0_Plus;
                case LexicalLabel.PL0_Plus:
                    Comiler.Read();
                    break;
                default:
                    break;
            }
        }
        private void 表达式_项(StringBuilder builder)
        {
            while (true)
            {
                表达式_因子(builder);
                switch (Comiler.Peek().LexicalLabel)
                {
                    case LexicalLabel.PL0_Mul:
                        builder.Append("*");
                        break;
                    case LexicalLabel.PL0_Div:
                        builder.Append("/");
                        break;
                    default:
                        return;
                }
                Comiler.Read();
            }
        }
        private void 表达式_因子(StringBuilder builder)
        {
            switch (Comiler.Peek().LexicalLabel)
            {
                case LexicalLabel.PL0_ID:
                    if (SignTree.Contains(Comiler.Peek().GetString(), out bool flag))
                    {
                        if (flag)
                            builder.Append(SignTree.GetVar(Comiler.Peek().GetString()));
                        else
                            builder.Append(SignTree.GetConst(Comiler.Peek().GetString()).ToString());
                        Comiler.Read();
                    }
                    else
                        ThrowException("未定义的标识符", true);
                    break;
                case LexicalLabel.PL0_Number:
                    builder.Append(Comiler.Peek().GetInt().ToString());
                    Comiler.Read();
                    break;
                case LexicalLabel.PL0_LBrace:
                    builder.Append("(");
                    Comiler.Read();
                    表达式(builder);
                    if (Comiler.Peek().LexicalLabel == LexicalLabel.PL0_RBrace)
                    {
                        builder.Append(")");
                        Comiler.Read();
                    }
                    else
                        ThrowException("')'");
                    break;
            }
        }
        #endregion
        #region 条件
        private string[] 条件()
        {
            if (Comiler.Peek().LexicalLabel == LexicalLabel.PL0_OddSym)
                return Odd条件();
            return 条件_表达式组();
        }
        private string[] 条件_表达式组()
        {
            string[] target = new string[0];
            表达式();
            WriteToFile("or(4, 8, {[0]}, 0)");
            switch (Comiler.Peek().LexicalLabel)
            {
                case LexicalLabel.PL0_Equal:
                    target = new string[] { "js", "jns" };
                    break;
                case LexicalLabel.PL0_Nequal:
                    target = new string[] { "jz" };
                    break;
                case LexicalLabel.PL0_Less:
                    target = new string[] { "jz", "jns" };
                    break;
                case LexicalLabel.PL0_LessEqual:
                    target = new string[] { "jns" };
                    break;
                case LexicalLabel.PL0_Greater:
                    target = new string[] { "jz", "js" };
                    break;
                case LexicalLabel.PL0_GreaterQeual:
                    target = new string[] { "js" };
                    break;
                default:
                    ThrowException("'='、'#'、'<'、'<='、'>'、'>='");
                    break;
            }
            Comiler.Read();
            表达式();
            WriteToFile("or(4, 4, {[0]}, 0)");
            WriteToFile("or(4, 0, {[8]}, 0)");
            WriteToFile("sub(4, 0, {[0]}, {[4]})");
            return target;
        }
        private string[] Odd条件()
        {
            Comiler.Read();
            表达式();
            WriteToFile("sub(4, 0, 0, {[0]})");
            return new string[] { "jz" };
        }
        #endregion

        private void WriteToFile(string str, bool space = true, bool semi = true)
        {
            if (space)
                Writer.WriteLine("\t" + str + (semi ? ";" : ""));
            else
                Writer.WriteLine(str + (semi ? ";" : ":"));
        }
        private void ThrowException(string mean, bool flag = false)
        {
            if (!flag)
                throw new SyntaxAnalysisException(Comiler.Peek().Line, new string[]
                    {
                        "错误的符号",
                        "期望的符号： " + mean,
                        "当前的符号： " + Comiler.Peek().GetString()
                    });
            else
                throw new SyntaxAnalysisException(Comiler.Peek().Line, new string[]
                    {
                        "错误",
                        mean,
                        "当前的符号： " + Comiler.Peek().GetString()
                    });
        }

    }
}
