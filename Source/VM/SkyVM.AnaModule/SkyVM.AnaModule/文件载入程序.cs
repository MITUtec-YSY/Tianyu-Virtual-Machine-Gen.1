using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using SkyVM.AnaModule;
using static SkyVM.AnaModule.ParameterList;

namespace SkyVM.AnaModule
{
    /// <summary>
    /// 运算表达式过程表
    /// </summary>
    public class Expression
    {
        /// <summary>
        /// 获取当前过程数
        /// </summary>
        public int Count => OPList.Count;
        /// <summary>
        /// 获取当前表达式标签
        /// </summary>
        private int Label_N;
        private readonly List<OP> OPList;

        /// <summary>
        /// 运算表达式过程表构造函数
        /// </summary>
        public Expression()
        {
            OPList = new List<OP>();
            Label_N = -1;
        }

        /// <summary>
        /// 添加一个运算过程
        /// </summary>
        /// <param name="op">运算符</param>
        /// <param name="n1f">运算数1-类型</param>
        /// <param name="n1">运算数1</param>
        /// <param name="n2f">运算数2-类型</param>
        /// <param name="n2">运算数2</param>
        /// <returns>返回运算过程标号</returns>
        public int Add(Expression_Operation op, bool n1f, object n1, bool n2f, object n2)
        {
            OPList.Add(new OP
            {
                Operation = op,
                Label = Label_N + 1,
                N1 = n1,
                N1_Flag = n1f,
                N2 = n2,
                N2_Flag = n2f
            });
            Label_N++;
            return Label_N;
        }

        /// <summary>
        /// 运算过程索引器
        /// </summary>
        /// <param name="index">索引</param>
        /// <returns>返回过程</returns>
        public OP this[int index]
        {
            get
            {
                if (0 <= index && OPList.Count > index)
                    return OPList[index];
                return null;
            }
        }

        /// <summary>
        /// 运算过程类（false：立即数 | true：过程号）
        /// </summary>
        public class OP
        {
            /// <summary>
            /// 运算操作
            /// </summary>
            public Expression_Operation Operation;
            /// <summary>
            /// 运算数1-类型
            /// </summary>
            public bool N1_Flag;
            /// <summary>
            /// 运算数1
            /// </summary>
            public object N1;
            /// <summary>
            /// 运算数2-类型
            /// </summary>
            public bool N2_Flag;
            /// <summary>
            /// 运算数2
            /// </summary>
            public object N2;
            /// <summary>
            /// 运算过程标签
            /// </summary>
            public int Label;
        }

        /// <summary>
        /// 运算表达式操作
        /// </summary>
        public enum Expression_Operation
        {
            /// <summary>
            /// 加运算
            /// </summary>
            ADD,
            /// <summary>
            /// 减运算
            /// </summary>
            SUB,
            /// <summary>
            /// 乘运算
            /// </summary>
            MUL,
            /// <summary>
            /// 除运算
            /// </summary>
            DIV,
            /// <summary>
            /// 与运算
            /// </summary>
            AND,
            /// <summary>
            /// 或运算
            /// </summary>
            OR,
            /// <summary>
            /// 非运算
            /// </summary>
            NOT,
            /// <summary>
            /// 异或运算
            /// </summary>
            XOR,
            /// <summary>
            /// 取值运算
            /// </summary>
            GET,
        }
    }


    /// <summary>
    /// 文件载入程序
    /// </summary>
    public class FileLoad
    {
        private StreamReader Reader;
        private string Path;
        private int Line;
        private readonly int MemorySize;
        private readonly IToFileLoad ToFileLoad;
        private bool Error;

        /// <summary>
        /// 文件载入程序构造函数
        /// </summary>
        /// <param name="memory_size">默认程序空间</param>
        /// <param name="toFile">文件载入程序上次接口</param>
        public FileLoad(int memory_size, IToFileLoad toFile)
        {
            MemorySize = memory_size;
            ToFileLoad = toFile;
            Path = "";
            Error = false;
        }

        /// <summary>
        /// 编译程序
        /// </summary>
        /// <param name="path">程序路径</param>
        /// <returns>返回程序代码段</returns>
        public ProgramUnit Complete(string path)
        {
            try
            {
                Reader = new StreamReader(path);
            }
            catch(IOException ioe)
            {
                ErrorOperation(0x30, ioe.Message);
                return null;
            }
            catch(Exception e)
            {
                ErrorOperation(0x31, e.Message);
                return null;
            }
            Error = false;
            Path = path;
            Line = 1;
            ProgramUnit program = GetProgram();
            Reader.Close();
            return Error ? null : program;
        }

