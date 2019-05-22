using PostSharp.Aspects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace TestingCenterSystem.Utils.Attribute
{
    [Serializable]
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    public class LogsAttribute : OnMethodBoundaryAspect
    {
        /// <summary>
        /// 入口参数信息
        /// </summary>
        public string EntryText { get; set; }

        /// <summary>
        /// 出口参数信息
        /// </summary>
        public string ExitText { get; set; }

        /// <summary>
        /// 异常信息
        /// </summary>
        public string ExceptionText { get; set; }

        //进入方法时输出方法的输入参数
        public override void OnEntry(MethodExecutionArgs eventArgs)
        {
            Arguments arguments = eventArgs.Arguments;
            StringBuilder sb = new StringBuilder();
            ParameterInfo[] parameters = eventArgs.Method.GetParameters();
            for (int i = 0; arguments != null && i < arguments.Count; i++)
            {
                //进入的参数的值
                sb.Append(parameters[i].Name + "=" + arguments[i] + " ");
            }
            string message = string.Format("{0}.{1} Method. The Entry Arg Is：{2}",
                eventArgs.Method.DeclaringType.FullName, eventArgs.Method.Name, sb.ToString());
            Console.WriteLine(message);
        }

        //退出方法时的方法返回值
        public override void OnExit(MethodExecutionArgs eventArgs)
        {
            Console.WriteLine(string.Format("{0}.{1} Method. The Result Is：{2}",
                eventArgs.Method.DeclaringType.FullName, eventArgs.Method.Name, eventArgs.ReturnValue.ToString()));
        }

        //方法发生异常时记录异常信息--这里可截获我要的方法异常信息
        public override void OnException(MethodExecutionArgs eventArgs)
        {
            Console.WriteLine(string.Format("{0}.{1} Method. The Exception Is：{2}",
                eventArgs.Method.DeclaringType.FullName, eventArgs.Method.Name, eventArgs.Exception.Message));

        }
    }
}
