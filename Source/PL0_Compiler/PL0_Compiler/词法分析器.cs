using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace PL0_Compiler
{
    class LexAnalysisUnit
    {
        public int Line { get; private set; }

        private readonly StreamReader Reader;
        private readonly Queue<char> SymbolQueue;

        private GeneralVocabularyUnit VocabularyUnit;
        private GeneralVocabularyUnit VocabularyUnitNext;
        private char character;

        public LexAnalysisUnit(StreamReader reader)
        {
            SymbolQueue = new Queue<char>();
            Reader = reader;
            VocabularyUnit = null;
            VocabularyUnitNext = null;
            character = ' ';
            Line = 1;
            Init();   
        }
        private void Init()
        {
            LexicalAnalysis();
            VocabularyUnit = VocabularyUnitNext;
            LexicalAnalysis();
        }

        public GeneralVocabularyUnit Peek
        {
            get
            {
                return VocabularyUnit;
            }
        }
        public GeneralVocabularyUnit Read
        {
            get
            {
                GeneralVocabularyUnit vocabulary = VocabularyUnit;
                VocabularyUnit = VocabularyUnitNext;
                LexicalAnalysis();
                return vocabulary;
            }
        }
        public GeneralVocabularyUnit Next
        {
            get
            {
                return VocabularyUnitNext;
            }
        }

        private void LexicalAnalysis()
        {
            if (!Reader.EndOfStream)
            {
                RemoveSplitSign();
                switch (character)
                {
                    case ',':
                        VocabularyUnitNext = new GeneralVocabularyUnit(LexicalLabel.PL0_Comma, Line);
                        break;
                    case '(':
                        VocabularyUnitNext = new GeneralVocabularyUnit(LexicalLabel.PL0_LBrace, Line);
                        break;
                    case ')':
                        VocabularyUnitNext = new GeneralVocabularyUnit(LexicalLabel.PL0_RBrace, Line);
                        break;
                    case '.':
                        VocabularyUnitNext = new GeneralVocabularyUnit(LexicalLabel.PL0_Dot, Line);
                        break;
                    case ';':
                        VocabularyUnitNext = new GeneralVocabularyUnit(LexicalLabel.PL0_Semicolon, Line);
                        break;
                    case '+':
                        VocabularyUnitNext = new GeneralVocabularyUnit(LexicalLabel.PL0_Plus, Line);
                        break;
                    case '-':
                        VocabularyUnitNext = new GeneralVocabularyUnit(LexicalLabel.PL0_Sub, Line);
                        break;
                    case '*':
                        VocabularyUnitNext = new GeneralVocabularyUnit(LexicalLabel.PL0_Mul, Line);
                        break;
                    case '/':
                        VocabularyUnitNext = new GeneralVocabularyUnit(LexicalLabel.PL0_Div, Line);
                        break;
                    case '#':
                        VocabularyUnitNext = new GeneralVocabularyUnit(LexicalLabel.PL0_Nequal, Line);
                        break;
                    case '=':
                        VocabularyUnitNext = new GeneralVocabularyUnit(LexicalLabel.PL0_Equal, Line);
                        break;
                    case '<':
                        if ('=' == (char)Reader.Peek())
                        {
                            Reader.Read();
                            VocabularyUnitNext = new GeneralVocabularyUnit(LexicalLabel.PL0_LessEqual, Line);
                        }
                        else
                            VocabularyUnitNext = new GeneralVocabularyUnit(LexicalLabel.PL0_Less, Line);
                        break;
                    case '>':
                        if ('=' == (char)Reader.Peek())
                        {
                            Reader.Read();
                            VocabularyUnitNext = new GeneralVocabularyUnit(LexicalLabel.PL0_GreaterQeual, Line);
                        }
                        else
                            VocabularyUnitNext = new GeneralVocabularyUnit(LexicalLabel.PL0_Greater, Line);
                        break;
                    case ':':
                        if ('=' == (char)Reader.Peek())
                        {
                            Reader.Read();
                            VocabularyUnitNext = new GeneralVocabularyUnit(LexicalLabel.PL0_Become, Line);
                        }
                        else
                            throw new LexAnalysisException(Line, "‘:’、‘" + (char)Reader.Peek() + "’");
                        break;
                    default:
                        ReadStr();
                        break;
                }
            }
            else
                VocabularyUnitNext = null;
        }

        private void ReadStr()
        {
            if ('a' <= character && 'z' >= character || 'A' <= character && 'Z' >= character)
                ReadSignAsID();
            else if ('0' <= character && '9' >= character)
                ReadSignAsUInt();
            else if ('\uffff' == character)
            {
                VocabularyUnitNext = null;
                return;
            }
            else
                throw new LexAnalysisException(Line, "‘" + character.ToString() + "’");
        }
        private void ReadSignAsID()
        {
            SymbolQueue.Enqueue(character);
            while (true)
            {
                if ('a' <= (char)Reader.Peek() && 'z' >= (char)Reader.Peek() || 'A' <= (char)Reader.Peek() && 'Z' >= (char)Reader.Peek())
                    SymbolQueue.Enqueue((char)Reader.Peek());
                else if ('0' <= (char)Reader.Peek() && '9' >= (char)Reader.Peek())
                    SymbolQueue.Enqueue((char)Reader.Peek());
                else
                {
                    LexicalLabel label = GetLexicalLabel(new string(SymbolQueue.ToArray()));
                    if (LexicalLabel.PL0_ID == label)
                        VocabularyUnitNext = new IdentifierVocabularyUnit(label, new string(SymbolQueue.ToArray()), Line);
                    else
                        VocabularyUnitNext = new GeneralVocabularyUnit(label, Line);
                    SymbolQueue.Clear();
                    return;
                }
                character = (char)Reader.Read();
            }
        }
        private void ReadSignAsUInt()
        {
            SymbolQueue.Enqueue(character);
            while (true)
            {
                if ('0' <= (char)Reader.Peek() && '9' >= (char)Reader.Peek())
                    SymbolQueue.Enqueue((char)Reader.Peek());
                else
                {
                    VocabularyUnitNext = new UnsignedNumberVocabularyUnit(LexicalLabel.PL0_Number, FormatNumber(new string(SymbolQueue.ToArray())), Line);
                    SymbolQueue.Clear();
                    return;
                }
                character = (char)Reader.Read();
            }
        }
        private void RemoveSplitSign()
        {
            while (true)
            {
                character = (char)Reader.Read();
                switch (character)
                {
                    case ' ':
                    case '\t':
                    case '\r':
                        break;
                    case '\n':
                        Line++;
                        break;
                    default:
                        return;
                }
            }
                
        }

        private LexicalLabel GetLexicalLabel(string vocabulary)
        {
            switch (vocabulary.ToLower())
            {
                case "const":
                    return LexicalLabel.PL0_ConstSym;
                case "var":
                    return LexicalLabel.PL0_VarSym;
                case "procedure":
                    return LexicalLabel.PL0_ProcedureSym;
                case "begin":
                    return LexicalLabel.PL0_BeginSym;
                case "end":
                    return LexicalLabel.PL0_EndSym;
                case "odd":
                    return LexicalLabel.PL0_OddSym;
                case "if":
                    return LexicalLabel.PL0_IfSym;
                case "then":
                    return LexicalLabel.PL0_ThenSym;
                case "call":
                    return LexicalLabel.PL0_CallSym;
                case "while":
                    return LexicalLabel.PL0_WhileSym;
                case "do":
                    return LexicalLabel.PL0_DoSym;
                case "read":
                    return LexicalLabel.PL0_ReadSym;
                case "write":
                    return LexicalLabel.PL0_WriteSym;
                default:
                    return LexicalLabel.PL0_ID;
            }
        }
        private uint FormatNumber(string num)
        {
            if (num is null)
                return 0;
            else
            {
                uint temp = 0;
                foreach (char i in num)
                    temp = (uint)(temp * 10 + (i - 48));
                return temp;
            }
        }
    }
}