        /// <summary>
        /// 执行文件编译
        /// </summary>
        /// <returns>返回程序段</returns>
        private ProgramUnit GetProgram()
        {
            ProgramUnit program = new ProgramUnit
            {
                MemorySize = MemorySize
            };
            string labeltemp = ""; //临时标识符储存器
            Instructuions_Package instructuions = Instructuions_Package.END; //临时指令操作
            Queue<ParameterPackage> queue = new Queue<ParameterPackage>();

            Vocabulary_Type vocabulary;
            object obj;
            int state = 0;
            bool finish = false;
            while (!finish && !Error && !Reader.EndOfStream)
            {
                switch (state)
                {
                    case 0:
                        obj = GetVocabulary(out vocabulary);
                        if (null != obj)
                            switch (vocabulary)
                            {
                                case Vocabulary_Type.STR:
                                    labeltemp = (string)obj;
                                    state = 1;
                                    break;
                                case Vocabulary_Type.END:
                                    state = 7;
                                    break;
                                default:
                                    ErrorOperation(0x00, "错误的语法");
                                    break;
                            }
                        break;
                    case 1:
                        obj = GetVocabulary(out vocabulary);
                        if (null != obj)
                           switch (vocabulary)
                            {
                                case Vocabulary_Type.SIGN:
                                    if (":" == (string)obj || "：" == (string)obj)
                                        state = 2;
                                    else
                                    {
                                        instructuions = GetInstructuions(labeltemp);
                                        if (Instructuions_Package.Calculate == instructuions)
                                            queue.Enqueue(new ParameterPackage
                                            {
                                                Parameter = labeltemp,
                                                Type = Parameter_Type.STR
                                            });
                                        if ("(" == (string)obj || "（" == (string)obj)
                                            state = 3;
                                        else if (";" == (string)obj || "；" == (string)obj || "。" == (string)obj)
                                            state = 6;
                                        else
                                            ErrorOperation(0x00, "错误的语法");
                                    }
                                    break;
                                default:
                                    ErrorOperation(0x00, "错误的语法");
                                    break;
                            }
                        break;
                    case 2:
                        program.AddID(labeltemp);
                        labeltemp = "";
                        state = 0;
                        break;
                    case 3:
                        obj = GetVocabulary(out vocabulary);
                        if (null != obj)
                            switch (vocabulary)
                            {
                                case Vocabulary_Type.Expression:
                                    queue.Enqueue(new ParameterPackage
                                    {
                                        Parameter = obj,
                                        Type = Parameter_Type.Expression
                                    });
                                    break;
                                case Vocabulary_Type.BigNumber:
                                    queue.Enqueue(new ParameterPackage
                                    {
                                        Parameter = obj,
                                        Type = Parameter_Type.BigNumber
                                    });
                                    break;
                                case Vocabulary_Type.Long:
                                    queue.Enqueue(new ParameterPackage
                                    {
                                        Parameter = obj,
                                        Type = Parameter_Type.Long
                                    });
                                    break;
                                case Vocabulary_Type.STR:
                                    queue.Enqueue(new ParameterPackage
                                    {
                                        Parameter = obj,
                                        Type = Parameter_Type.STR
                                    });
                                    break;
                                default:
                                    ErrorOperation(0x00, "错误的语法");
                                    break;
                            }
                        state = 4;
                        break;
                    case 4:
                        obj = GetVocabulary(out vocabulary);
                        if (null != obj) 
                            if (Vocabulary_Type.SIGN == vocabulary)
                            {
                                if (")" == (string)obj || "）" == (string)obj)
                                    state = 5;
                                else if ("," == (string)obj || "，" == (string)obj)
                                    state = 3;
                                else
                                    ErrorOperation(0x00, "错误的语法");
                            }
                            else
                                ErrorOperation(0x00, "错误的语法");
                        break;
                    case 5:
                        obj = GetVocabulary(out vocabulary);
                        if (null != obj)
                            if (Vocabulary_Type.SIGN == vocabulary && (";" == (string)obj || "；" == (string)obj || "。" == (string)obj))
                                state = 6;
                            else
                                ErrorOperation(0x00, "错误的语法");
                        break;
                    case 6:
                        if (CheckInstructuion(instructuions, queue.ToArray()))
                            program.Add(instructuions, queue.ToArray());
                        else
                            ErrorOperation(0x01, "错误的方法类型");
                        queue.Clear();
                        instructuions = Instructuions_Package.END;
                        labeltemp = "";
                        state = 0;
                        break;
                    case 7:
                        finish = true;
                        break;
                }
            }

            program.EndRead();
            return program;
        }
        /// <summary>
        /// 词法分析与单词获取
        /// </summary>
        /// <param name="vocabulary">单词类型</param>
        /// <returns>返回单词</returns>
        private object GetVocabulary(out Vocabulary_Type vocabulary)
        {
            Queue<char> queue = new Queue<char>();
            int state = 0;
            char ch = (char)Reader.Read();
            bool strflag = false;
            while (!Reader.EndOfStream && !Error)
            {
                switch (state)
                {
                    case 0:
                        switch (ch)
                        {
                            case '\'':
                            case '’': //注释处理
                                state = 16;
                                break;
                            case '\n':
                                Line++;
                                ch = (char)Reader.Read();
                                break;
                            case ' ':
                            case '\t':
                            case '\r':
                                ch = (char)Reader.Read();
                                break;
                            case ',':
                            case ':':
                            case ';':
                            case '(':
                            case ')':
                            case '，':
                            case '：':
                            case '；':
                            case '。':
                            case '（':
                            case '）':
                                queue.Enqueue(ch);
                                state = 1; //进入单符号终止状态
                                break;
                            case '{': //进入表达式模式
                                ch = (char)Reader.Read();
                                state = 2;
                                break;
                            default:
                                queue.Enqueue(ch);
                                if ('-' == ch)
                                {
                                    state = 0;
                                    ch = (char)Reader.Read();
                                }
                                else if ('0' == ch)
                                    state = 3; //以0开头的数字类型
                                else if ('1' <= ch && '9' >= ch)
                                    state = 4; //非0开头的数字类型
                                else if ('a' <= ch && 'z' >= ch || 'A' <= ch && 'Z' >= ch)
                                    state = 5; //以字母开头的字符串类型
                                else if (0x4E00 <= ch && 0x9FBB >= ch)
                                    state = 5; //以字母开头的字符串类型
                                else if ('\"' == ch || '“' == ch)
                                {
                                    state = 5;
                                    strflag = true;
                                }
                                else
                                    ErrorOperation(0x10, "错误的字符 " + ch);
                                break;
                        }
                        break;
                    case 1: //单符号-结束
                        vocabulary = Vocabulary_Type.SIGN;
                        return new string(queue.ToArray());
                    case 2: //表达式模式
                        if ('{' == ch)
                            ErrorOperation(0x10, "错误的字符 " + ch);
                        else if ('}' == ch) //表达式结束
                            state = 6;
                        else
                        {
                            queue.Enqueue(ch);
                            ch = (char)Reader.Read();
                        }
                        break;
                    case 3: //以0开头的数字
                        switch ((char)Reader.Peek())
                        {
                            case 'x':
                            case 'X': //16进制数字模式
                                Reader.Read();
                                state = 8;
                                break;
                            case 'b':
                            case 'B': //2进制数字模式
                                Reader.Read();
                                state = 9;
                                break;
                            default:
                                if ('0' <= (char)Reader.Peek() && '7' >= (char)Reader.Peek())  //8进制数字模式
                                {
                                    queue.Enqueue((char)Reader.Read());
                                    state = 10;
                                }
                                else if ('8' == (char)Reader.Peek() || '9' == (char)Reader.Peek())  //10进制数字模式
                                {
                                    queue.Enqueue((char)Reader.Read());
                                    state = 11;
                                }
                                else if (',' == (char)Reader.Peek() || '，' == (char)Reader.Peek() || '）' == (char)Reader.Peek() || ')' == (char)Reader.Peek()) //数字结束-数字为零
                                    state = 15;
                                else if (' ' == (char)Reader.Peek())
                                {
                                    while (' ' == (char)Reader.Peek() || '\t' == (char)Reader.Peek())
                                        Reader.Read();
                                    state = 15;
                                }
                                else
                                    ErrorOperation(0x10, "错误的字符 " + (char)Reader.Peek());
                                break;
                        }
                        break;
                    case 4:
                        switch ((char)Reader.Peek())
                        {
                            case '，':
                            case '）':
                            case ',':
                            case ')': //10进制数字结束
                                state = 15;
                                break;
                            default:
                                if ('0' <= (char)Reader.Peek() && '9' >= (char)Reader.Peek()) //正式进入10进制模式
                                {
                                    queue.Enqueue((char)Reader.Read());
                                    state = 11;
                                }
                                else
                                    ErrorOperation(0x10, "错误的字符 " + (char)Reader.Peek());
                                break;
                        }
                        break;
                    case 5: //字符串模式
                        if (strflag)
                        {
                            switch ((char)Reader.Peek())
                            {
                                case '\n':
                                    Line++;
                                    queue.Enqueue((char)Reader.Read());
                                    break;
                                case '”':
                                case '\"':
                                    queue.Enqueue((char)Reader.Read());
                                    state = 7; //字符串读取结束
                                    break;
                                default:
                                    queue.Enqueue((char)Reader.Read());
                                    break;
                            }
                        }
                        else
                        {
                            switch ((char)Reader.Peek())
                            {
                                case '\n':
                                    Line++;
                                    queue.Enqueue((char)Reader.Read());
                                    break;
                                case ';':
                                case '；':
                                case ':':
                                case ',':
                                case ')':
                                case '(':
                                case '：':
                                case '，':
                                case '）':
                                case '（':
                                    state = 7; //字符串读取结束
                                    break;
                                default:
                                    queue.Enqueue((char)Reader.Read());
                                    break;
                            }
                        }
                        break;
                    case 6: //表达式结束处理
                        vocabulary = Vocabulary_Type.Expression;
                        object obj = ExpressionOption(new string(queue.ToArray()), out bool f);
                        if (f)
                            vocabulary = Vocabulary_Type.Long;
                        return obj;
                    case 7: //字符串处理结束
                        vocabulary = Vocabulary_Type.STR;
                        if (strflag)
                            return (new string(queue.ToArray()).Replace("\"", "")).Replace("“", "").Replace("”", "");
                        else
                            return new string(queue.ToArray());
                    case 8: //16进制数字模式
                        switch ((char)Reader.Peek())
                        {
                            case '，':
                            case '）': 
                            case ',':
                            case ')': //16进制数字结束
                                state = 12;
                                break;
                            default:
                                if ('0' <= (char)Reader.Peek() && '9' >= (char)Reader.Peek())
                                    queue.Enqueue((char)Reader.Read());
                                else if ('a' <= (char)Reader.Peek() && 'f' >= (char)Reader.Peek() || 'A' <= (char)Reader.Peek() && 'F' >= (char)Reader.Peek())
                                    queue.Enqueue((char)Reader.Read());
                                else
                                    ErrorOperation(0x10, "错误的字符 " + (char)Reader.Peek());
                                break;
                        }
                        break;
                    case 9: //2进制数字模式
                        switch ((char)Reader.Peek())
                        {
                            case '，':
                            case '）':
                            case ',':
                            case ')': //2进制数字结束
                                state = 13;
                                break;
                            default:
                                if ('0' == (char)Reader.Peek() || '1' >= (char)Reader.Peek())
                                    queue.Enqueue((char)Reader.Read());
                                else
                                    ErrorOperation(0x10, "错误的字符 " + (char)Reader.Peek());
                                break;
                        }
                        break;
                    case 10: //8进制数字模式
                        switch ((char)Reader.Peek())
                        {
                            case '，':
                            case '）': //8进制数字结束
                            case ',':
                            case ')': //8进制数字结束
                                state = 14;
                                break;
                            default:
                                if ('0' <= (char)Reader.Peek() && '7' >= (char)Reader.Peek())
                                    queue.Enqueue((char)Reader.Read());
                                else if ('8' == (char)Reader.Peek() || '9' == (char)Reader.Peek())
                                {
                                    queue.Enqueue((char)Reader.Read());
                                    state = 11; //跳转进入10进制模式
                                }
                                else
                                    ErrorOperation(0x10, "错误的字符 " + (char)Reader.Peek());
                                break;
                        }
                        break;
                    case 11: //10进制数字模式
                        switch ((char)Reader.Peek())
                        {
                            case '，':
                            case '）':
                            case ',':
                            case ')': //10进制数字结束
                                state = 15;
                                break;
                            default:
                                if ('0' <= (char)Reader.Peek() && '9' >= (char)Reader.Peek())
                                    queue.Enqueue((char)Reader.Read());
                                else
                                    ErrorOperation(0x10, "错误的字符 " + (char)Reader.Peek());
                                break;
                        }
                        break;
                    case 12: //16进制数字模式结束
                        {
                            BigInteger integer = 0;
                            bool flag = true;
                            for (; 0 < queue.Count;)
                            {
                                switch (queue.Dequeue())
                                {
                                    case '0':
                                        integer = integer * 16 + 0;
                                        break;
                                    case '1':
                                        integer = integer * 16 + 1;
                                        break;
                                    case '2':
                                        integer = integer * 16 + 2;
                                        break;
                                    case '3':
                                        integer = integer * 16 + 3;
                                        break;
                                    case '4':
                                        integer = integer * 16 + 4;
                                        break;
                                    case '5':
                                        integer = integer * 16 + 5;
                                        break;
                                    case '6':
                                        integer = integer * 16 + 6;
                                        break;
                                    case '7':
                                        integer = integer * 16 + 7;
                                        break;
                                    case '8':
                                        integer = integer * 16 + 8;
                                        break;
                                    case '9':
                                        integer = integer * 16 + 9;
                                        break;
                                    case 'A':
                                    case 'a':
                                        integer = integer * 16 + 10;
                                        break;
                                    case 'B':
                                    case 'b':
                                        integer = integer * 16 + 11;
                                        break;
                                    case 'C':
                                    case 'c':
                                        integer = integer * 16 + 12;
                                        break;
                                    case 'D':
                                    case 'd':
                                        integer = integer * 16 + 13;
                                        break;
                                    case 'E':
                                    case 'e':
                                        integer = integer * 16 + 14;
                                        break;
                                    case 'F':
                                    case 'f':
                                        integer = integer * 16 + 15;
                                        break;
                                    case '-':
                                        flag = false;
                                        break;
                                }
                            }
                            vocabulary = Vocabulary_Type.BigNumber;
                            return flag ? integer : -integer;
                        }
                    case 13: //2进制数字模式结束
                        {
                            BigInteger integer = 0;
                            bool flag = true;
                            for (; 0 < queue.Count;)
                            {
                                switch (queue.Dequeue())
                                {
                                    case '0':
                                        integer = integer * 2 + 0;
                                        break;
                                    case '1':
                                        integer = integer * 2 + 1;
                                        break;
                                    case '-':
                                        flag = false;
                                        break;
                                }
                            }
                            vocabulary = Vocabulary_Type.BigNumber;
                            return flag ? integer : -integer;
                        }
                    case 14: //8进制数字模式结束
                        {
                            BigInteger integer = 0;
                            bool flag = true;
                            for (; 0 < queue.Count;)
                            {
                                switch (queue.Dequeue())
                                {
                                    case '0':
                                        integer = integer * 8 + 0;
                                        break;
                                    case '1':
                                        integer = integer * 8 + 1;
                                        break;
                                    case '2':
                                        integer = integer * 8 + 2;
                                        break;
                                    case '3':
                                        integer = integer * 8 + 3;
                                        break;
                                    case '4':
                                        integer = integer * 8 + 4;
                                        break;
                                    case '5':
                                        integer = integer * 8 + 5;
                                        break;
                                    case '6':
                                        integer = integer * 8 + 6;
                                        break;
                                    case '7':
                                        integer = integer * 8 + 7;
                                        break;
                                    case '-':
                                        flag = false;
                                        break;
                                }
                            }
                            vocabulary = Vocabulary_Type.BigNumber;
                            return flag ? integer : -integer;
                        }
                    case 15: //10进制数字模式结束
                        {
                            BigInteger integer = 0;
                            bool flag = true;
                            for (; 0 < queue.Count;)
                            {
                                switch (queue.Dequeue())
                                {
                                    case '0':
                                        integer = integer * 10 + 0;
                                        break;
                                    case '1':
                                        integer = integer * 10 + 1;
                                        break;
                                    case '2':
                                        integer = integer * 10 + 2;
                                        break;
                                    case '3':
                                        integer = integer * 10 + 3;
                                        break;
                                    case '4':
                                        integer = integer * 10 + 4;
                                        break;
                                    case '5':
                                        integer = integer * 10 + 5;
                                        break;
                                    case '6':
                                        integer = integer * 10 + 6;
                                        break;
                                    case '7':
                                        integer = integer * 10 + 7;
                                        break;
                                    case '8':
                                        integer = integer * 10 + 8;
                                        break;
                                    case '9':
                                        integer = integer * 10 + 9;
                                        break;
                                    case '-':
                                        flag = false;
                                        break;
                                }
                            }
                            vocabulary = Vocabulary_Type.BigNumber;
                            return flag ? integer : -integer;
                        }
                    case 16: //注释处理
                        do
                        {
                            ch = (char)Reader.Read();
                        } while ('\n' != ch && 0xFFFF != ch);
                        state = 0;
                        break;
                }

            }

