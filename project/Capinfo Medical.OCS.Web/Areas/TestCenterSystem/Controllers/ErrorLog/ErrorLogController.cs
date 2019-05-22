using Elight.Infrastructure;
using Elight.Web.Controllers;
using System.Collections.Generic;
using System.Web.Mvc;
using Elight.Web.Filters;
using TestingCenterSystem.Service.DataItem;
using TestingCenterSystem.Service.DataItem.Interface;
using TestingCenterSystem.Service.ErrorLog;
using TestingCenterSystem.Service.ErrorLog.Interface;
using TestingCenterSystem.ViewModels.Project;

namespace Elight.Web.Areas.TestCenterSystem.Controllers
{
    [LoginChecked]
    public class ErrorLogController : BaseController
    {
        private static readonly ICatConfService _catconService = new CatConfService();
        private static readonly IErrorLogService _ErrorService = new ErrorLogService();

        public ActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// 分页数据
        /// </summary>
        [HttpGet]
        public ActionResult GetList(int page, int limit, string keyWord = "")
        {
            int sdId = ProjectProvider.Instance.Current.SD_ID;
            var pageData = _ErrorService.GetPage(page, limit, sdId, keyWord);
            var result = new Dictionary<string, object>()
            {
                { "code", 0},
                { "msg","success"},
                { "data", pageData.Items},
                { "count", pageData.TotalItems}
            };
            return Content(result.ToJson());
        }
    }
}