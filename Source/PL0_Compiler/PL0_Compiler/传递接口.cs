using System;
using System.Collections.Generic;
using System.Text;

namespace PL0_Compiler
{
    interface IComiler
    {
        GeneralVocabularyUnit Peek();
        GeneralVocabularyUnit Read();
        GeneralVocabularyUnit Next();
    }
}
