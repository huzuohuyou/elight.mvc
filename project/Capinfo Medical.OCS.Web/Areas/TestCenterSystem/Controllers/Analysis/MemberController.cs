using Elight.Infrastructure;
using Elight.Web.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Elight.Web.Filters;
using TestingCenterSystem.Service.Project;
using TestingCenterSystem.Service.Project.Interface;
using TestingCenterSystem.Service.Sys;
using TestingCenterSystem.Service.Sys.Interface;
using TestingCenterSystem.ViewModels.Analysis;

namespace Elight.Web.Areas.TestCenterSystem.Controllers.Analysis
{
    [LoginChecked]
    public class MemberController : BaseController
    {
        private static readonly IUserService _userService = new UserService();
        private static readonly IRoleService _roleService = new RoleService();
        private static readonly IUserRoleService _userRoleService = new UserRoleService();
        private static readonly IProjectService _projectService = new ProjectService();

        public JsonResult ProjectDevelopers()
        {
            return Json(_userService.GetDevelopers(), JsonRequestBehavior.AllowGet);
        }

        public ActionResult DeveloperCost(DateTime? START_DATE, DateTime? END_DATE, string USER_ID, string PROJECT_ID, string SD_ID)
        {
            var ID = "";
            USER_ID = USER_ID ?? "";
            ICanGetTimeCost costService = null;
            if (SD_ID == null || SD_ID == "")
            {
                ID = PROJECT_ID ?? "";
                costService = new ProjectService();
            }
            else
            {
                ID = SD_ID ?? "";
                costService = new SDService();
            }
            var users = _userService.GetDevelopers();
            var memberCost = new List<MEMBER_COST_VIEWMODEL>();
            users.Where(r => r.Item1.Contains(USER_ID)).ToList().ForEach(r =>
              {
                  memberCost.Add(new MEMBER_COST_VIEWMODEL()
                  {
                      USER_NAME = r.Item2,
                      DONE_COUNT = costService.GetDateItemCount(ID?.ToString(), r.Item1,START_DATE, END_DATE),
                      COST_HOURS = costService.GetTimeCost(ID?.ToString(), r.Item1, START_DATE, END_DATE),
                      NO = 0
                  });
              });
            var sortResult = memberCost.OrderByDescending(r => r.EFFICIENCY);
            return Content(sortResult.ToJson());
        }


        public ActionResult DeveloperProjectCost(DateTime? START_DATE, DateTime? END_DATE, string USER_ID, string PROJECT_ID)
        {
            var projects = _userService.GetUserProject(USER_ID, START_DATE, END_DATE);
            PROJECT_ID = PROJECT_ID ?? "";
            USER_ID = USER_ID ?? "";
            ICanGetTimeCost costService = new ProjectService();
            var users = _userService.GetDevelopers();
            var memberCost = new List<MEMBER_COST_VIEWMODEL>();
            projects.ForEach(r =>
            {
                memberCost.Add(new MEMBER_COST_VIEWMODEL()
                {
                    USER_NAME = r.TC_PROJ_NAME,
                    DONE_COUNT = costService.GetDateItemCount(r.TC_PROJ_ID.ToString(), USER_ID, START_DATE, END_DATE),
                    COST_HOURS = costService.GetTimeCost(r.TC_PROJ_ID.ToString(), USER_ID, START_DATE, END_DATE),
                    NO = 0
                });
            });
            var sortResult = memberCost.OrderByDescending(r => r.EFFICIENCY);
            return Content(sortResult.ToJson());
        }

        public ActionResult DeveloperSDCost(DateTime? START_DATE, DateTime? END_DATE, string USER_ID, string PROJECT_ID, string SD_ID)
        {

            var sds=_userService.GetUserSD(USER_ID, START_DATE, END_DATE, PROJECT_ID, SD_ID);
            USER_ID = USER_ID ?? "";
            ICanGetTimeCost costService  = new SDService();
            var users = _userService.GetDevelopers();
            var memberCost = new List<MEMBER_COST_VIEWMODEL>();
            sds.ForEach(r =>
            {
                memberCost.Add(new MEMBER_COST_VIEWMODEL()
                {
                    USER_NAME = r.SD_NAME,
                    DONE_COUNT = costService.GetDateItemCount(r.SD_ID.ToString(), USER_ID, START_DATE, END_DATE),
                    COST_HOURS = costService.GetTimeCost(r.SD_ID.ToString(), USER_ID, START_DATE, END_DATE),
                    NO = 0
                });
            });
            var sortResult = memberCost.OrderByDescending(r => r.EFFICIENCY);
            return Content(sortResult.ToJson());
        }
    }
}