            vocabulary = Vocabulary_Type.END;
            if (0xFFFF != ch) 
            {
                switch (ch)
                {
                    case '\n':
                        Line++;
                        break;
                    case ' ':
                    case '\t':
                    case '\r':
                        break;
                    case ',':
                    case ':':
                    case ';':
                    case '(':
                    case ')':
                    case '，':
                    case '：':
                    case '；':
                    case '。':
                    case '（':
                    case '）':
                        queue.Enqueue(ch);
                        vocabulary = Vocabulary_Type.SIGN;
                        return new string(queue.ToArray());
                    default:
                        ErrorOperation(0x10, "错误的字符 " + ch);
                        break;
                }
            }
            return null;
        }

        /// <summary>
        /// 表达式处理
        /// </summary>
        /// <param name="source">表达式源字符串</param>
        /// <param name="flag">返回值类型标识-（false：表达式类型 | true：值类型）</param>
        /// <returns>返回表达式结果</returns>
        private object ExpressionOption(string source, out bool flag)
        {
            Expression expression = null;
            flag = false;

            if (null != source)
            {
                string str = source;
                bool finish = false;
                expression = new Expression();
                source = source.Replace(" ", "");
                source += "#";
                Stack<char> opsign = new Stack<char>();  //运算符栈
                Stack<Node> stack = new Stack<Node>();  //运算数栈
                Queue<char> temp = new Queue<char>();  //临时栈（用于保存运算数，并进行转换）
                opsign.Push('#');

                int state = 0;
                for (int i = 0; i < source.Length && !finish && !Error; i++)
                {
                    switch (state)
                    {
                        case 0: //初始状态
                            switch (source[i])
                            {
                                case '[':
                                case '【':
                                    opsign.Push('[');
                                    break;
                                case '(':
                                case '（':
                                    opsign.Push('(');
                                    break;
                                case '~':
                                    opsign.Push('~');
                                    break;
                                default:
                                    if ('-' == source[i])
                                        temp.Enqueue('-');
                                    else if ('0' == source[i]) //以0开头的数字类型（2/8/16进制）
                                    {
                                        temp.Enqueue('0');
                                        state = 1;
                                    }
                                    else if ('1' <= source[i] && '9' >= source[i]) //非0开头的数字类型（10进制数）
                                    {
                                        temp.Enqueue(source[i]);
                                        state = 2;
                                    }
                                    else
                                        ErrorOperation(0x21, "错误的 符号 存在于表达式 " + str + " 中");
                                    break;
                            }
                            break;
                        case 1: //以0开头数字处理
                            switch (source[i])
                            {
                                case 'b':
                                case 'B':
                                    state = 3; //2进制数
                                    break;
                                case 'x':
                                case 'X':
                                    state = 4; //16进制数
                                    break;
                                default:
                                    if ('0' <= source[i] && '7' >= source[i]) //8进制数
                                    {
                                        temp.Enqueue(source[i]);
                                        state = 5;
                                    }
                                    else if ('8' == source[i] || '9' == source[i]) //10进制数
                                    {
                                        temp.Enqueue(source[i]);
                                        state = 2;
                                    }
                                    else if (OPSET(source[i]))
                                    {
                                        state = 2;
                                        i--;
                                    }
                                    else
                                        ErrorOperation(0x21, "错误的 符号 存在于表达式 " + str + " 中");
                                    break;
                            }
                            break;
                        case 2: //以非0开头的10进制数处理
                            if (!OPSET(source[i])) //非运算符
                            {
                                if ('0' <= source[i] && '9' >= source[i])
                                    temp.Enqueue(source[i]);
                                else
                                    ErrorOperation(0x21, "错误的 符号 存在于表达式 " + str + " 中");
                            }
                            else //运算符
                            {
                                stack.Push(new Node
                                {
                                    data = Convert.ToInt32(new string(temp.ToArray()), 10),
                                    flag = false
                                });
                                temp.Clear();
                                i--;
                                state = 6; //进入运算符处理模块
                            }
                            break;
                        case 3: //以0开头的2进制数处理
                            if (!OPSET(source[i])) //非运算符
                            {
                                if ('0' == source[i] || '1' == source[i])
                                    temp.Enqueue(source[i]);
                                else
                                    ErrorOperation(0x21, "错误的 符号 存在于表达式 " + str + " 中");
                            }
                            else //运算符
                            {
                                stack.Push(new Node
                                {
                                    data = Convert.ToInt32(new string(temp.ToArray()), 2),
                                    flag = false
                                });
                                temp.Clear();
                                i--;
                                state = 6; //进入运算符处理模块
                            }
                            break;
                        case 4: //以0开头的16进制数处理
                            if (!OPSET(source[i])) //非运算符
                            {
                                if ('0' <= source[i] && '9' >= source[i] || 'a' <= source[i] && 'z' >= source[i] || 'A' <= source[i] && 'Z' >= source[i])
                                    temp.Enqueue(source[i]);
                                else
                                    ErrorOperation(0x21, "错误的 符号 存在于表达式 " + str + " 中");
                            }
                            else //运算符
                            {
                                stack.Push(new Node
                                {
                                    data = Convert.ToInt32(new string(temp.ToArray()), 16),
                                    flag = false
                                });
                                temp.Clear();
                                i--;
                                state = 6; //进入运算符处理模块
                            }
                            break;
                        case 5: //以0开头的8进制数处理
                            if (!OPSET(source[i])) //非运算符
                            {
                                if ('0' <= source[i] || '7' >= source[i])
                                    temp.Enqueue(source[i]);
                                else if ('8' == source[i] || '9' == source[i])
                                {
                                    temp.Enqueue(source[i]);
                                    state = 2; //出现大于7的数-进入10进制数模式
                                }
                                else
                                    ErrorOperation(0x21, "错误的 符号 存在于表达式 " + str + " 中");
                            }
                            else //运算符
                            {
                                stack.Push(new Node
                                {
                                    data = Convert.ToInt32(new string(temp.ToArray()), 8),
                                    flag = false
                                });
                                temp.Clear();
                                i--;
                                state = 6; //进入运算符处理模块
                            }
                            break;
                        case 6: //运算符处理
                            switch (OPPRI(opsign.Peek(), source[i]))
                            {
                                case OP_PRI.LOW: //上一个运算符运算优先级低于当前运算符
                                    opsign.Push(source[i]);
                                    state = 0;  //返回初始状态
                                    break;
                                case OP_PRI.EQUAL: //括号匹配，脱括号
                                    opsign.Pop();
                                    if ('#' == source[i])
                                        finish = true;
                                    else if (']' == source[i] || '】' == source[i])
                                    {
                                        if (':' != source[i + 1] && '：' != source[i + 1])
                                            stack.Push(new Node
                                            {
                                                data = expression.Add(Expression.Expression_Operation.GET, stack.Peek().flag, stack.Pop().data, false, (long)4),
                                                flag = true
                                            });
                                    }
                                    else if (!OPSET(source[i + 1])) //判断括号后一个字符是否为运算符，若非运算符则引发运算符错误异常
                                        ErrorOperation(0x21, "错误的 符号 存在于表达式 " + str + " 中");
                                    break;
                                case OP_PRI.HIGH: //上一个运算符运算优先级高于当前运算符
                                    Expression.Expression_Operation operation = Expression.Expression_Operation.GET;
                                    switch (opsign.Pop())
                                    {
                                        case '+':
                                            operation = Expression.Expression_Operation.ADD;
                                            break;
                                        case '-':
                                            operation = Expression.Expression_Operation.SUB;
                                            break;
                                        case '*':
                                            operation = Expression.Expression_Operation.MUL;
                                            break;
                                        case '/':
                                            operation = Expression.Expression_Operation.DIV;
                                            break;
                                        case '&':
                                            operation = Expression.Expression_Operation.AND;
                                            break;
                                        case '|':
                                            operation = Expression.Expression_Operation.OR;
                                            break;
                                        case '~':
                                            operation = Expression.Expression_Operation.NOT;
                                            break;
                                        case '^':
                                            operation = Expression.Expression_Operation.XOR;
                                            break;
                                    }
                                    try
                                    {
                                        if (Expression.Expression_Operation.NOT != operation)
                                        {
                                            Node node2 = stack.Pop(), node1 = stack.Pop();
                                            stack.Push(CALAULATE(node1, operation, node2, expression));
                                        }
                                        else
                                            stack.Push(CALAULATE(stack.Peek(), operation, stack.Pop(), expression));
                                    }
                                    catch (Exception e)
                                    {
                                        ErrorOperation(0x22, "错误的 运算符 存在于表达式 " + str + " 中" + "\r\n" + e.Message);
                                    }
                                    i--;
                                    break;
                                case OP_PRI.NONE: //错误的运算符搭配
                                    ErrorOperation(0x22, "错误的 运算符 存在于表达式 " + str + " 中");
                                    break;
                            }
                            break;
                        default:
                            break;
                    }
                }

                if (0 < stack.Count)
                {
                    if (!stack.Peek().flag)
                    {
                        flag = true;
                        return stack.Peek().data;
                    }
                    expression.Add(Expression.Expression_Operation.OR, stack.Peek().flag, stack.Peek().data, false, (long)0);
                }
            }

