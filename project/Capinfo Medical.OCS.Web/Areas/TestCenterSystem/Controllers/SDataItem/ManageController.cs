using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Elight.Entity;
using Elight.Infrastructure;
using Elight.Web.Controllers;
using Elight.Web.Filters;
using TestingCenterSystem.Models.PDP;
using TestingCenterSystem.Models.Unit;
using TestingCenterSystem.Service.comm.Interface;
using TestingCenterSystem.Service.Comm;
using TestingCenterSystem.Service.Comm.Interface;
using TestingCenterSystem.Service.DataItem;
using TestingCenterSystem.Service.DataItem.Interface;
using TestingCenterSystem.Service.InGroup;
using TestingCenterSystem.Service.SDataItem;
using TestingCenterSystem.Service.SDataItem.Interface;
using TestingCenterSystem.Service.Sys;
using TestingCenterSystem.Service.Sys.Interface;
using TestingCenterSystem.ViewModels.DataItem;
using TestingCenterSystem.ViewModels.Project;
using TestingCenterSystem.ViewModels.SDataItem;

namespace Elight.Web.Areas.TestCenterSystem.Controllers.SDataItem
{
    [LoginChecked]
    public class ManageController : BaseController
    {
        private readonly IUserService _userService = new UserService();
        private readonly ISDataItemService _sDataItemService = new SDataItemService();
        private readonly IProcStateService _procStateService = new ProcStateService();
        private new readonly IProcLogService _procLogService = new ProcLogService();
        private readonly IDataItemService _itemService = new DataItemService();
        private readonly SItemResultService _itemResultService = new SItemResultService();
        private readonly SPNOService _spnoService = new SPNOService();
        // GET: TestCenterSystem/Manage
        public ActionResult Index(string primaryKey)
        {
            var id = int.Parse(primaryKey);
            var entity = _sDataItemService.Get(item => item.SITEM_ID == id);
            ViewBag.SITEM_CODE = entity.SITEM_CODE;
            ViewBag.SITEM_NAME = entity.SITEM_NAME;
            ViewBag.SITEM_ALIAS = entity.SITEM_ALIAS;
            var procCatCode = _procStateService.SDataItemProcCatCode();
            var state =
                _procStateService.Get(proc => proc.PROC_CONTENT_ID == primaryKey && proc.PROC_CAT_CODE == procCatCode);
            ViewBag.ExecuteState = state.PROC_STAT_CODE;
            if (state.PROC_STAT_CODE != "1")
            {
                var model = _userService.Get(user => user.Id == state.UPD_USER_ID);
                var userName = model == null ? state.UPD_USER_ID : model.RealName;
                ViewBag.LastExeUser = userName;
                ViewBag.LastExeTime = state.UPD_DATE;
            }
            var sd_id = ProjectProvider.Instance.Current.SD_ID;
            var procCat = _procLogService.SDataItemCatCode();
            var procLog =
                _procLogService.GetSearchList(
                    proc => proc.PROC_CONTENT_ID == primaryKey && proc.SD_ID == sd_id && proc.PROC_CAT_CODE == procCat && proc.PROC_STAT_CODE == 4)
                    .OrderByDescending(proc => proc.UPD_DATE)
                    .FirstOrDefault();
            if (procLog != null)
            {
                ViewBag.TestStart = procLog.START_TIME == null ? "" : procLog.START_TIME.ToDateString();
                ViewBag.ExeStart = procLog.START_TIME == null ? "" : procLog.START_TIME.ToDateString();
                ViewBag.TestEnd = procLog.START_TIME == null ? "" : procLog.END_TIME.ToDateString();
                ViewBag.ExeEnd = procLog.START_TIME == null ? "" : procLog.END_TIME.ToDateString();
                ViewBag.PatientId = procLog.PATIENT_ID;
                ViewBag.TestUrl = procLog.PROC_URL;
                ViewBag.ExecuteUrl = procLog.PROC_URL;
            }
            return View();
        }

