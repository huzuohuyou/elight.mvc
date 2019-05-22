using PostSharp.Aspects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestingCenterSystem.Utils.Attribute
{
    [Serializable]
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    public class ExceptionAttribute : MethodInterceptionAspect
    {
        //调用本函数时截取异常
        public override void OnInvoke(MethodInterceptionArgs args)
        {
            try
            {
                base.OnInvoke(args);
            }
            catch (Exception ex)
            {
                Console.WriteLine(string.Format("此方法异常信息是：{0}", ex.ToString()));
            }
        }
    }
}
