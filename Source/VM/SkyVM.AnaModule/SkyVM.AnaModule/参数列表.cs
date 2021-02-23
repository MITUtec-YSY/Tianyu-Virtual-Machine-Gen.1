using System;
using System.Collections.Generic;
using System.Text;

namespace SkyVM.AnaModule
{
    /// <summary>
    /// 参数列表
    /// </summary>
    public class ParameterList
    {
        /// <summary>
        /// 获取参数数目
        /// </summary>
        public int Count { get; private set; }

        private readonly ParameterNode Head;
        private ParameterNode End;

        /// <summary>
        /// 参数列表构造函数
        /// </summary>
        public ParameterList()
        {
            Head = new ParameterNode
            {
                Type = Parameter_Type.STR,
                Parameter = null,
                Next = null
            };
            End = Head;
            Count = 0;
        }

        /// <summary>
        /// 添加一个参数
        /// </summary>
        /// <param name="type">参数类型</param>
        /// <param name="para">参数主体</param>
        public void Add(Parameter_Type type, object para)
        {
            End.Next = new ParameterNode
            {
                Type = type,
                Parameter = para,
                Next = null
            };
            End = End.Next;
            Count++;
        }
        /// <summary>
        /// 获取整个参数列表
        /// </summary>
        /// <returns>返回参数列表</returns>
        public ParameterPackage[] GetParameters()
        {
            ParameterPackage[] result = null;
            if (0 < Count)
            {
                result = new ParameterPackage[Count];
                ParameterNode node = Head;
                for(int i = 0; node.Next != null && i < result.Length; i++, node = node.Next)
                    result[i] = new ParameterPackage
                    {
                        Type = node.Next.Type,
                        Parameter = node.Next.Parameter
                    };
            }
            return result;
        }

        /// <summary>
        /// 参数列表参数从传递包
        /// </summary>
        public class ParameterPackage
        {
            /// <summary>
            /// 参数类型
            /// </summary>
            public Parameter_Type Type;
            /// <summary>
            /// 参数主体
            /// </summary>
            public object Parameter;
        }

        class ParameterNode
        {
            public Parameter_Type Type;
            public object Parameter;
            public ParameterNode Next;
        }
    }
}