        [HttpPost]
        public ActionResult TestDllEvent(string primaryKey, TEST_PARAM_VIEWMODEL param)
        {
            var id = int.Parse(primaryKey);
            var model = _sDataItemService.Get(item => item.SITEM_ID == id);
            try
            {
                var result = _sDataItemService.TestUrl(param.url.Trim(), id, "", model, param);
                if (result == null)
                    return Error("api接口调用失败，url或参数信息不正确！", null);
                //更新执行日志表
                var proLog = new PDP_PROC_LOG()
                {
                    SD_ID = int.Parse(model.SD_ID),
                    PROC_CAT_CODE = _procLogService.SDataItemCatCode(),
                    PROC_CONTENT_ID = model.SITEM_ID.ToString(),
                    PROC_URL = param.url,
                    PATIENT_ID = param.patientId
                };
                if (param.startTime != DateTime.MinValue)
                    proLog.START_TIME = param.startTime;
                if (param.startTime != DateTime.MinValue)
                    proLog.END_TIME = param.endTime;
                _procLogService.Insert(proLog);
                return Success("测试成功！", result);
            }
            catch (Exception ex)
            {
                if (model.SD_ID != null)
                    _itemService.InsertErrorLog(int.Parse(model.SD_ID), 2, param.url, "数据项测试失败！异常信息：" + ex.Message, "4");
                return Error("测试失败！");
            }
        }

        [HttpPost]
        public ActionResult ExecuteDllEvent(string primaryKey, TEST_PARAM_VIEWMODEL param) // string url)
        {
            var id = int.Parse(primaryKey);
            var model = _sDataItemService.Get(item => item.SITEM_ID == id);
            try
            {
                _sDataItemService.ExecuteUrl(param.url.Trim(), id, param.clientKey, model, param);
                return Success("开始执行数据项！"); //, data : Error("执行失败！请检查信息是否完整！");row == cpatList.Count ? 
            }
            catch (Exception ex)
            {
                if (model.SD_ID != null)
                    _itemService.InsertErrorLog(int.Parse(model.SD_ID), 1, param.url, "数据项执行失败！异常信息：" + ex.Message, "2");
                return Error("执行失败！" + ex.ToString());
            }
        }
        /// <summary>
        /// 执行
        /// </summary>
        /// <param name="clientKey"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult CancelExeDll(string clientKey)// string url)
        {
            try
            {
                _sDataItemService.CancelExe(clientKey);
                return null;
            }
            catch (Exception ex)
            {
                return Error("取消执行失败！" + ex.ToString());
            }
        }

        public ActionResult ClearData(string primaryKey)
        {
            var id = int.Parse(primaryKey);
            var model = _sDataItemService.Get(item => item.SITEM_ID == id);
            var sdId = int.Parse(model.SD_ID);
            try
            {
                var count = _itemResultService.Count(i => i.SD_SITEM_ID == id);
                var result = _itemResultService.ClearData(id);
                if (result == count)
                {
                    //更新执行状态表
                    _procStateService.SDataItemDoClear(sdId, primaryKey);
                    //更新数据项
                    _sDataItemService.Update(model);
                }
                var proc = _procStateService.Get(item => item.PROC_CONTENT_ID == primaryKey && item.SD_ID == sdId);
                var user = _userService.Get(u => u.Id == proc.UPD_USER_ID);
                var userName = user == null ? proc.UPD_USER_ID : user.RealName;
                var data = new Dictionary<string, string>
                {
                    {"exeUser", userName},
                    {"exeTime", proc.UPD_DATE.ToString()}
                };
                return result == count ? Success("清库成功！", data) : Error("清库失败！");
            }
            catch (Exception ex)
            {
                _itemService.InsertErrorLog(sdId, 0, "", "清库失败！" + ex.Message, "4");
                return Error("清库失败！");
            }
        }

