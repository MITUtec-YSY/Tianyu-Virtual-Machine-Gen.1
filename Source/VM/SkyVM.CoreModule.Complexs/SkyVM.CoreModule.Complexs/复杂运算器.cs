using SkyVM.InterfaceModule;
using System;


namespace SkyVM.CoreModule.Complexs
{
    public class ComplexAU : IComplexOperation
    {
        public override bool OperateTest(string operation)
        {
            return true;
        }

        protected override byte[] Calculate(out Symbol_Flag flag, out bool overflow, string operation, CalculateParameter[] parameters)
        {
            byte[] result = new byte[0];
            flag = Symbol_Flag.Zero;
            overflow = false;
            return result;
        }
    }
}
