using Elight.Infrastructure;
using Elight.Web.Controllers;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Elight.Web.Filters;
using TestingCenterSystem.Models.PDP;
using TestingCenterSystem.Service.DataItem;
using TestingCenterSystem.Service.DataItem.Interface;

namespace Elight.Web.Areas.TestCenterSystem.Controllers.DictMaintenance
{
    [LoginChecked]
    public class DictMaintenanceController : BaseController
    {
        private static readonly ICatConfService _catconfService = new CatConfService();

        #region 视图

        public ActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public ActionResult Form(string primaryKey = "")
        {
            ViewBag.ItemType = _catconfService.GetDropList();
            return View();
        }

        #endregion 视图

        public ActionResult GetCatPa(string dicType)
        {
            var list = _catconfService.GetSearchList(s => s.CAT_TYPE_CODE == dicType).ToList();
            var dicList = new Dictionary<string, string>();
            foreach (var conf in list.Where(conf => !string.IsNullOrWhiteSpace(conf.CAT_PA_CODE) && !dicList.ContainsKey(conf.CAT_PA_CODE)))
            {
                dicList.Add(conf.CAT_PA_CODE, conf.CAT_PA_NAME);
            }
            var result = dicList.Select(dic => new Dictionary<string, string>() { { "CAT_PA_CODE", dic.Key }, { "CAT_PA_NAME", dic.Value } }).ToList();
            return Content(result.ToJson());
        }
        public ActionResult GetList(int page, int limit, string keyword = "")
        {
            var pageData = _catconfService.GetPage(page, limit, keyword);
            var result = new Dictionary<string, object>()
            {
                {"code", "0"},
                {"msg","success" },
                {"data",pageData.Items },
                {"count",pageData.TotalItems }
            };
            return Content(result.ToJson());
        }

        public ActionResult Form(SD_CAT_CONF model, string name)
        {
            bool isSuccess = false;
            model.CAT_TYPE_NAME = name;
            string message = string.Empty;
            isSuccess = _catconfService.InsOrUpd(model, out message);
            if (isSuccess)
            {
                return Success();
            }
            else
            {
                return string.IsNullOrEmpty(message) ? Error() : Info(message);
            }
        }

        public ActionResult Delete(string catId)
        {
            return _catconfService.ExDelete(d => d.CAT_ID.ToString() == catId) > 0 ?
                Success() : Error();
        }

        [HttpPost]
        public ActionResult GetSingle(string catId)
        {
            var single = _catconfService.Get(s => s.CAT_ID.ToString() == catId);
            return Content(single.ToJson());
        }
    }
}