            return Error ? null : expression;
        }

        /// <summary>
        /// 表达式单过程求值
        /// </summary>
        /// <param name="a">运算数1</param>
        /// <param name="sign">运算符</param>
        /// <param name="b">运算数2</param>
        /// <param name="expression">运算过程表</param>
        /// <returns>返回运算结果数</returns>
        private Node CALAULATE(Node a, Expression.Expression_Operation sign, Node b, Expression expression)
        {
            Node node;
            if (!a.flag && !b.flag && Expression.Expression_Operation.GET != sign)
            {
                long temp = 0;
                switch (sign)
                {
                    case Expression.Expression_Operation.ADD:
                        temp = a.data + b.data;
                        break;
                    case Expression.Expression_Operation.SUB:
                        temp = a.data - b.data;
                        break;
                    case Expression.Expression_Operation.MUL:
                        temp = a.data * b.data;
                        break;
                    case Expression.Expression_Operation.DIV:
                        temp = a.data / b.data;
                        break;
                    case Expression.Expression_Operation.AND:
                        temp = a.data & b.data;
                        break;
                    case Expression.Expression_Operation.OR:
                        temp = a.data | b.data;
                        break;
                    case Expression.Expression_Operation.NOT:
                        temp = ~a.data;
                        break;
                    case Expression.Expression_Operation.XOR:
                        temp = a.data ^ b.data;
                        break;
                    default:
                        ErrorOperation(0x20, "错误的运算 " + a.data + " " + sign.ToString() + " " + b.data);
                        break;
                }
                node = new Node
                {
                    data = temp,
                    flag = false
                };
            }
            else
            {
                if (Expression.Expression_Operation.NOT != sign)
                    node = new Node
                    {
                        data = expression.Add(sign, a.flag, a.data, b.flag, b.data),
                        flag = true
                    };
                else
                    node = new Node
                    {
                        data = expression.Add(sign, a.flag, a.data, false, (long)0),
                        flag = true
                    };
            }
            return node;
        }

        private void ErrorOperation(int exception_code, string message)
        {
            Error = true;
            ToFileLoad.Interrupt(exception_code, Line, message, Path);
        }

        #region 指令操作函数段

        /// <summary>
        /// 获取操作指令
        /// </summary>
        /// <param name="str">指令字符串</param>
        /// <returns>返回指令</returns>
        private Instructuions_Package GetInstructuions(string str)
        {
            Instructuions_Package package;

            switch (str.ToUpper())
            {
                case "NOP":
                case "等待":
                    package = Instructuions_Package.NOP;
                    break;
                case "SLEEP":
                case "休眠":
                    package = Instructuions_Package.Sleep;
                    break;
                case "SET":
                case "设定内存":
                    package = Instructuions_Package.Set_Memory;
                    break;
                case "NEW":
                case "申请内存":
                    package = Instructuions_Package.New_Memory;
                    break;
                case "FREE":
                case "释放内存":
                    package = Instructuions_Package.Free_Memory;
                    break;
                case "MINIT":
                case "初始内存":
                    package = Instructuions_Package.Init_Memory;
                    break;
                case "TREAD":
                case "获取时间":
                    package = Instructuions_Package.Read_Time;
                    break;
                case "TWRITE":
                case "写入时间":
                    package = Instructuions_Package.Write_Time;
                    break;
                case "TSYNC":
                case "时间同步":
                    package = Instructuions_Package.Sync_Time;
                    break;
                case "DACT":
                case "激活设备":
                    package = Instructuions_Package.Act_Device;
                    break;
                case "DPAUSE":
                case "暂停设备":
                    package = Instructuions_Package.Pause_Device;
                    break;
                case "DRESET":
                case "复位设备":
                    package = Instructuions_Package.Reset_Device;
                    break;
                case "DCLOSE":
                case "设备关闭":
                    package = Instructuions_Package.Close_Device;
                    break;
                case "DREAD":
                case "读设备":
                    package = Instructuions_Package.Read_Device;
                    break;
                case "DWRITE":
                case "写设备":
                    package = Instructuions_Package.Write_Device;
                    break;
                case "PRINT":
                case "显示输出":
                    package = Instructuions_Package.Print;
                    break;
                case "PUTC":
                case "显示字符":
                    package = Instructuions_Package.Putc;
                    break;
                case "SCAN":
                case "键盘输入":
                    package = Instructuions_Package.Scan;
                    break;
                case "CONNECT":
                case "新建连接":
                    package = Instructuions_Package.Connect;
                    break;
                case "NPAUSE":
                case "暂停连接":
                    package = Instructuions_Package.Pause_Net;
                    break;
                case "NRESET":
                case "回复连接":
                    package = Instructuions_Package.Reset_Net;
                    break;
                case "STOP":
                case "结束连接":
                    package = Instructuions_Package.Close_Net;
                    break;
                case "GETPORT":
                case "获取端口":
                    package = Instructuions_Package.GetPort;
                    break;
                case "SEND":
                case "发送数据":
                    package = Instructuions_Package.Send;
                    break;
                case "RECEIVE":
                case "接收数据":
                    package = Instructuions_Package.Receive;
                    break;
                case "CREATE":
                case "新建":
                    package = Instructuions_Package.Create;
                    break;
                case "DELETE":
                case "删除":
                    package = Instructuions_Package.Delete;
                    break;
                case "FOPEN":
                case "打开文件":
                    package = Instructuions_Package.Open_File;
                    break;
                case "FCLOSE":
                case "关闭文件":
                    package = Instructuions_Package.Close_File;
                    break;
                case "POSITION":
                case "文件指向":
                    package = Instructuions_Package.Position;
                    break;
                case "FREAD":
                case "读文件":
                    package = Instructuions_Package.Read_File;
                    break;
                case "FWRITE":
                case "写文件":
                    package = Instructuions_Package.Write_File;
                    break;
                case "FCOPY":
                case "拷贝文件":
                    package = Instructuions_Package.Copy_File;
                    break;
                case "END":
                case "本地结束":
                    package = Instructuions_Package.END;
                    break;
                case "PINIT":
                case "启动进程":
                    package = Instructuions_Package.Init_Process;
                    break;
                case "PEND":
                case "结束进程":
                    package = Instructuions_Package.End_Process;
                    break;
                case "PSWITCH":
                case "切换进程":
                    package = Instructuions_Package.Switch_Process;
                    break;
                case "LOAD":
                case "加载文件":
                    package = Instructuions_Package.Init_Thread;
                    break;
                case "UNLOAD":
                case "结束加载":
                    package = Instructuions_Package.End_Thread;
                    break;
                case "CALLO":
                case "外部调用":
                    package = Instructuions_Package.Outside_Call;
                    break;
                case "CALL":
                case "本地调用":
                    package = Instructuions_Package.Local_Call;
                    break;
                case "RET":
                case "结束调用":
                    package = Instructuions_Package.Ret;
                    break;
                case "JMP":
                case "跳转":
                    package = Instructuions_Package.Jmp;
                    break;
                case "JNS":
                case "正跳转":
                    package = Instructuions_Package.JNS;
                    break;
                case "JS":
                case "负跳转":
                    package = Instructuions_Package.JS;
                    break;
                case "JZ":
                case "零跳转":
                    package = Instructuions_Package.JZ;
                    break;
                case "PUSH":
                case "入栈":
                    package = Instructuions_Package.PUSH;
                    break;
                case "POP":
                case "出栈":
                    package = Instructuions_Package.POP;
                    break;
                case "RAND":
                case "随机数":
                    package = Instructuions_Package.Rand;
                    break;
                case "BPREAD":
                case "基础缓冲读":
                    package = Instructuions_Package.Read_BasePool;
                    break;
                case "BPWRITE":
                case "基础缓冲写":
                    package = Instructuions_Package.Write_BasePool;
                    break;
                case "SPREAD":
                case "专用缓冲读":
                    package = Instructuions_Package.Read_SpecialPool;
                    break;
                case "SPWRITE":
                case "专用缓冲写":
                    package = Instructuions_Package.Write_SpecialPool;
                    break;
                default:
                    if (ToFileLoad.CheckCalculateString(str))
                        package = Instructuions_Package.Calculate;
                    else
                        throw new Exception("错误的 关键字 " + str);
                    break;
            }

            return package;
        }

