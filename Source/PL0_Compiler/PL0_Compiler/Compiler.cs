using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace PL0_Compiler
{
    public class Compiler : IComiler
    {
        private readonly string ReaderPath;
        private readonly string WriterPath;
        private LexAnalysisUnit LexAnalysis;

        public Compiler(string path, string wpath = null)
        {
            try
            {
                FileInfo info = new FileInfo(path);
                if (info.Exists)
                {
                    if (null == wpath)
                        WriterPath = info.FullName.TrimEnd(info.Extension.ToCharArray()) + ".tyh";
                    else
                        WriterPath = wpath;
                    ReaderPath = path;
                }
                else
                    throw new Exception("文件：" + path + " 无法访问的路径或文件不存在");
            }
            catch(Exception e)
            {
                throw e;
            }
        }

        public void LexAnalys()
        {
            StreamReader reader = new StreamReader(ReaderPath);
            try
            {
                LexAnalysisUnit LexAnalysis = new LexAnalysisUnit(reader);
                Console.WriteLine(string.Format("{0,-10}\t{1,-10}\t{2,-8}\t{3,-8}",
                        "符号类型", "符号名", "符号值", "行号"));
                while (null != LexAnalysis.Peek)
                {
                    GeneralVocabularyUnit unit = LexAnalysis.Read;
                    Console.WriteLine(string.Format("{0,-10}\t{1,-15}\t{2,-8}\t{3,-8}",
                        unit.LexicalLabel.ToString(), unit.GetString(), unit.GetInt(), unit.Line));
                }
            }
            catch (Exception e)
            {
                throw e;
            }
            finally
            {
                reader.Close();
            }
        }
        public void Compile()
        {
            StreamWriter writer = new StreamWriter(WriterPath);
            StreamReader reader = new StreamReader(ReaderPath);
            try
            {
                LexAnalysis = new LexAnalysisUnit(reader);
                SyntaxAnalysisUnit syntax = new SyntaxAnalysisUnit(writer, this);
                syntax.SyntaxAnalysis();
            }
            catch (Exception e)
            {
                throw e;
            }
            finally
            {
                writer.Close();
                reader.Close();
            }
        }

        GeneralVocabularyUnit IComiler.Peek() => LexAnalysis.Peek;
        GeneralVocabularyUnit IComiler.Read() => LexAnalysis.Read;
        GeneralVocabularyUnit IComiler.Next() => LexAnalysis.Next;
    }
}
