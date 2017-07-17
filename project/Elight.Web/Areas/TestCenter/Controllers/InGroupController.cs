using Elight.Web.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Elight.Web.Areas.TestCenter.Controllers
{
    public class InGroupController : BaseController
    {
        // GET: TestCenter/InGroup
        public ActionResult Index()
        {
            return View();
        }
    }
}