        #region 指令检查函数段

        /// <summary>
        /// 指令错误检查
        /// </summary>
        /// <param name="instructuion">指令名</param>
        /// <param name="parameters">参数列表</param>
        /// <returns></returns>
        private bool CheckInstructuion(Instructuions_Package instructuion, ParameterPackage[] parameters)
        {
            bool result = false;

            switch (instructuion)
            {
                case Instructuions_Package.END:
                case Instructuions_Package.NOP:
                case Instructuions_Package.Ret:
                case Instructuions_Package.Sync_Time:
                case Instructuions_Package.End_Process:
                case Instructuions_Package.Switch_Process:
                    result = true;
                    break;
                case Instructuions_Package.Sleep:
                case Instructuions_Package.Set_Memory:
                    result = CheckInstructuion_SetMemory(parameters);
                    break;
                case Instructuions_Package.Jmp:
                case Instructuions_Package.JNS:
                case Instructuions_Package.JS:
                case Instructuions_Package.JZ:
                    result = CheckInstructuion_JmpGroup(parameters);
                    break;
                case Instructuions_Package.PUSH:
                case Instructuions_Package.POP:
                    result = CheckInstructuion_StackGroup(parameters);
                    break;
                case Instructuions_Package.Calculate:
                    result = CheckInstructuion_Calculate(parameters);
                    break;
                case Instructuions_Package.Rand:
                    result = CheckInstructuion_Rand(parameters);
                    break;
                case Instructuions_Package.Scan:
                    result = CheckInstructuion_Scan(parameters);
                    break;
                case Instructuions_Package.Print:
                    result = CheckInstructuion_Print(parameters);
                    break;
                case Instructuions_Package.Putc:
                    result = CheckInstructuion_Putc(parameters);
                    break;
                case Instructuions_Package.Connect:
                    result = CheckInstructuion_Connect(parameters);
                    break;
                case Instructuions_Package.Send:
                    result = CheckInstructuion_Send(parameters);
                    break;
                case Instructuions_Package.Receive:
                    result = CheckInstructuion_Receive(parameters);
                    break;
                case Instructuions_Package.GetPort:
                    result = CheckInstructuion_GetPort(parameters);
                    break;
                case Instructuions_Package.Create:
                    result = CheckInstructuion_Create(parameters);
                    break;
                case Instructuions_Package.Delete:
                    result = CheckInstructuion_Delete(parameters);
                    break;
                case Instructuions_Package.Position:
                    result = CheckInstructuion_Position(parameters);
                    break;
                case Instructuions_Package.New_Memory:
                    result = CheckInstructuion_NewMemory(parameters);
                    break;
                case Instructuions_Package.Free_Memory:
                    result = CheckInstructuion_FreeMemory(parameters);
                    break;
                case Instructuions_Package.Init_Memory:
                    result = CheckInstructuion_InitMemory(parameters);
                    break;
                case Instructuions_Package.Read_Time:
                    result = CheckInstructuion_ReadTime(parameters);
                    break;
                case Instructuions_Package.Write_Time:
                    result = CheckInstructuion_WriteTime(parameters);
                    break;
                case Instructuions_Package.Act_Device:
                case Instructuions_Package.Pause_Device:
                case Instructuions_Package.Reset_Device:
                case Instructuions_Package.Close_Device:
                    result = CheckInstructuion_DeviceControlGroup(parameters);
                    break;
                case Instructuions_Package.Read_Device:
                    result = CheckInstructuion_ReadDevice(parameters);
                    break;
                case Instructuions_Package.Write_Device:
                    result = CheckInstructuion_WriteDevice(parameters);
                    break;
                case Instructuions_Package.Pause_Net:
                case Instructuions_Package.Reset_Net:
                case Instructuions_Package.Close_Net:
                    result = CheckInstructuion_NetControlGroup(parameters);
                    break;
                case Instructuions_Package.Open_File:
                    result = CheckInstructuion_OpenFile(parameters);
                    break;
                case Instructuions_Package.Close_File:
                    result = CheckInstructuion_CloseFile(parameters);
                    break;
                case Instructuions_Package.Read_File:
                    result = CheckInstructuion_ReadFile(parameters);
                    break;
                case Instructuions_Package.Write_File:
                    result = CheckInstructuion_WriteFile(parameters);
                    break;
                case Instructuions_Package.Copy_File:
                    result = CheckInstructuion_CopyFile(parameters);
                    break;
                case Instructuions_Package.Init_Process:
                    result = CheckInstructuion_InitProcess(parameters);
                    break;
                case Instructuions_Package.Init_Thread:
                    result = CheckInstructuion_InitThread(parameters);
                    break;
                case Instructuions_Package.End_Thread:
                    result = CheckInstructuion_EndThread(parameters);
                    break;
                case Instructuions_Package.Outside_Call:
                    result = CheckInstructuion_OutSideCall(parameters);
                    break;
                case Instructuions_Package.Local_Call:
                    result = CheckInstructuion_LocalCall(parameters);
                    break;
                case Instructuions_Package.Set_TimeSlice:
                    result = CheckInstructuion_SetTimeSlice(parameters);
                    break;
                case Instructuions_Package.Get_TimeSlice:
                    result = CheckInstructuion_GetTimeSlice(parameters);
                    break;
                case Instructuions_Package.Get_Core:
                    result = CheckInstructuion_GetCore(parameters);
                    break;

            }

            return result;
        }

        #region 指令检查子函数段

