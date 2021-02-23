using System;
using System.Collections.Generic;
using System.Text;

namespace PL0_Compiler
{
    class LexAnalysisException : Exception
    {
        private readonly long Line;
        private readonly string Symbol;

        public LexAnalysisException(long line, string sym) : base()
        {
            Line = line;
            Symbol = sym;
        }

        public override string Message
        {
            get
            {
                return "错误的字符" + Symbol + " 在第 " + Line + " 行";
            }
        }
    }

    class SyntaxAnalysisException : Exception
    {
        private readonly long Line;
        private readonly string[] Str;

        public SyntaxAnalysisException(long line, string[] vs) : base()
        {
            Line = line;
            Str = vs;
        }

        public override string Message
        {
            get
            {
                StringBuilder builder = new StringBuilder();
                builder.AppendLine("错误 在第 " + Line + " 行");
                foreach (var i in Str)
                    builder.AppendLine(i);
                return builder.ToString();
            }
        }
    }

    class SignRepeatException : Exception
    {
        public string SignName { get; private set; }
        public SignRepeatException(string name) : base()
        {
            SignName = name;
        }
    }
}
