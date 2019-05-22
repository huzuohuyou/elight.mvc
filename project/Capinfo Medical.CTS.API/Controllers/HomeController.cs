using Elight.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace Capinfo_Medical.CTS.API.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            ViewBag.Title = "Home Page";

            return View();
        }

        [HttpGet]
        public HttpResponseMessage GetForm1(string flag, string dtp_current_start, string dtp_current_end, string dtp_period_start, string dtp_period_end)
        {
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

            HttpResponseMessage result = new HttpResponseMessage {
                Content = new StringContent(str, Encoding.GetEncoding("UTF-8"), "application/json") };
            return result;

        }
    }
}
