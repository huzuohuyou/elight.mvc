using Elight.Infrastructure;
using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web.Http;
using System.Web.Http.Filters;

namespace Capinfo_Medical.CTS.API.Areas.IDIS.Controllers
{
    public class IncomeOvertheSamePeriodController : ApiController
    {
        
        [HttpGet]
        public HttpResponseMessage GetForm1(string flag, string dtp_current_start, string dtp_current_end, string dtp_period_start, string dtp_period_end)
        {
            //Response.ContentType = "application/json";
            //Response.Charset = "utf-8";
            flag = "all";
            dtp_current_start = "2019-01-01";
            dtp_current_end = "2019-02-01";
            dtp_period_start = "2019-03-01";
            dtp_period_end = "2019-04-05";
            

            var str = new
            {
                msg = "获取出错！JSON数据格式不正常",
                code = 500,
                total_cont = 111
            }.ToJson();

            HttpResponseMessage result = new HttpResponseMessage { Content = new StringContent(str, Encoding.GetEncoding("UTF-8"), "application/json") };
            return result;

        }
    }
}