        private bool CheckInstructuion_Position(ParameterPackage[] parameters)
        {
            bool result = false;
            if (null != parameters)
                if (2 == parameters.Length)
                    if (Parameter_Type.STR == parameters[0].Type)
                        if (Parameter_Type.Expression == parameters[1].Type || Parameter_Type.BigNumber == parameters[1].Type || Parameter_Type.Long == parameters[1].Type)
                            result = true;
            return result;
        }
        private bool CheckInstructuion_NewMemory(ParameterPackage[] parameters)
        {
            bool result = false;
            if (null != parameters && 2 == parameters.Length)
                if (Parameter_Type.Expression == parameters[0].Type || Parameter_Type.BigNumber == parameters[0].Type || Parameter_Type.Long == parameters[0].Type)
                    if (Parameter_Type.Expression == parameters[1].Type || Parameter_Type.BigNumber == parameters[1].Type || Parameter_Type.Long == parameters[1].Type)
                        result = true;
            return result;
        }
        private bool CheckInstructuion_FreeMemory(ParameterPackage[] parameters)
        {
            bool result = false;
            if (null != parameters && 1 == parameters.Length)
                if (Parameter_Type.Expression == parameters[0].Type || Parameter_Type.BigNumber == parameters[0].Type || Parameter_Type.Long == parameters[0].Type)
                    result = true;
            return result;
        }
        private bool CheckInstructuion_InitMemory(ParameterPackage[] parameters)
        {
            bool result = false;
            if (null != parameters && 2 <= parameters.Length)
                if (null != parameters && 4 == parameters.Length)
                    if (Parameter_Type.Expression == parameters[0].Type || Parameter_Type.BigNumber == parameters[0].Type || Parameter_Type.Long == parameters[0].Type)
                        for (int i = 1; i < parameters.Length; i++)
                            if (Parameter_Type.BigNumber == parameters[i].Type || Parameter_Type.Long == parameters[i].Type)
                                result = true;
                            else
                                break;
            return result;
        }
        private bool CheckInstructuion_ReadTime(ParameterPackage[] parameters)
        {
            bool result = false;
            if (null != parameters && 2 == parameters.Length)
                if (Parameter_Type.Expression == parameters[0].Type || Parameter_Type.BigNumber == parameters[0].Type || Parameter_Type.Long == parameters[0].Type)
                    if (Parameter_Type.STR == parameters[1].Type || Parameter_Type.Expression == parameters[1].Type || Parameter_Type.BigNumber == parameters[1].Type || Parameter_Type.Long == parameters[1].Type)
                        result = true;
            return result;
        }
        private bool CheckInstructuion_WriteTime(ParameterPackage[] parameters)
        {
            bool result = false;
            if (null != parameters && 2 == parameters.Length)
                if (Parameter_Type.Expression == parameters[0].Type || Parameter_Type.BigNumber == parameters[0].Type || Parameter_Type.Long == parameters[0].Type)
                    if (Parameter_Type.STR == parameters[1].Type || Parameter_Type.Expression == parameters[1].Type || Parameter_Type.BigNumber == parameters[1].Type || Parameter_Type.Long == parameters[1].Type)
                        result = true;
            return result;
        }
        private bool CheckInstructuion_DeviceControlGroup(ParameterPackage[] parameters)
        {
            bool result = false;
            if (null != parameters && 1 == parameters.Length)
                if (Parameter_Type.Expression == parameters[0].Type || Parameter_Type.BigNumber == parameters[0].Type || Parameter_Type.Long == parameters[0].Type)
                    result = true;
            return result;
        }
        private bool CheckInstructuion_ReadDevice(ParameterPackage[] parameters)
        {
            bool result = false;
            if (null != parameters && 2 == parameters.Length)
                if (Parameter_Type.Expression == parameters[0].Type || Parameter_Type.BigNumber == parameters[0].Type || Parameter_Type.Long == parameters[0].Type)
                    if (Parameter_Type.Expression == parameters[1].Type || Parameter_Type.BigNumber == parameters[1].Type || Parameter_Type.Long == parameters[1].Type)
                        result = true;
            return result;
        }
        private bool CheckInstructuion_WriteDevice(ParameterPackage[] parameters)
        {
            bool result = false;
            if (null != parameters)
                if (3 == parameters.Length)
                {
                    if (Parameter_Type.Expression == parameters[0].Type || Parameter_Type.BigNumber == parameters[0].Type || Parameter_Type.Long == parameters[0].Type)
                        if (Parameter_Type.Expression == parameters[1].Type || Parameter_Type.BigNumber == parameters[1].Type || Parameter_Type.Long == parameters[1].Type)
                            if (Parameter_Type.Expression == parameters[2].Type || Parameter_Type.BigNumber == parameters[2].Type || Parameter_Type.Long == parameters[2].Type)
                                result = true;
                }
                else if (2 == parameters.Length)
                {
                    if (Parameter_Type.Expression == parameters[0].Type || Parameter_Type.BigNumber == parameters[0].Type || Parameter_Type.Long == parameters[0].Type)
                        if (Parameter_Type.STR == parameters[1].Type)
                            result = true;
                }
            return result;
        }
        private bool CheckInstructuion_NetControlGroup(ParameterPackage[] parameters)
        {
            bool result = false;
            if (null != parameters && 1 == parameters.Length)
                if (Parameter_Type.STR == parameters[0].Type)
                    result = true;
            return result;
        }
        private bool CheckInstructuion_OpenFile(ParameterPackage[] parameters)
        {
            bool result = false;
            if (null != parameters)
                if (3 == parameters.Length)
                {
                    if (Parameter_Type.STR == parameters[0].Type)
                        if (Parameter_Type.STR == parameters[1].Type)
                            if (Parameter_Type.STR == parameters[2].Type || Parameter_Type.Expression == parameters[2].Type || Parameter_Type.BigNumber == parameters[2].Type || Parameter_Type.Long == parameters[2].Type)
                                result = true;
                }
                else if (3 == parameters.Length)
                {
                    if (Parameter_Type.STR == parameters[0].Type)
                        if (Parameter_Type.Expression == parameters[1].Type || Parameter_Type.BigNumber == parameters[1].Type || Parameter_Type.Long == parameters[1].Type)
                            if (Parameter_Type.Expression == parameters[2].Type || Parameter_Type.BigNumber == parameters[2].Type || Parameter_Type.Long == parameters[2].Type)
                                if (Parameter_Type.STR == parameters[3].Type || Parameter_Type.BigNumber == parameters[3].Type || Parameter_Type.Long == parameters[3].Type)
                                    result = true;
                }
            return result;
        }
        private bool CheckInstructuion_CloseFile(ParameterPackage[] parameters)
        {
            bool result = false;
            if (null != parameters && 1 == parameters.Length)
                if (Parameter_Type.STR == parameters[0].Type)
                    result = true;
            return result;
        }
        private bool CheckInstructuion_ReadFile(ParameterPackage[] parameters)
        {
            bool result = false;
            if (null != parameters && 3 == parameters.Length)
                if (Parameter_Type.STR == parameters[0].Type)
                    if (Parameter_Type.Expression == parameters[1].Type || Parameter_Type.BigNumber == parameters[1].Type || Parameter_Type.Long == parameters[1].Type)
                        if (Parameter_Type.Expression == parameters[2].Type || Parameter_Type.BigNumber == parameters[2].Type || Parameter_Type.Long == parameters[2].Type)
                            result = true;
            return result;
        }
        private bool CheckInstructuion_WriteFile(ParameterPackage[] parameters)
        {
            bool result = false;
            if (null != parameters)
                if (3 == parameters.Length)
                {
                    if (Parameter_Type.STR == parameters[0].Type)
                        if (Parameter_Type.Expression == parameters[1].Type || Parameter_Type.BigNumber == parameters[1].Type || Parameter_Type.Long == parameters[1].Type)
                            if (Parameter_Type.Expression == parameters[2].Type || Parameter_Type.BigNumber == parameters[2].Type || Parameter_Type.Long == parameters[2].Type)
                                result = true;
                }
                else if (4 == parameters.Length)
                {
                    if (Parameter_Type.STR == parameters[0].Type)
                        if (Parameter_Type.Expression == parameters[1].Type || Parameter_Type.BigNumber == parameters[1].Type || Parameter_Type.Long == parameters[1].Type)
                            if (Parameter_Type.Expression == parameters[2].Type || Parameter_Type.BigNumber == parameters[2].Type || Parameter_Type.Long == parameters[2].Type)
                                if (Parameter_Type.STR == parameters[3].Type || Parameter_Type.Expression == parameters[3].Type || Parameter_Type.BigNumber == parameters[3].Type || Parameter_Type.Long == parameters[3].Type)
                                    result = true;
                }
            return result;
        }
        private bool CheckInstructuion_CopyFile(ParameterPackage[] parameters)
        {
            bool result = false;
            if (null != parameters)
                if (2 == parameters.Length)
                {
                    if (Parameter_Type.STR == parameters[0].Type)
                        if (Parameter_Type.STR == parameters[1].Type)
                            result = true;
                }
                else if (4 == parameters.Length)
                {
                    if (Parameter_Type.Expression == parameters[0].Type || Parameter_Type.BigNumber == parameters[0].Type || Parameter_Type.Long == parameters[0].Type)
                        if (Parameter_Type.Expression == parameters[1].Type || Parameter_Type.BigNumber == parameters[1].Type || Parameter_Type.Long == parameters[1].Type)
                            if (Parameter_Type.Expression == parameters[2].Type || Parameter_Type.BigNumber == parameters[2].Type || Parameter_Type.Long == parameters[2].Type)
                                if (Parameter_Type.Expression == parameters[3].Type || Parameter_Type.BigNumber == parameters[3].Type || Parameter_Type.Long == parameters[3].Type)
                                    result = true;
                }
            return result;
        }
        private bool CheckInstructuion_SetMemory(ParameterPackage[] parameters)
        {
            bool result = false;
            if (null != parameters && 1 == parameters.Length)
                if (Parameter_Type.BigNumber == parameters[0].Type || Parameter_Type.Long == parameters[0].Type)
                    result = true;
            return result;
        }
        private bool CheckInstructuion_JmpGroup(ParameterPackage[] parameters)
        {
            bool result = false;
            if (null != parameters && 1 == parameters.Length)
                if (Parameter_Type.STR == parameters[0].Type)
                    result = true;
            return result;
        }
        private bool CheckInstructuion_StackGroup(ParameterPackage[] parameters)
        {
            bool result = false;
            if (null != parameters && 1 == parameters.Length)
                if (Parameter_Type.Expression == parameters[0].Type || Parameter_Type.BigNumber == parameters[0].Type || Parameter_Type.Long == parameters[0].Type)
                    result = true;
            return result;
        }
        private bool CheckInstructuion_Calculate(ParameterPackage[] parameters)
        {
            bool result = false;
            if (null != parameters && 2 <= parameters.Length)
                if (Parameter_Type.STR == parameters[0].Type)
                    for (int i = 1; i < parameters.Length; i++)
                        if (Parameter_Type.Expression == parameters[i].Type)
                            result = true;
                        else if (Parameter_Type.BigNumber == parameters[i].Type || Parameter_Type.Long == parameters[i].Type)
                            result = true;
                        else
                            break;
            return result;
        }
        private bool CheckInstructuion_Rand(ParameterPackage[] parameters)
        {
            bool result = false;
            if (null != parameters && (2 == parameters.Length || 3 == parameters.Length))
                for (int i = 0; i < parameters.Length; i++)
                    if (Parameter_Type.Expression == parameters[i].Type)
                        result = true;
                    else if (Parameter_Type.BigNumber == parameters[i].Type || Parameter_Type.Long == parameters[i].Type)
                        result = true;
                    else
                        break;
            return result;
        }
        private bool CheckInstructuion_Scan(ParameterPackage[] parameters)
        {
            bool result = false;
            if (null != parameters && 2 == parameters.Length)
                if (Parameter_Type.Expression == parameters[0].Type || Parameter_Type.BigNumber == parameters[0].Type || Parameter_Type.Long == parameters[0].Type)
                    if (Parameter_Type.Expression == parameters[1].Type || Parameter_Type.BigNumber == parameters[1].Type || Parameter_Type.Long == parameters[1].Type)
                        result = true;
            return result;
        }
        private bool CheckInstructuion_Print(ParameterPackage[] parameters)
        {
            bool result = false;
            if (null != parameters)
                if (1 == parameters.Length)
                {
                    if (Parameter_Type.STR == parameters[0].Type)
                        result = true;
                }
                else if (2 == parameters.Length)
                {
                    if (Parameter_Type.Expression == parameters[0].Type || Parameter_Type.BigNumber == parameters[0].Type || Parameter_Type.Long == parameters[0].Type)
                        if (Parameter_Type.Expression == parameters[1].Type || Parameter_Type.BigNumber == parameters[1].Type || Parameter_Type.Long == parameters[1].Type)
                            result = true;
                }
            return result;
        }
        private bool CheckInstructuion_Putc(ParameterPackage[] parameters)
        {
            bool result = false;
            if (null != parameters && 1 == parameters.Length)
                if (Parameter_Type.Expression == parameters[0].Type || Parameter_Type.BigNumber == parameters[0].Type || Parameter_Type.Long == parameters[0].Type)
                    result = true;
            return result;
        }
        private bool CheckInstructuion_Connect(ParameterPackage[] parameters)
        {
            bool result = false;
            if (null != parameters && 3 <= parameters.Length)
                if (Parameter_Type.STR == parameters[0].Type)
                    if (Parameter_Type.STR == parameters[1].Type || Parameter_Type.Expression == parameters[1].Type)
                        if (Parameter_Type.Expression == parameters[2].Type || Parameter_Type.BigNumber == parameters[2].Type || Parameter_Type.Long == parameters[2].Type)
                            for (int i = 3; i < parameters.Length; i++)
                                if (Parameter_Type.Expression == parameters[i].Type)
                                    result = true;
                                else if (Parameter_Type.STR == parameters[i].Type)
                                    result = true;
                                else
                                    break;
            return result;
        }
        private bool CheckInstructuion_Send(ParameterPackage[] parameters)
        {
            bool result = false;
            if (null != parameters)
                if (2 == parameters.Length)
                {
                    if (Parameter_Type.STR == parameters[0].Type)
                        if (Parameter_Type.STR == parameters[1].Type)
                            result = true;
                }
                else if (3 == parameters.Length)
                {
                    if (Parameter_Type.STR == parameters[0].Type)
                        if (Parameter_Type.Expression == parameters[1].Type || Parameter_Type.BigNumber == parameters[1].Type || Parameter_Type.Long == parameters[1].Type)
                            if (Parameter_Type.Expression == parameters[2].Type || Parameter_Type.BigNumber == parameters[2].Type || Parameter_Type.Long == parameters[2].Type)
                                result = true;
                }
            return result;
        }
        private bool CheckInstructuion_Receive(ParameterPackage[] parameters)
        {
            bool result = false;
            if (null != parameters && 2 == parameters.Length)
                if (Parameter_Type.STR == parameters[0].Type)
                    if (Parameter_Type.Expression == parameters[1].Type || Parameter_Type.BigNumber == parameters[1].Type || Parameter_Type.Long == parameters[1].Type)
                        result = true;
            return result;
        }
        private bool CheckInstructuion_GetPort(ParameterPackage[] parameters)
        {
            bool result = false;
            if (null != parameters && 3 == parameters.Length)
                if (Parameter_Type.STR == parameters[0].Type)
                    if (Parameter_Type.STR == parameters[1].Type || Parameter_Type.Expression == parameters[1].Type || Parameter_Type.BigNumber == parameters[1].Type || Parameter_Type.Long == parameters[1].Type)
                        if (Parameter_Type.Expression == parameters[2].Type || Parameter_Type.BigNumber == parameters[2].Type || Parameter_Type.Long == parameters[2].Type)
                            result = true;
            return result;
        }
        private bool CheckInstructuion_Create(ParameterPackage[] parameters)
        {
            bool result = false;
            if (null != parameters)
                if (1 == parameters.Length)
                {
                    if (Parameter_Type.STR == parameters[0].Type)
                        result = true;
                }
                else if (2 == parameters.Length)
                {
                    if (Parameter_Type.STR == parameters[0].Type)
                        if (Parameter_Type.STR == parameters[1].Type || Parameter_Type.Expression == parameters[1].Type || Parameter_Type.BigNumber == parameters[1].Type || Parameter_Type.Long == parameters[1].Type)
                            result = true;
                }
                else if (3 == parameters.Length)
                {
                    if (Parameter_Type.Expression == parameters[0].Type || Parameter_Type.BigNumber == parameters[0].Type || Parameter_Type.Long == parameters[0].Type)
                        if (Parameter_Type.Expression == parameters[1].Type || Parameter_Type.BigNumber == parameters[1].Type || Parameter_Type.Long == parameters[1].Type)
                            if (Parameter_Type.STR == parameters[2].Type || Parameter_Type.Expression == parameters[2].Type || Parameter_Type.BigNumber == parameters[2].Type || Parameter_Type.Long == parameters[2].Type)
                                result = true;
                }
            return result;
        }
        private bool CheckInstructuion_Delete(ParameterPackage[] parameters)
        {
            bool result = false;
            if (null != parameters)
                if (1 == parameters.Length)
                {
                    if (Parameter_Type.STR == parameters[0].Type)
                        result = true;
                }
                else if (2 == parameters.Length)
                {
                    if (Parameter_Type.STR == parameters[0].Type)
                        if (Parameter_Type.STR == parameters[1].Type || Parameter_Type.Expression == parameters[1].Type || Parameter_Type.BigNumber == parameters[1].Type || Parameter_Type.Long == parameters[1].Type)
                            result = true;
                }
                else if (3 == parameters.Length)
                {
                    if (Parameter_Type.Expression == parameters[0].Type || Parameter_Type.BigNumber == parameters[0].Type || Parameter_Type.Long == parameters[0].Type)
                        if (Parameter_Type.Expression == parameters[1].Type || Parameter_Type.BigNumber == parameters[1].Type || Parameter_Type.Long == parameters[1].Type)
                            if (Parameter_Type.STR == parameters[2].Type || Parameter_Type.Expression == parameters[2].Type || Parameter_Type.BigNumber == parameters[2].Type || Parameter_Type.Long == parameters[2].Type)
                                result = true;
                }
            return result;
        }
        private bool CheckInstructuion_GetCore(ParameterPackage[] parameters)
        {
            bool result = false;
            if (null != parameters)
                if (1 == parameters.Length)
                    if (Parameter_Type.Expression == parameters[0].Type || Parameter_Type.BigNumber == parameters[0].Type || Parameter_Type.Long == parameters[0].Type)
                        result = true;
            return result;
        }
        private bool CheckInstructuion_GetTimeSlice(ParameterPackage[] parameters)
        {
            bool result = false;
            if (null != parameters)
                if (1 == parameters.Length)
                    if (Parameter_Type.Expression == parameters[0].Type || Parameter_Type.BigNumber == parameters[0].Type || Parameter_Type.Long == parameters[0].Type)
                        result = true;
                else if (2 == parameters.Length)
                    if (Parameter_Type.Expression == parameters[0].Type || Parameter_Type.BigNumber == parameters[0].Type || Parameter_Type.Long == parameters[0].Type)
                        if (Parameter_Type.Expression == parameters[1].Type || Parameter_Type.BigNumber == parameters[1].Type || Parameter_Type.Long == parameters[1].Type)
                            result = true;
            return result;
        }
        private bool CheckInstructuion_SetTimeSlice(ParameterPackage[] parameters)
        {
            bool result = false;
            if (null != parameters)
                if (1 == parameters.Length)
                    if (Parameter_Type.Expression == parameters[0].Type || Parameter_Type.BigNumber == parameters[0].Type || Parameter_Type.Long == parameters[0].Type)
                        result = true;
                else if (2 == parameters.Length)
                    if (Parameter_Type.Expression == parameters[0].Type || Parameter_Type.BigNumber == parameters[0].Type || Parameter_Type.Long == parameters[0].Type)
                        if (Parameter_Type.Expression == parameters[1].Type || Parameter_Type.BigNumber == parameters[1].Type || Parameter_Type.Long == parameters[1].Type)
                            result = true;
            return result;
        }
        private bool CheckInstructuion_LocalCall(ParameterPackage[] parameters)
        {
            bool result = false;
            if (null != parameters && 1 == parameters.Length)
                if (Parameter_Type.STR == parameters[0].Type)
                    result = true;
            return result;
        }
        private bool CheckInstructuion_OutSideCall(ParameterPackage[] parameters)
        {
            bool result = false;
            if (null != parameters && 2 == parameters.Length)
                if (Parameter_Type.STR == parameters[0].Type)
                    if (Parameter_Type.STR == parameters[1].Type)
                        result = true;
            return result;
        }
        private bool CheckInstructuion_EndThread(ParameterPackage[] parameters)
        {
            bool result = false;
            if (null != parameters && 1 == parameters.Length)
                if (Parameter_Type.STR == parameters[0].Type)
                    result = true;
            return result;
        }
        private bool CheckInstructuion_InitThread(ParameterPackage[] parameters)
        {
            bool result = false;
            if (null != parameters && 2 == parameters.Length)
                if (Parameter_Type.STR == parameters[0].Type)
                    if (Parameter_Type.STR == parameters[1].Type)
                        result = true;
            return result;
        }
        private bool CheckInstructuion_InitProcess(ParameterPackage[] parameters)
        {
            bool result = false;
            if (null != parameters && 1 == parameters.Length)
                if (Parameter_Type.STR == parameters[0].Type)
                    result = true;
            return result;
        }

