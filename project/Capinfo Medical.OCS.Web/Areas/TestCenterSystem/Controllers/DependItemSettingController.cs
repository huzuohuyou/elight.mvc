using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Elight.Web.Filters;

namespace Elight.Web.Areas.TestCenterSystem.Controllers
{
    [LoginChecked]
    public class DependItemSettingController : Controller
    {
        // GET: TestCenterSystem/DependItemSetting
        public ActionResult Index()
        {
            return View();
        }
    }
}