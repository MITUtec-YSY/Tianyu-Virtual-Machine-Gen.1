using System;
using System.Collections.Generic;
using System.Text;

namespace PL0_Compiler
{
    class GeneralVocabularyUnit
    {
        public long Line { get; }
        public LexicalLabel LexicalLabel { get; }

        public GeneralVocabularyUnit(LexicalLabel lexicalLabel, long line)
        {
            LexicalLabel = lexicalLabel;
            Line = line;
        }

        virtual public string GetString()
        {
            switch (LexicalLabel)
            {
                case LexicalLabel.PL0_Plus:
                    return "+";
                case LexicalLabel.PL0_Sub:
                    return "-";
                case LexicalLabel.PL0_LBrace:
                    return "(";
                case LexicalLabel.PL0_RBrace:
                    return ")";
                case LexicalLabel.PL0_Mul:
                    return "*";
                case LexicalLabel.PL0_Div:
                    return "/";
                default:
                    return LexicalLabel.ToString();
            }
        }
        virtual public uint GetInt()
        {
            return (uint)LexicalLabel;
        }
    }

    class IdentifierVocabularyUnit : GeneralVocabularyUnit
    {
        private readonly string Identifier;

        public IdentifierVocabularyUnit(LexicalLabel lexicalLabel, string id, long line) : base(lexicalLabel, line)
        {
            Identifier = id;
        }

        public override string GetString()
        {
            return Identifier;
        }
    }

    class UnsignedNumberVocabularyUnit : GeneralVocabularyUnit
    {
        private readonly uint Number;

        public UnsignedNumberVocabularyUnit(LexicalLabel lexicalLabel, uint number, long line) : base(lexicalLabel, line)
        {
            Number = number;
        }

        public override uint GetInt()
        {
            return Number;
        }
    }
}
