using Elight.Entity;
using Elight.Infrastructure;
using Elight.IService;
using Elight.Web.Controllers;
using Elight.Web.Filters;
using InhospitalIndicators.Service;
using InhospitalIndicators.Service.Entitys;
using System.Data;
using System.IO.Compression;
using System.Linq;
using System.Web.Mvc;

namespace Elight.Web.Areas.IDIS.Controllers
{
    //[LoginChecked]    
    public class IncomeOvertheSamePeriodController : BaseController// ApiController
    {
        private readonly IUserService _userService;
        private readonly IUserLogOnService _userLogOnService;
        private readonly IUserRoleRelationService _userRoleRelationService;

        public IncomeOvertheSamePeriodController(IUserService userService, IUserRoleRelationService userRoleRelationService, IUserLogOnService userLogOnService)
        {
            this._userService = userService;
            this._userRoleRelationService = userRoleRelationService;
            this._userLogOnService = userLogOnService;
        }

        public class CompressAttribute : ActionFilterAttribute
        {
            public override void OnActionExecuting(ActionExecutingContext filterContext)
            {
                var acceptEncoding = filterContext.HttpContext.Request.Headers["Accept-Encoding"];
                if (!string.IsNullOrEmpty(acceptEncoding))
                {
                    acceptEncoding = acceptEncoding.ToLower();
                    var response = filterContext.HttpContext.Response;
                    if (acceptEncoding.Contains("gzip"))
                    {
                        
                        response.AppendHeader("Content-Encoding", "gzip");
                        //response.AppendHeader("Transfer-Encoding", "chunked");
                        response.Filter = new GZipStream(response.Filter, CompressionMode.Compress);
                    }
                    else if (acceptEncoding.Contains("deflate"))
                    {
                        response.AppendHeader("Content-Encoding", "deflate");
                        response.Filter = new DeflateStream(response.Filter, CompressionMode.Compress);
                    }
                }
            }
        }

        [Compress]
        [HttpGet]
        public string GetForm1(string flag, string dtp_current_start, string dtp_current_end, string dtp_period_start, string dtp_period_end)
        {
            Response.ContentType = "application/json";
            Response.Charset = "utf-8";
            Response.CacheControl = "no-cache";
            //Response.AppendHeader("Content-encoding", "gzip");
            //flag = "all";
            //dtp_current_start = "2019-01-01";
            //dtp_current_end = "2019-02-01";
            //dtp_period_start = "2019-03-01";
            //dtp_period_end = "2019-04-05";

            if (flag==""
                || dtp_current_start==""
                || dtp_current_end == ""
                || dtp_period_start == ""
                || dtp_period_end == "")
            {
                return "";
            }
            var list = new SameReriodIncomeReportService(flag
                , dtp_current_start
                , dtp_current_end
                , dtp_period_start
                , dtp_period_end).Do();
            var result = new LayPadding<SamePeriodIncomRatio>
            {
                result = true,
                msg = "success",
                list = list,
                count = list.Count
            };
            return result.ToJson();


            
            
        }

        [HttpGet, AuthorizeChecked]
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost, AuthorizeChecked]
        public ActionResult Index(int pageIndex, int pageSize, string keyWord)
        {
            var pageData = _userService.GetList(pageIndex, pageSize, keyWord);
            var result = new LayPadding<Sys_User>()
            {
                result = true,
                msg = "success",
                list = pageData.Items,
                count = pageData.TotalItems
            };
            return Content(result.ToJson());
        }

        [HttpGet, AuthorizeChecked]
        public ActionResult Form()
        {
            return View();
        }

        [HttpPost, AuthorizeChecked, ValidateAntiForgeryToken]
        public ActionResult Form(Sys_User model, string password, string roleIds)
        {
            if (model.Id.IsNullOrEmpty())
            {
                //新增用户基本信息。
                var userId = _userService.Insert(model).ToString();
                //新增用户角色信息。
                _userRoleRelationService.SetRole(userId, roleIds.ToStrArray());
                //新增用户登陆信息。
                Sys_UserLogOn userLogOnEntity = new Sys_UserLogOn()
                {
                    UserId = userId,
                    Password = password
                };
                var userLoginId = _userLogOnService.Insert(userLogOnEntity);
                return userId != null && userLoginId != null ? Success() : Error();
            }
            else
            {
                //更新用户基本信息。
                int row = _userService.Update(model);
                //更新用户角色信息。
                _userRoleRelationService.SetRole(model.Id, roleIds.ToStrArray());
                return row > 0 ? Success() : Error();
            }
        }

        [HttpGet, AuthorizeChecked]
        public ActionResult Detail()
        {
            return View();
        }

        [HttpPost]
        public ActionResult GetForm(string primaryKey)
        {
            var entity = _userService.Get(primaryKey);

            entity.RoleId = _userRoleRelationService.GetList(entity.Id).Select(c => c.RoleId).ToList();

            return Content(entity.ToJson());
        }

        [HttpPost, AuthorizeChecked]
        public ActionResult Delete(string userIds)
        {
            //多用户删除。
            int row = _userService.Delete(userIds.ToStrArray());
            _userRoleRelationService.Delete(userIds.ToStrArray());
            _userLogOnService.Delete(userIds.ToStrArray());
            return row > 0 ? Success() : Error();
        }

        [HttpPost]
        public ActionResult CheckAccount(string userName)
        {
            var userEntity = _userService.GetByUserName(userName);
            if (userEntity != null)
            {
                return Error("已存在当前用户名，请重新输入");
            }
            return Success("恭喜您，该用户名可以注册");
        }


    }
}
