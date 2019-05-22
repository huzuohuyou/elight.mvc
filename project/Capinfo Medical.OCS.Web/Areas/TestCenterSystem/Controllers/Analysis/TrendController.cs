
using Elight.Web.Controllers;
using Elight.Web.Filters;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using TestingCenterSystem.Service.comm.Interface;
using TestingCenterSystem.Service.DataItem;
using TestingCenterSystem.Service.DataItem.Interface;
using TestingCenterSystem.Service.InGroup;
using TestingCenterSystem.Service.KPI;
using TestingCenterSystem.Service.KPI.Interface;
using TestingCenterSystem.Service.Project;
using TestingCenterSystem.Service.Project.Interface;
using TestingCenterSystem.Service.Sys;
using TestingCenterSystem.Service.Sys.Interface;

namespace Elight.Web.Areas.TestCenterSystem.Controllers.Analysis
{
    [LoginChecked]
    public class TrendController : BaseController
    {
        private static readonly IProjectService _projectService = new ProjectService();
        private static readonly IKPIService _kpiService = new KPIService();
        private static readonly IUserService _userService = new UserService();
        private static readonly IDataItemService _dataItemService = new DataItemService();
        private static readonly IProcStateService _procStateService = new ProcStateService();
        // GET: TestCenterSystem/Trend
        public ActionResult Index()
        {
            return View();
        }

        public object GetTrendofProject(string PROJECT_ID, string PROC_CAT_CODE, string USER_ID = "")
        {
            if (PROJECT_ID == "")
            {
                return Error("无数据");
            }
            var SD_IDS = "";
            var P_ID = int.Parse(PROJECT_ID);
            _sdService.GetManay(r => r.TC_PROJ_ID == P_ID && r.SD_TYPE_CODE == 0).ToList().ForEach(r => SD_IDS += r.SD_ID.ToString() + "|");
            return GetTrendofSD(PROC_CAT_CODE, USER_ID, SD_IDS.Trim('|'));
        }


        public object GetTrendofSD(string PROC_CAT_CODE, string USER_ID = "", string SD_IDS = "")
        {
            if (SD_IDS == "" | SD_IDS == null)
            {
                return Error("无病种");
            }
            var DATA = new
            {
                TIME_RANGE = new List<string>() { },
                TO_BE_EXECUTED = new List<int>() { },
                EXECUTED = new List<int>() { },
                ALREADY_LOCKED = new List<int>() { },
            };
            var START_TIME = _sdService.GetStartDatetime(SD_IDS);
            var END_TIME = _sdService.GetEndDatetime(SD_IDS);
            var SD_CACHES = _procLogService.GetManay(r => SD_IDS.Contains(r.SD_ID.ToString()));
            for (var i = START_TIME; i <= END_TIME.Value.AddDays(1); i = i.Value.AddDays(1))
            {
                DATA.TIME_RANGE.Add(i.Value.ToShortDateString());
                DATA.TO_BE_EXECUTED.Add(
                    SD_CACHES
                    .Where(
                        r => r.PROC_CAT_CODE == PROC_CAT_CODE
                        && r.UPD_DATE != null
                        && r.UPD_DATE.Value.Day == i.Value.Day
                        && r.UPD_DATE.Value.Month == i.Value.Month
                        && r.UPD_DATE.Value.Year == i.Value.Year
                        && r.UPD_USER_ID.Contains(USER_ID)
                        && r.PROC_STAT_CODE == 1)
                        .ToList()
                        .GroupBy(
                        r => new { r.PROC_CAT_CODE, r.PROC_CONTENT_ID })
                        .ToList()
                        .Count);
                DATA.EXECUTED.Add(
                    SD_CACHES
                    .Where(
                        r => r.PROC_CAT_CODE == PROC_CAT_CODE
                        && r.UPD_DATE != null
                        && r.UPD_DATE.Value.Day == i.Value.Day
                        && r.UPD_DATE.Value.Month == i.Value.Month
                        && r.UPD_DATE.Value.Year == i.Value.Year
                        && r.PROC_STAT_CODE == 2
                        && r.UPD_USER_ID.Contains(USER_ID))
                        .ToList()
                        .GroupBy(
                        r => new { r.PROC_CAT_CODE, r.PROC_CONTENT_ID })
                        .ToList()
                        .Count);
                DATA.ALREADY_LOCKED.Add(
                    SD_CACHES
                    .Where(
                        r => r.PROC_CAT_CODE == PROC_CAT_CODE
                        && r.UPD_DATE != null
                        && r.UPD_DATE.Value.Day == i.Value.Day
                        && r.UPD_DATE.Value.Month == i.Value.Month
                        && r.UPD_DATE.Value.Year == i.Value.Year
                        && r.PROC_STAT_CODE == 4
                        && r.UPD_USER_ID.Contains(USER_ID))
                        .ToList()
                        .GroupBy(
                        r => new { r.PROC_CAT_CODE, r.PROC_CONTENT_ID })
                        .ToList()
                        .Count);
            }

            return DATA;
        }

        public JsonResult ProjectInGroup(string PROJECT_ID, string SD_ID)
        {
            if (SD_ID != "")
            {
                return Json(GetTrendofSD("1", "", SD_ID), JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(GetTrendofProject(PROJECT_ID, "1"), JsonRequestBehavior.AllowGet);
            }

        }

        public JsonResult ProjectDataItem(string PROJECT_ID, string SD_ID)
        {
            if (SD_ID != "")
            {
                return Json(GetTrendofSD("2", "", SD_ID), JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(GetTrendofProject(PROJECT_ID, "2"), JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult ProjectKPI(string PROJECT_ID, string SD_ID)
        {
            if (SD_ID != "")
            {
                return Json(GetTrendofSD("3", "", SD_ID), JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(GetTrendofProject(PROJECT_ID, "3"), JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult Users(string PROJECT_ID, string SD_ID)
        {
            #region V1.3
            //if (SD_ID != "")
            //{
            //    return Json(_sdService.GetSDMember(int.Parse(SD_ID)), JsonRequestBehavior.AllowGet);
            //}
            //else
            //{
            //    return Json(_projectService.GetProjectMember(int.Parse(PROJECT_ID)), JsonRequestBehavior.AllowGet);
            //} 
            #endregion

            if (SD_ID != "")
                return Json(_sdService.GetSDDateItemMember(int.Parse(SD_ID)).OrderBy(r=>r.Item2).ToList(), JsonRequestBehavior.AllowGet);
            else
                return Json(_projectService.GetProjectDataItemMember(int.Parse(PROJECT_ID)).OrderBy(r=>r.Item2).ToList(), JsonRequestBehavior.AllowGet);
        }

        public JsonResult UserInGroup(string PROJECT_ID, string SD_ID, string USER_ID = "")
        {
            if (SD_ID != "")
            {
                return Json(GetTrendofSD("1", USER_ID, SD_ID), JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(GetTrendofProject(PROJECT_ID, "1", USER_ID), JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult UserDataItem(string PROJECT_ID, string SD_ID, string USER_ID = "")
        {
            if (SD_ID != "")
            {
                return Json(GetTrendofSD("2", USER_ID, SD_ID), JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(GetTrendofProject(PROJECT_ID, "2", USER_ID), JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult UserKPI(string PROJECT_ID, string SD_ID, string USER_ID = "")
        {
            if (SD_ID != "")
            {
                return Json(GetTrendofSD("3", USER_ID, SD_ID), JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(GetTrendofProject(PROJECT_ID, "3", USER_ID), JsonRequestBehavior.AllowGet);
            }
        }
    }
}