using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestingCenterSystem.Utils.StringExtend
{
    public static class SimpleReg
    {
        /// <summary>
        /// 从核心词向后截取length个字符
        /// </summary>
        /// <param name="content">备操作字符串</param>
        /// <param name="coreWord">核心词</param>
        /// <param name="length">长度</param>
        /// <returns></returns>
        public static string SubString(this string content, string coreWord,int length)
        {
            int startIndex = content.IndexOf(coreWord);
            return content.Substring(startIndex, length);

        }
        public static void demo() {
            var d = "EMR表，EMR_TYPE_NAME/LGCY_TYPE_NAME/LGCY_SUB_TYPE_NAME含“新生儿”and“生产当日”的，REC_CONTENT【关键词】“免疫球蛋白”向前向后各截取10个字符，满足【关键词】前含“予”或【关键词】后含“100”的，记为“给予乙肝免疫球蛋白注射”";
            d.SubString("新生儿", 10);
        }
    }
}
