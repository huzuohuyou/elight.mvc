using System.Web.Mvc;
using Elight.Entity;
using Elight.Infrastructure;

namespace Elight.Web.Controllers
{
    /// <summary>
    /// 控制器基类。
    /// </summary>
    public class BaseController : Controller
    {

        #region 快捷方法
        protected ActionResult Success(string message = "恭喜您，操作成功。", object data = null)
        {
            return Content(new AjaxResult(ResultType.Success, message, data).ToJson());
        }
        protected ActionResult Error(string message = "对不起，操作失败。", object data = null)
        {
            return Content(new AjaxResult(ResultType.Error, message, data).ToJson());
        }
        protected ActionResult Warning(string message, object data = null)
        {
            return Content(new AjaxResult(ResultType.Warning, message, data).ToJson());
        }
        protected ActionResult Info(string message, object data = null)
        {
            return Content(new AjaxResult(ResultType.Info, message, data).ToJson());
        }

        protected ActionResult NoPage()
        {
            var result = new LayPadding<dynamic>()
            {
                result = true,
                msg = "查无数据",
                list =null,
                count = 0
            };
            return Content(result.ToJson());
        }
        #endregion
    }
}
