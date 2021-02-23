using System;

namespace PL0_Compiler
{
    enum LexicalLabel
    {
        PL0_ConstSym,
        PL0_VarSym,
        PL0_ProcedureSym,
        PL0_BeginSym,
        PL0_EndSym,
        PL0_OddSym,
        PL0_IfSym,
        PL0_ThenSym,
        PL0_CallSym,
        PL0_WhileSym,
        PL0_DoSym,
        PL0_ReadSym,
        PL0_WriteSym,

        PL0_Dot,
        PL0_Comma,
        PL0_Semicolon,
        PL0_Equal,
        PL0_Become,
        PL0_Plus,
        PL0_Sub,
        PL0_LBrace,
        PL0_RBrace,
        PL0_Mul,
        PL0_Div,
        PL0_Nequal,
        PL0_Less,
        PL0_LessEqual,
        PL0_Greater,
        PL0_GreaterQeual,

        PL0_ID,
        PL0_Number,

        ERROR,
    }

    enum OP_PRI
    {
        /// <summary>
        /// 低优先级
        /// </summary>
        LOW,
        /// <summary>
        /// 相等优先级-去括号
        /// </summary>
        EQUAL,
        /// <summary>
        /// 高优先级
        /// </summary>
        HIGH,
        /// <summary>
        /// 错误运算符匹配
        /// </summary>
        NONE,
    }
}