        public ActionResult RereshExeResult(string primaryKey)
        {
            var id = int.Parse(primaryKey);
            var model = _sDataItemService.Get(item => item.SITEM_ID == id);
            var sdId = int.Parse(model.SD_ID);
            var proc =
                _procStateService.Get(item => item.PROC_CONTENT_ID == primaryKey && item.SD_ID == sdId);
            var user = _userService.Get(u => u.Id == proc.UPD_USER_ID);
            var userName = user == null ? proc.UPD_USER_ID : user.RealName;
            var data = new Dictionary<string, string>
            {
                {"exeUser", userName},
                {"exeTime", proc.UPD_DATE.ToString()}
            };
            return Success("", data);
        }

        public ActionResult ExportData(string primaryKey)
        {
            var id = int.Parse(primaryKey);
            var item = _sDataItemService.Get(i => i.SITEM_ID == id);
            var sdId = int.Parse(item.SD_ID);
            var data = new Dictionary<string, object>();
            data.Add("overflow", "");
            try
            {
                var itemState = _procStateService.SDataItemProcCatCode();
                var state =
                    _procStateService.Get(
                        proc =>
                            proc.PROC_CONTENT_ID == primaryKey && proc.SD_ID == sdId &&
                            proc.PROC_CAT_CODE == itemState);
                if (state.PROC_STAT_CODE == _procStateService.HasNotExeProcStatCode() ||
                    state.PROC_STAT_CODE == _procStateService.ClearDBProcStatCode())
                    data.Add("data", null);
                else
                {
                    if (_spnoService.Count(i => i.SITEM_ID == id) > 200000)
                    {
                        data.Add("data", null);
                        data["overflow"] = "数据量太大，无法导出！";
                        return Content(data.ToJson());
                    }
                    var itemValueList = _spnoService.GetManay(i => i.SITEM_ID == id).ToList();
                    var resList = new List<EX_SITEMRESULT_VIEWMODEL>();
                    resList.AddRange(itemValueList.Select((t, i) => new EX_SITEMRESULT_VIEWMODEL()
                    {
                        SITEM_NAME = item.SITEM_NAME,
                        PATIENT_NO = t.PATIENT_NO,
                        PATIENT_ID = t.PATIENT_ID,
                        SD_CPAT_NO = t.SD_CPAT_NO,
                        DEPT_NAME = t.DEPT_NAME,
                        DOCTOR_NAME = t.DOCTOR_NAME,
                        ACTUAL_VALUE = t.ACTUAL_VALUE.ToString(),
                        IN_DATE = t.IN_DATE.ToString(),
                        OUT_DATE = t.OUT_DATE.ToString()
                    }));
                    data.Add("title", new List<EX_SITEMRESULT_VIEWMODEL>()
                    {
                        new EX_SITEMRESULT_VIEWMODEL()
                        {
                            SITEM_NAME = "数据项名称",
                            PATIENT_NO = "PATIENT_NO",
                            PATIENT_ID = "PATIENT_ID",
                            SD_CPAT_NO = "入组样本号",
                            DEPT_NAME = "科室名称",
                            DOCTOR_NAME = "医生名称",
                            ACTUAL_VALUE = "结果值",
                            IN_DATE = "入院时间",
                            OUT_DATE = "出院时间"
                        }
                    });
                    data.Add("data", resList);
                    //data.Add("titleForKey", new List<string>() { "SITEM_NAME", "PATIENT_NO", "PATIENT_ID", "SD_CPAT_NO", "DEPT_NAME", "DOCTOR_NAME", "ACTUAL_VALUE", "IN_DATE", "OUT_DATE" });
                    //data.Add("title", new List<string>() { "", "数据项名称", "PATIENT_NO", "PATIENT_ID", "入组样本号", "科室名称", "医生名称", "结果值", "入院时间", "出院时间" });
                    //data.Add("data", resList);
                }
            }
            catch (Exception ex)
            {
                _itemService.InsertErrorLog(int.Parse(item.SD_ID), 3, "", ex.Message, "4");
                data.Add("data", null);
            }
            return Content(data.ToJson());
        }
    }
}