using System;
using System.Collections.Generic;
using System.Text;

namespace PL0_Compiler
{
    class ProcedureTable
    {
        private readonly Dictionary<string, SignTree> Items;

        public ProcedureTable(SignTree tree)
        {
            Items = new Dictionary<string, SignTree>()
            {
                { tree.Name, tree }
            };
        }

        public void AddItem(string name, SignTree tree)
        {
            Items.Add(name, tree);
        }
        public bool Contains(string name) => Items.ContainsKey(name);
        public SignTree GetItem(string name)
        {
            if (Items.TryGetValue(name, out SignTree tree))
                return tree;
            throw new KeyNotFoundException(name);
        }
    }

    class SignTree
    {
        public string Name { get; private set; }
        public SignTree Father { get; private set; }

        private readonly string InitiationAddress;
        private int IfIdentifier;
        private int WhileIdentifier;
        private readonly List<SignTree> Children;
        private readonly List<KeyValuePair<string, int>> VarTable;
        private readonly List<KeyValuePair<string, uint>> ConstTable;

        public SignTree(string name, SignTree father, string init_addr)
        {
            Name = name;
            Father = father;
            InitiationAddress = init_addr;
            IfIdentifier = -1;
            WhileIdentifier = -1;
            Children = new List<SignTree>();
            VarTable = new List<KeyValuePair<string, int>>();
            ConstTable = new List<KeyValuePair<string, uint>>();
        }

        public SignTree AddItem(string name)
        {
            int add = VarTable[VarTable.Count - 1].Value + 4;
            SignTree item = new SignTree(name, this, "[" + InitiationAddress + "+" + add.ToString() + "]");
            Children.Add(item);
            return item;
        }
        public SignTree GetItem(string[] name)
        {
            SignTree item = null;

            if (null != name)
            {
                if (0 < name.Length)
                {
                    for (int i = 0; i < Children.Count; i++)
                        if (Children[i].Name == name[0])
                        {
                            string[] temp = new string[name.Length - 1];
                            for (int j = 0; j < temp.Length; j++)
                                temp[j] = name[j + 1];
                            return Children[i].GetItem(temp);
                        }
                }
                else
                {
                    for (int i = 0; i < Children.Count; i++)
                        if (Children[i].Name == name[0])
                            return Children[i];
                }

            }

            return item;
        }

        public void AddVar(string name)
        {
            for (int i = 0; i < VarTable.Count; i++)
                if (VarTable[i].Key == name)
                    throw new SignRepeatException(name);
            VarTable.Add(new KeyValuePair<string, int>(name, VarTable.Count * 4));
        }
        public void AddConst(string name, uint value)
        {
            for (int i = 0; i < ConstTable.Count; i++)
                if (ConstTable[i].Key == name)
                    throw new SignRepeatException(name);
            ConstTable.Add(new KeyValuePair<string, uint>(name, value));
        }

        public string GetVar(string name)
        {
            string str = GetVarAddress(name);
            if (null != str)
                return "[" + str + "]";
            return null;
        }
        public uint GetConst(string name)
        {
            for (int i = 0; i < ConstTable.Count; i++)
                if (name == ConstTable[i].Key)
                    return ConstTable[i].Value;
            if (null != Father)
                return Father.GetConst(name);
            throw new KeyNotFoundException(name);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="flag">true:Var | false:Const</param>
        /// <returns></returns>
        public bool Contains(string name, out bool flag)
        {
            flag = true;
            for (int i = 0; i < VarTable.Count; i++)
                if (name == VarTable[i].Key)
                {
                    flag = true;
                    return true;
                }
            for (int i = 0; i < ConstTable.Count; i++)
                if (name == ConstTable[i].Key)
                {
                    flag = false;
                    return true;
                }
            if (null != Father)
                return Father.Contains(name, out flag);
            return false;
        }

        public string[] GetIfLabel()
        {
            IfIdentifier++;
            return new string[]
            {
                Name + "_If_" + IfIdentifier.ToString() + "_S",
                Name + "_If_" + IfIdentifier.ToString() + "_E"
            };
        }
        public string[] GetWhileLable()
        {
            WhileIdentifier++;
            return new string[]
            {
                Name + "_While_" + WhileIdentifier.ToString() + "_S",
                Name + "_While_" + WhileIdentifier.ToString() + "_E"
            };
        }

        public string GetProcedureAddress()
        {
            return InitiationAddress + "+" + GetMemSize().ToString();
        }
        public string GetVarAddress(string name)
        {
            for (int i = 0; i < VarTable.Count; i++)
                if (name == VarTable[i].Key)
                    return InitiationAddress + "+" + VarTable[i].Value.ToString();
            if (null != Father)
                return Father.GetVarAddress(name);
            return null;
        }

        public int GetMemSize()
        {
            return (VarTable.Count + 1) * 4;
        }

        public static void GetPath(SignTree running, SignTree target, out string[] callback, out string[] callin)
        {
            Stack<string> stackr = new Stack<string>();
            Stack<string> stackt = new Stack<string>();
            ReadPath(running, stackr);
            ReadPath(target, stackt);
            string[] runs = stackr.ToArray();
            string[] tars = stackt.ToArray();
            int index = 0;
            for(int i = 0; i < runs.Length && i < tars.Length; i++)
            {
                if (runs[i] == tars[i])
                    index = i;
                else
                    break;
            }
            callback = new string[runs.Length - index];
            for (int i = 0; i < callback.Length; i++)
                callback[i] = runs[i + index];
            callin = new string[tars.Length - index];
            for (int i = 0; i < callin.Length; i++)
                callin[i] = tars[i + index];
        }
        private static void ReadPath(SignTree tree, Stack<string> stack)
        {
            SignTree temp = tree;
            while (null != temp)
            {
                stack.Push(temp.Name);
                temp = temp.Father;
            }
        }
    }
}