        #endregion
        #endregion

        #endregion

        #region 算符优先检测文法函数段

        /// <summary>
        /// 运算符检测
        /// </summary>
        /// <param name="op">符号</param>
        /// <returns>返回结果</returns>
        private bool OPSET(char op)
        {
            bool result = false;
            switch (op)
            {
                case '#':
                case '+':
                case '-':
                case '*':
                case '/':
                case '[':
                case ']':
                case '(':
                case ')':
                case ':':
                case '【':
                case '】':
                case '（':
                case '）':
                case '：':
                case '~':
                case '&':
                case '|':
                case '^':
                    result = true;
                    break;
                default:
                    break;
            }
            return result;
        }
        /// <summary>
        /// 算符优先检测
        /// </summary>
        /// <param name="op_ahead">栈顶符号</param>
        /// <param name="op_now">当前符号</param>
        /// <returns>返回优先级</returns>
        private OP_PRI OPPRI(char op_ahead, char op_now)
        {
            // 运算符优先级表   
            /////// '+' '-' '*' '/' '(' ')' '#' '^'   
            //*'+'*/ 1,  1, -1, -1, -1,  1,  1, -1,
            //*'-'*/ 1,  1, -1, -1, -1,  1,  1, -1,
            //*'*'*/ 1,  1,  1,  1, -1,  1,  1, -1,
            //*'/'*/ 1,  1,  1,  1, -1,  1,  1, -1,
            //*'('*/-1, -1, -1, -1, -1,  0, 11, -1,
            //*')'*/ 1,  1,  1,  1, 11,  1,  1,  1,
            //*'#'*/-1, -1, -1, -1, -1, 11,  0, -1,
            //*'^'*/ 1,  1,  1,  1, -1,  1,  1,  1
            OP_PRI pri = OP_PRI.NONE;
            switch (op_ahead)
            {
                case '#':
                    switch (op_now)
                    {
                        case '#':
                            pri = OP_PRI.EQUAL;
                            break;
                        case ')':
                        case ']':
                        case '）':
                        case '】':
                            pri = OP_PRI.NONE;
                            break;
                        case ':':
                        case '：':
                        case '*':
                        case '/':
                        case '+':
                        case '-':
                        case '[':
                        case '(':
                        case '【':
                        case '（':
                        case '~':
                        case '&':
                        case '|':
                        case '^':
                            pri = OP_PRI.LOW;
                            break;
                    }
                    break;
                case '+':
                case '-':
                    switch (op_now)
                    {
                        case '#':
                        case '+':
                        case '-':
                        case ')':
                        case ']':
                        case '）':
                        case '】':
                        case '&':
                        case '|':
                        case '^':
                            pri = OP_PRI.HIGH;
                            break;
                        case '~':
                        case '*':
                        case '/':
                        case ':':
                        case '[':
                        case '(':
                        case '：':
                        case '【':
                        case '（':
                            pri = OP_PRI.LOW;
                            break;
                    }
                    break;
                case '*':
                case '/':
                    switch (op_now)
                    {
                        case '#':
                        case '+':
                        case '-':
                        case '*':
                        case '/':
                        case ':':
                        case ')':
                        case ']':
                        case '：':
                        case '）':
                        case '】':
                        case '&':
                        case '|':
                        case '^':
                            pri = OP_PRI.HIGH;
                            break;
                        case '~':
                        case '[':
                        case '(':
                        case '【':
                        case '（':
                            pri = OP_PRI.LOW;
                            break;
                    }
                    break;
                case '[':
                case '【':
                    switch (op_now)
                    {
                        case '#':
                        case ')':
                        case '）':
                            pri = OP_PRI.NONE;
                            break;
                        case ']':
                        case '】':
                            pri = OP_PRI.EQUAL;
                            break;
                        case '+':
                        case '-':
                        case '*':
                        case '/':
                        case ':':
                        case '[':
                        case '(':
                        case '：':
                        case '【':
                        case '（':
                        case '&':
                        case '|':
                        case '^':
                        case '~':
                            pri = OP_PRI.LOW;
                            break;
                    }
                    break;
                case ']':
                case '】':
                    switch (op_now)
                    {
                        case '#':
                        case '+':
                        case '-':
                        case '*':
                        case '/':
                        case ':':
                        case ')':
                        case '(':
                        case ']':
                        case '[':
                        case '：':
                        case '）':
                        case '（':
                        case '】':
                        case '【':
                        case '&':
                        case '|':
                        case '^':
                        case '~':
                            pri = OP_PRI.NONE;
                            break;
                    }
                    break;
                case '(':
                case '（':
                    switch (op_now)
                    {
                        case ')':
                        case '）':
                            pri = OP_PRI.EQUAL;
                            break;
                        case '#':
                        case ']':
                        case '】':
                            pri = OP_PRI.NONE;
                            break;
                        case '+':
                        case '-':
                        case '*':
                        case '/':
                        case ':':
                        case '[':
                        case '(':
                        case '：':
                        case '【':
                        case '（':
                        case '&':
                        case '|':
                        case '^':
                        case '~':
                            pri = OP_PRI.LOW;
                            break;
                    }
                    break;
                case ')':
                case '）':
                    switch (op_now)
                    {
                        case '#':
                        case '+':
                        case '-':
                        case '*':
                        case '/':
                        case ':':
                        case ')':
                        case ']':
                        case '[':
                        case '(':
                        case '：':
                        case '）':
                        case '】':
                        case '【':
                        case '（':
                        case '&':
                        case '|':
                        case '^':
                        case '~':
                            pri = OP_PRI.NONE;
                            break;
                    }
                    break;
                case ':':
                case '：':
                    switch (op_now)
                    {
                        case '#':
                        case '+':
                        case '-':
                        case '*':
                        case '/':
                        case ')':
                        case ']':
                        case '）':
                        case '】':
                        case '&':
                        case '|':
                        case '^':
                            pri = OP_PRI.HIGH;
                            break;
                        case '~':
                        case ':':
                        case '[':
                        case '(':
                        case '：':
                        case '【':
                        case '（':
                            pri = OP_PRI.LOW;
                            break;
                    }
                    break;
                case '&':
                case '|':
                case '^':
                    switch (op_now)
                    {
                        case '#':
                        case ')':
                        case ']':
                        case '）':
                        case '】':
                        case '&':
                        case '|':
                        case '^':
                            pri = OP_PRI.HIGH;
                            break;
                        case '+':
                        case '-':
                        case '*':
                        case '/':
                        case '~':
                        case ':':
                        case '[':
                        case '(':
                        case '：':
                        case '【':
                        case '（':
                            pri = OP_PRI.LOW;
                            break;
                    }
                    break;
                case '~':
                    switch (op_now)
                    {
                        case ')':
                        case ']':
                        case '）':
                        case '】':
                        case '&':
                        case '|':
                        case '^':
                        case '#':
                        case '+':
                        case '-':
                        case '*':
                        case '/':
                        case '~':
                        case ':':
                        case '：':
                            pri = OP_PRI.HIGH;
                            break;
                        case '[':
                        case '(':
                        case '【':
                        case '（':
                            pri = OP_PRI.HIGH;
                            break;
                    }
                    break;
            }
            return pri;
        }

        #endregion

        #region 内部类/枚举

        /// <summary>
        /// 运算数包
        /// </summary>
        class Node
        {
            /// <summary>
            /// 运算数
            /// </summary>
            public long data;
            /// <summary>
            /// 运算数类型
            /// </summary>
            public bool flag;
        }
        /// <summary>
        /// 算符优先级
        /// </summary>
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
        enum Vocabulary_Type
        {
            /// <summary>
            /// 符号类型
            /// </summary>
            SIGN,
            /// <summary>
            /// 表达式类型
            /// </summary>
            Expression,
            /// <summary>
            /// 立即数类型-大整数
            /// </summary>
            BigNumber,
            /// <summary>
            /// 立即数类型-长整数
            /// </summary>
            Long,
            /// <summary>
            /// 字符串类型
            /// </summary>
            STR,
            /// <summary>
            /// 未知的类型
            /// </summary>
            NONE,
            /// <summary>
            /// 文件读取完毕
            /// </summary>
            END,
        }

        #endregion
    }
}
