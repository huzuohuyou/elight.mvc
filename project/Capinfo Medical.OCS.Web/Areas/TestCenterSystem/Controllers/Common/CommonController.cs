using Elight.Infrastructure;
using Elight.Web.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Elight.Web.Filters;

namespace Elight.Web.Areas.TestCenterSystem.Controllers.Common
{
    [LoginChecked]
    public class CommonController : BaseController
    {
        // GET: TestCenterSystem/Json2CSV
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Json2CSV(string route, string data = null)
        {
            try
            {
                ViewBag.route = route;
                ViewBag.data = data;
                return View();
            }
            catch (Exception ex)
            {
                LogHelper.Error("Json2CSV:" + ex.ToString());
                throw;
            }
        }
    }
}