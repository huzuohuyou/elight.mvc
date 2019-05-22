﻿using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Elight.Entity;
using Elight.Infrastructure;
using Elight.IService;
using Elight.Web.Filters;

namespace Elight.Web.Controllers
{
    [LoginChecked]
    public class HomeController : BaseController
    {
        private readonly IUserService _userService;
        private readonly IUserLogOnService _userLogOnService;
        private readonly IUserRoleRelationService _userRoleRelationService;
        private readonly ILogService _logService;
        private readonly IPermissionService _permissionService;
        

        public HomeController(IUserService userService, IUserLogOnService userLogOnService,
            IUserRoleRelationService userRoleRelationService, ILogService logService, IPermissionService permissionService)
        {
            this._userService = userService;
            this._userLogOnService = userLogOnService;
            this._userRoleRelationService = userRoleRelationService;
            this._logService = logService;
            this._permissionService = permissionService;
        }

        /// <summary>
        /// 后台首页视图。
        /// </summary>
        /// <returns></returns>
        [HttpGet, LoginChecked]
        public ActionResult Index()
        {

            if (OperatorProvider.Instance.Current != null)
            {
                ViewBag.SoftwareName = Configs.GetValue("SoftwareName");
                ViewBag.Account =  OperatorProvider.Instance.Current.Account;
                ViewBag.Avatar =  OperatorProvider.Instance.Current.Avatar;
                return View();
            }
            else
            {
                return Redirect("/Account/Login");
            }
        }

        /// <summary>
        /// 默认显示视图。
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult Default()//zlt
        {
            
            return View();
        }

        /// <summary>
        /// 获取左侧菜单。
        /// </summary>
        /// <param name="flag">0:专病，1:全院，2:专科，3:其他</param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult GetLeftMenu(int flag)
        {
            string userId = OperatorProvider.Instance.Current.UserId;

            List<LayNavbar> listNavbar = new List<LayNavbar>();
            var listModules = _permissionService.GetList(userId);
            foreach (var item in listModules.Where(c => c.Type == ModuleType.Menu && c.Layer == 0).ToList())
            {
                LayNavbar navbarEntity = new LayNavbar();
                List<LayChildNavbar> listChildNav = new List<LayChildNavbar>();
                //var listChildNav = listModules.Where(c => c.Type == ModuleType.Menu && c.Layer == 1 && c.ParentId == item.Id)
                //    .Select(c => new LayChildNavbar() { href = c.Url, icon = c.Icon, title = c.Name }).ToList();
                var listChildPer = listModules.Where(c => c.Type == ModuleType.Menu && c.Layer == 1 && c.ParentId == item.Id) ?? new List<Sys_Permission>();
                if (item.EnCode != null && item.EnCode.Equals("project-manage"))
                {
                    if (flag == 0)
                        listChildNav = listChildPer.Where(c => c.EnCode.Split('-')[0].Contains("zb")).Select(c => new LayChildNavbar() { href = c.Url, icon = c.Icon, title = c.Name }).ToList();
                    else if (flag == 1)
                        listChildNav = listChildPer.Where(c => c.EnCode.Split('-')[0].Contains("qy")).Select(c => new LayChildNavbar() { href = c.Url, icon = c.Icon, title = c.Name }).ToList();
                    else if (flag == 2)
                        listChildNav = listChildPer.Where(c => c.EnCode.Split('-')[0].Contains("zk")).Select(c => new LayChildNavbar() { href = c.Url, icon = c.Icon, title = c.Name }).ToList();//c.EnCode.Substring(0, c.EnCode.IndexOf("-")
                    else
                        listChildNav = listChildPer.Select(c => new LayChildNavbar() { href = c.Url, icon = c.Icon, title = c.Name }).ToList();
                }
                else
                {
                    listChildNav = listChildPer.Select(c => new LayChildNavbar() { href = c.Url, icon = c.Icon, title = c.Name }).ToList();
                }
                navbarEntity.icon = item.Icon;
                navbarEntity.spread = false;
                navbarEntity.title = item.Name;
                navbarEntity.children = listChildNav;
                listNavbar.Add(navbarEntity);
            }
            return Content(listNavbar.ToJson());
        }

        [HttpPost]
        public ActionResult GetLeftMenu2(Operator ope)
        {
            var userId = ope.UserId;// OperatorProvider.Instance.Current;//?.UserId ?? "D1EF3DCD-2C7D-4E8F-8F29-9F73625DD5DF";

            List<LayNavbar> listNavbar = new List<LayNavbar>();
            var listModules = _permissionService.GetList(userId);
            foreach (var item in listModules.Where(c => c.Type == ModuleType.Menu && c.Layer == 0).ToList())
            {
                LayNavbar navbarEntity = new LayNavbar();
                var listChildNav = listModules.Where(c => c.Type == ModuleType.Menu && c.Layer == 1 && c.ParentId == item.Id)
                    .Select(c => new LayChildNavbar() { href = c.Url, icon = c.Icon, title = c.Name }).ToList();
                navbarEntity.icon = item.Icon;
                navbarEntity.spread = false;
                navbarEntity.title = item.Name;
                navbarEntity.children = listChildNav;
                listNavbar.Add(navbarEntity);
            }
            return Content(listNavbar.ToJson());
        }

        /// <summary>
        /// 获取登录用户权限。
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult GetPermission()
        {
            var userId = OperatorProvider.Instance.Current.UserId;
            var modules = _permissionService.GetList(userId);
            return Content(modules.ToJson());
        }
    }
}