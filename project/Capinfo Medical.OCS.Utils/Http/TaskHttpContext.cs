using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace TestingCenterSystem.Utils.Http
{
    public class TaskHttpContext
    {
        private TaskHttpContext() { }

        private static HttpContext TContext;
        public static void SetInstance(HttpContext t)
        {
            TContext = t;
        }
        public static HttpContext GetInstance()
        {
            return TContext;
        }
    }
}
