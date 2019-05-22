using Elight.Entity;
using Elight.Infrastructure;
using Elight.Web.Controllers;
using Elight.Web.Filters;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using TestingCenterSystem.Models.PDP;
using TestingCenterSystem.Models.Unit;
using TestingCenterSystem.Service.comm.Interface;
using TestingCenterSystem.Service.Comm;
using TestingCenterSystem.Service.Comm.Interface;
using TestingCenterSystem.Service.DataItem;
using TestingCenterSystem.Service.DataItem.Interface;
using TestingCenterSystem.Service.InGroup;
using TestingCenterSystem.Service.InGroup.Interface;
using TestingCenterSystem.Service.Project;
using TestingCenterSystem.Service.Project.Interface;
using TestingCenterSystem.Service.Sys;
using TestingCenterSystem.Service.Sys.Interface;
using TestingCenterSystem.Utils;
using TestingCenterSystem.ViewModels.DataItem;
using TestingCenterSystem.ViewModels.Project;
using ThreadState = System.Threading.ThreadState;

namespace Elight.Web.Areas.TestCenterSystem.Controllers
{
    [LoginChecked]
    public partial class CustomItemController : BaseController
    {
        private readonly IUserService _userService = new UserService();
        private readonly IDataItemService _itemService = new DataItemService();
        private readonly ICatConfService _catconfService = new CatConfService();
        private readonly IProjectService _projectService = new ProjectService();
        private readonly IItemDepService _itemDepService = new ItemDepService();
        private readonly IItemResultService _itemResultService = new ItemResultService();
        private readonly ItemValueLockService _itemValueLockService = new ItemValueLockService();
        private readonly IProcStateService _procStateService = new ProcStateService();
        private new readonly IProcLogService _procLogService = new ProcLogService();
        private static Dictionary<string, string> _itemResult = new Dictionary<string, string>();
        private readonly ISDService _sdService = new SDService();
        public static List<ITEM_VALUE_VIEWMODEL> TestResult = new List<ITEM_VALUE_VIEWMODEL>();
        private readonly object _locker = new object();
        [HttpGet]
        public ActionResult Index(string primaryKey)
        {
            ViewBag.SD_NAME = _projectService.GetCurrentSD().SD_NAME;
            ViewBag.TC_PROJECT_NAME = _projectService.GetCurrentSD().TC_PROJECT_NAME;
            var id = int.Parse(primaryKey);
            var entity = _itemService.Get(item => item.SD_ITEM_ID == id);
            ViewBag.SD_ITEM_CODE = entity.SD_ITEM_CODE;
            ViewBag.SD_ITEM_NAME = entity.SD_ITEM_NAME;
            ViewBag.SD_ITEM_ALIAS = entity.SD_ITEM_ALIAS;
            ViewBag.SD_ITEM_ALGO = entity.SD_ITEM_ALGO;
            var procCatCode = _procStateService.DataItemProcCatCode();
            var state = _procStateService.Get(proc => proc.PROC_CONTENT_ID == primaryKey && proc.PROC_CAT_CODE == procCatCode);
            ViewBag.ExecuteState = state.PROC_STAT_CODE;
            if (state.PROC_STAT_CODE != "1")
            {
                ViewBag.LastExeUser = state.UPD_USER_ID;
                ViewBag.LastExeTime = state.UPD_DATE;
            }
            var catEntity = _catconfService.Get(cat => cat.CAT_ID == entity.SD_ITEM_CAT_ID);
            if (catEntity != null)
                ViewBag.SD_ITEM_CAT_NAME = catEntity.CAT_NAME;
            var sd_id = _projectService.GetCurrentSD().SD_ID;
            var procCat = _procLogService.DataItemCatCode();
            var procLog =
                _procLogService.GetSearchList(
                    proc => proc.PROC_CONTENT_ID == primaryKey && proc.SD_ID == sd_id && proc.PROC_CAT_CODE == procCat)
                    .OrderByDescending(proc => proc.UPD_DATE)
                    .FirstOrDefault();
            if (procLog != null)
            {
                ViewBag.StartTime = procLog.START_TIME.ToDateString();
                ViewBag.EndTime = procLog.END_TIME.ToDateString();
                ViewBag.PatientId = procLog.PATIENT_ID;
                ViewBag.TestUrl = procLog.PROC_URL;
                ViewBag.ExecuteUrl = procLog.PROC_URL;
            }
            return View();
        }

        [HttpGet]
        public ActionResult Index1(string primaryKey)
        {
            var sd_id = _projectService.GetCurrentSD().SD_ID;
            ViewBag.SD_NAME = _projectService.GetCurrentSD().SD_NAME;
            ViewBag.TC_PROJECT_NAME = _projectService.GetCurrentSD().TC_PROJECT_NAME;
            var id = int.Parse(primaryKey);
            var entity = _itemService.Get(item => item.SD_ITEM_ID == id);
            ViewBag.SD_ITEM_CODE = entity.SD_ITEM_CODE;
            ViewBag.SD_ITEM_NAME = entity.SD_ITEM_NAME;
            ViewBag.SD_ITEM_ALIAS = entity.SD_ITEM_ALIAS;
            ViewBag.SD_ITEM_ALGO = entity.SD_ITEM_ALGO;
            var procCatCode = _procStateService.DataItemProcCatCode();
            var state = _procStateService.Get(proc => proc.SD_ID == sd_id && proc.PROC_CONTENT_ID == primaryKey && proc.PROC_CAT_CODE == procCatCode) ?? new PDP_PROC_STAT();//zlt
            ViewBag.ExecuteState = state.PROC_STAT_CODE;
            if (state.PROC_STAT_CODE != "1")
            {
                var model = _userService.Get(user => user.Id == state.UPD_USER_ID);
                var userName = model == null ? state.UPD_USER_ID : model.RealName;
                ViewBag.LastExeUser = userName;
                ViewBag.LastExeTime = state.UPD_DATE;
            }
            var catEntity = _catconfService.Get(cat => cat.CAT_ID == entity.SD_ITEM_CAT_ID);
            if (catEntity != null)
                ViewBag.SD_ITEM_CAT_NAME = catEntity.CAT_NAME;

            var procCat = _procLogService.DataItemCatCode();
            var procLog =
                _procLogService.GetSearchList(
                    proc => proc.PROC_CONTENT_ID == primaryKey && proc.SD_ID == sd_id && proc.PROC_CAT_CODE == procCat && proc.PROC_STAT_CODE == 2)
                    .OrderByDescending(proc => proc.UPD_DATE)
                    .FirstOrDefault();
            if (procLog != null)
            {
                ViewBag.TestStart = procLog.START_TIME == null ? "" : procLog.START_TIME.ToDateString();
                ViewBag.ExeStart = procLog.START_TIME == null ? "" : procLog.START_TIME.ToDateString();
                ViewBag.TestEnd = procLog.START_TIME == null ? "" : procLog.END_TIME.ToDateString();
                ViewBag.ExeEnd = procLog.START_TIME == null ? "" : procLog.END_TIME.ToDateString();
                ViewBag.PatientId = procLog.PATIENT_ID;
                ViewBag.TestUrl = procLog.PROC_URL.Replace("Many", "");
                ViewBag.ExecuteUrl = procLog.PROC_URL;
            }
            return View();
        }

        [HttpPost]
        public ActionResult Index(string primaryKey, int pageIndex, int pageSize, string keyWord)
        {
            var id = int.Parse(primaryKey);
            var pageData = _itemDepService.GetPage(id, pageIndex, pageSize, keyWord);
            var result = new LayPadding<ITEM_DEP_VIEWMODEL>()
            {
                result = true,
                msg = "success",
                list = pageData.Items,
                count = pageData.TotalItems
            };
            return Content(result.ToJson());
        }
        [HttpGet]
        public ActionResult GetDepList(string primaryKey, int page, int limit, string keyWord = "")
        {
            var id = int.Parse(primaryKey);
            var pageData = _itemDepService.GetPage(id, page, limit, keyWord);
            var result = new Dictionary<string, object>() {
                { "code", 0},
                { "msg","success"},
                { "data", pageData.Items},
                { "count", pageData.TotalItems}
            };
            return Content(result.ToJson());
        }

        #region 获取数据项集合
        /// <summary>
        /// 获取数据项集合
        /// </summary>
        /// <param name="primaryKey"></param>
        /// <param name="limit"></param>
        /// <param name="keyWord"></param>
        /// <param name="page"></param>
        /// <returns></returns>
        public ActionResult GetItemList1(string primaryKey, int page, int limit, string keyWord = "")
        {
            var id = int.Parse(primaryKey);
            var pageData = _itemDepService.GetItemColPage(id, page, limit, keyWord);
            var result = new Dictionary<string, object>() {
                { "code", 0},
                { "msg","success"},
                { "data", pageData.Items},
                { "count", pageData.TotalItems}
            };
            return Content(result.ToJson());
        }
        /// <summary>
        /// 获取数据项集合
        /// </summary>
        /// <param name="primaryKey"></param>
        /// <param name="limit"></param>
        /// <param name="keyWord"></param>
        /// <param name="page"></param>
        /// <returns></returns>
        public ActionResult GetItemList(string primaryKey, int pageIndex, int pageSize, string keyWord)
        {
            var id = int.Parse(primaryKey);
            var pageData = _itemDepService.GetItemColPage(id, pageIndex, pageSize, keyWord);
            var result = new LayPadding<ITEM_COLLECTION_VIEWMODEL>()
            {
                result = true,
                msg = "success",
                list = pageData.Items,
                count = pageData.TotalItems
            };
            return Content(result.ToJson());
        }
        #endregion
        #region 获取数据项结果列表

        /// <summary>
        /// 获取数据项结果列表
        /// </summary>
        /// <param name="primaryKey"></param>
        /// <param name="loadData"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="keyWord"></param>
        /// <returns></returns>
        public ActionResult GetItemValueList(string primaryKey, List<ITEM_VALUE_VIEWMODEL> loadData, int pageIndex, int pageSize, string keyWord)
        {
            var id = int.Parse(primaryKey);
            var pageData = _itemResultService.GetPage(loadData, id, pageIndex, pageSize, keyWord);
            if (pageData.Items.Count == 1 && pageData.Items[0] == null)
                pageData.Items.Clear();
            var page = new LayPadding<ITEM_VALUE_VIEWMODEL>()
            {
                result = true,
                msg = "success",
                list = pageData.Items,
                count = pageData.TotalItems
            };
            return Content(page.ToJson());
        }

        /// <summary>
        /// 获取数据项结果列表
        /// </summary>
        /// <param name="primaryKey"></param>
        /// <param name="loadData"></param>
        /// <param name="limit"></param>
        /// <param name="keyWord"></param>
        /// <param name="page"></param>
        /// <returns></returns>
        public ActionResult GetItemValueList1(string primaryKey, List<ITEM_VALUE_VIEWMODEL> loadData, int page, int limit, string keyWord)
        {
            if (loadData == null)
            {
                var res = new Dictionary<string, object>()
                {
                    {"code", 0},
                    {"msg", "success"},
                    {"data", null},
                    {"count", 0}
                };
                return Content(res.ToJson());
            }
            var id = int.Parse(primaryKey);
            var pageData = _itemResultService.GetPage(loadData, id, page, limit, keyWord);
            if (pageData.Items.Count == 1 && pageData.Items[0] == null)
                pageData.Items.Clear();
            var result = new Dictionary<string, object>() {
                { "code", 0},
                { "msg","success"},
                { "data", pageData.Items},
                { "count", pageData.TotalItems}
            };
            return Content(result.ToJson());
        }
        #endregion
        #region 测试
        /// <summary>
        /// 测试
        /// </summary>
        /// <param name="primaryKey"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult TestDllEvent(string primaryKey, TEST_PARAM_VIEWMODEL param)
        {
            var id = int.Parse(primaryKey);
            var model = _itemService.Get(item => item.SD_ITEM_ID == id);
            try
            {
                if (param.url.EndsWith("ManyDataItemApi"))
                    return Error("url输入错误！接口应为：DataItemApi");
                var startTime = param.startTime;
                var endTime = param.endTime;
                if (param.endTime != DateTime.MinValue)
                    endTime = Convert.ToDateTime(param.endTime.ToDateString() + " 23:59:59");
                var cpats = new List<UN_SD_CPATS>();
                var detailList = _itemResultService.GetCpatList(model.SD_ID.Value, param.startTime, endTime, param.patientId, ref cpats);
                if (cpats.Count == 0)
                    return Info("没有找到符合条件的入组信息！", null);
                var result = _itemService.TestUrl(param.url.Trim(), id, model, param);
                if (result == null)
                    return Error("测试失败！", null);
                //if (result.Count != cpatList.Count) return Error("测试失败！请检查信息填写是否正确！");
                //更新执行日志表
                var proLog = new PDP_PROC_LOG()
                {
                    SD_ID = model.SD_ID,
                    PROC_CAT_CODE = _procLogService.DataItemCatCode(),
                    PROC_CONTENT_ID = model.SD_ITEM_ID.ToString(),
                    PROC_URL = param.url,
                    PATIENT_ID = param.patientId
                };
                if (startTime != DateTime.MinValue)
                    proLog.START_TIME = startTime;
                if (startTime != DateTime.MinValue)
                    proLog.END_TIME = endTime;

                _procLogService.Insert(proLog);
                return Success("测试成功！", result);
            }
            catch (Exception ex)
            {
                if (model.SD_ID != null) _itemService.InsertErrorLog(model.SD_ID.Value, 2, param.url, "数据项测试失败！异常信息：" + ex.Message, "2");
                return Error("测试失败！");
            }
        }
        #endregion
        #region 执行

        /// <summary>
        /// 执行(弃用)
        /// </summary>
        /// <param name="primaryKey">数据项ID</param>
        /// <param name="param"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult ExecuteDllEvent(string primaryKey, TEST_PARAM_VIEWMODEL param)// string url)
        {
            var id = int.Parse(primaryKey);
            var model = _itemService.Get(item => item.SD_ITEM_ID == id);
            try
            {
                var cpats = new List<UN_SD_CPATS>();
                var detailList = _itemResultService.GetCpatList(model.SD_ID.Value, param.startTime, param.endTime, param.patientId, ref cpats);
                if (cpats.Count == 0)
                    return Info("没有找到符合条件的入组信息！");
                _itemService.ExecuteUrl(param.url.Trim(), id, param.clientKey, model, param);
                //更新执行状态表
                if (model.SD_ID != null)
                {
                    _procStateService.DataItemDoExecute(model.SD_ID.Value,
                        model.SD_ITEM_ID.ToString());
                    //更新执行日志表
                    var proLog = new PDP_PROC_LOG()
                    {
                        SD_ID = model.SD_ID,
                        PROC_CAT_CODE = _procLogService.DataItemCatCode(),
                        PROC_CONTENT_ID = model.SD_ITEM_ID.ToString(),
                        PATIENT_ID = param.patientId,
                        PROC_URL = param.url,
                        PROC_STAT_CODE = 2
                    };
                    if (param.startTime != DateTime.MinValue)
                        proLog.START_TIME = param.startTime;
                    if (param.endTime != DateTime.MinValue)
                        proLog.END_TIME = param.endTime;
                    _procLogService.Insert(proLog);
                }
                var proc = _procStateService.Get(item => item.PROC_CONTENT_ID == primaryKey && item.SD_ID == model.SD_ID.Value);
                var data = new Dictionary<string, string>
                {
                    {"exeUser", proc.UPD_USER_ID},
                    {"exeTime", proc.UPD_DATE.ToString()}
                };
                return Success("数据项正在后台执行，可在首页查看进度！", data);// : Error("执行失败！请检查信息是否完整！");
            }
            catch (Exception ex)
            {
                if (model.SD_ID != null) _itemService.InsertErrorLog(model.SD_ID.Value, 1, param.url, "数据项执行失败！异常信息：" + ex.Message, "2");
                return Error("执行失败！" + ex.ToString());
            }
        }

        /// <summary>
        /// 执行
        /// </summary>
        /// <param name="primaryKey">数据项ID</param>
        /// <param name="param"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult ExecuteDllEvent1(string primaryKey, TEST_PARAM_VIEWMODEL param)// string url)
        {
            var id = int.Parse(primaryKey);
            var model = _itemService.Get(item => item.SD_ITEM_ID == id);
            try
            {
                var cpats = new List<UN_SD_CPATS>();
                var detailList = _itemResultService.GetCpatList(model.SD_ID.Value, param.startTime, param.endTime, param.patientId, ref cpats);
                if (cpats.Count == 0)
                    return Info("没有找到符合条件的入组信息！");
                _itemService.ExecuteUrl(param.url.Trim(), id, param.clientKey, model, param);
                return Success("开始执行数据项！");//, data : Error("执行失败！请检查信息是否完整！");row == cpatList.Count ? 
            }
            catch (Exception ex)
            {
                if (model.SD_ID != null) _itemService.InsertErrorLog(model.SD_ID.Value, 1, param.url, "数据项执行失败！异常信息：" + ex.Message, "2");
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
                _itemService.CancelExe(clientKey);
                return null;
            }
            catch (Exception ex)
            {
                return Error("取消执行失败！" + ex.ToString());
            }
        }

        #endregion
        #region 删除依赖项
        /// <summary>
        /// 删除依赖项
        /// </summary>
        /// <param name="itemId"></param>
        /// <param name="depId"></param>
        /// <returns></returns>
        public ActionResult Delete(string itemId, string depId)
        {
            var itemIds = int.Parse(itemId);
            var depIds = int.Parse(depId);
            var row = _itemDepService.Delete(item => item.SD_ITEM_ID == itemIds && item.DEP_SD_ITEM_ID == depIds);
            return row > 0 ? Success() : Error();
        }
        #endregion

        public ActionResult LookResult()
        {
            return View();
        }
        public ActionResult RereshExeResult(string primaryKey)
        {
            var id = int.Parse(primaryKey);
            var model = _itemService.Get(item => item.SD_ITEM_ID == id);
            var proc = _procStateService.Get(item => item.PROC_CONTENT_ID == primaryKey && item.SD_ID == model.SD_ID.Value);
            var user = _userService.Get(u => u.Id == proc.UPD_USER_ID);
            var userName = user == null ? proc.UPD_USER_ID : user.RealName;
            var data = new Dictionary<string, string>
            {
                { "exeUser", userName},
                { "exeTime", proc.UPD_DATE.ToString()}
            };
            return Success("", data);
        }
        public ActionResult AddCustomDepend(string primaryKey)
        {
            var sdId = ProjectProvider.Instance.Current.SD_ID;
            //var catCode = _projectService.GetCurrentSD().ITEM_CAT_PA_CODE;
            var catCode = _sdService.Get(s => s.SD_ID == sdId).ITEM_CAT_PA_CODE;
            ViewBag.ItemType = _catconfService.GetSearchList(s => s.CAT_PA_CODE == catCode);
            return View();
        }
        public ActionResult AddCustomDepend1(string primaryKey)
        {
            var sdId = ProjectProvider.Instance.Current.SD_ID;
            //var catCode = _projectService.GetCurrentSD().ITEM_CAT_PA_CODE;
            var catCode = _sdService.Get(s => s.SD_ID == sdId).ITEM_CAT_PA_CODE;
            ViewBag.ItemType = _catconfService.GetSearchList(s => s.CAT_PA_CODE == catCode);
            return View();
        }
        #region 导出
        /// <summary>
        /// 导出
        /// </summary>
        /// <param name="primaryKey"></param>
        /// <returns></returns>
        public ActionResult ExportData(string primaryKey)
        {
            var id = int.Parse(primaryKey);
            var item = _itemService.Get(i => i.SD_ITEM_ID == id);
            var data = new Dictionary<string, List<ITEM_VALUE_VIEWMODEL>>();
            try
            {
                var itemState = _procStateService.DataItemProcCatCode();
                var state =
                    _procStateService.Get(
                        proc =>
                            proc.PROC_CONTENT_ID == primaryKey && proc.SD_ID == item.SD_ID &&
                            proc.PROC_CAT_CODE == itemState);
                //if (item.IS_RESULT == 0)
                //    data.Add("data", new List<ItemValueViewModel>());
                if (state.PROC_STAT_CODE == _procStateService.HasNotExeProcStatCode() || state.PROC_STAT_CODE == _procStateService.ClearDBProcStatCode())
                    data.Add("data", null);
                else
                {
                    var valueList = _itemResultService.GetList(id);
                    var title = new ITEM_VALUE_VIEWMODEL()
                    {
                        SD_CODE = "病种代码",
                        SD_ITEM_CODE = "数据项代码",
                        SD_ITEM_NAME = "数据项名称",
                        SD_ITEM_ID = "数据项ID",
                        SD_ITEM_DATA_TYPE = "数据类型",
                        SD_CPAT_NO = "入组样本ID",
                        SD_CPAT_DATE = "入组样本时间",
                        PATIENT_ID = "PATIENTID",
                        PATIENT_NO_I = "住院PATIENTNO",
                        PATIENT_NO_O = "门诊PATIENTNO",
                        PATIENT_NO_E = "急诊PATIENTNO",
                        SD_ITEM_VALUE = "数据项结果值",
                    };
                    data.Add("title", new List<ITEM_VALUE_VIEWMODEL>() { title });
                    data.Add("data", valueList);
                }
            }
            catch (Exception ex)
            {
                _itemService.InsertErrorLog(item.SD_ID.Value, 3, "", ex.Message, "2");
                data.Add("data", null);
            }
            return Content(data.ToJson());
        }
        #endregion
        #region 锁定
        /// <summary>
        /// 锁定
        /// </summary>
        /// <param name="primaryKey"></param>
        /// <returns></returns>
        public ActionResult LockData(string primaryKey)
        {
            var id = int.Parse(primaryKey);
            var model = _itemService.Get(i => i.SD_ITEM_ID == id);
            var itemState = _procStateService.DataItemProcCatCode();
            var sdId = _projectService.GetCurrentSD().SD_ID;
            try
            {
                var procState = _procStateService.Get(proc => proc.PROC_CONTENT_ID == primaryKey && proc.PROC_CAT_CODE == itemState && proc.SD_ID == sdId);
                if (procState.PROC_STAT_CODE != _procStateService.HadExedProcStatCode() && procState.PROC_STAT_CODE != _procStateService.LockProcStatCode())
                    return Error("数据项未执行，无法进行锁定！");
                if (model.IS_RESULT == 0)
                    return Error("数据项非结果数据项，无法进行锁定！");
                var result = _itemValueLockService.LockData(id, model);
                return result.Item1 ? Success(result.Item2) : Error(result.Item2);
            }
            catch (Exception ex)
            {
                _itemService.InsertErrorLog(sdId, 0, "", "锁定失败！" + ex.Message, "2");
                return Error("锁定失败！");
            }
        }
        #endregion
        #region 清库
        /// <summary>
        /// 清库
        /// </summary>
        /// <param name="primaryKey"></param>
        /// <returns></returns>
        public ActionResult ClearData(string primaryKey)
        {
            var id = int.Parse(primaryKey);
            var model = _itemService.Get(item => item.SD_ITEM_ID == id);
            try
            {
                var count = _itemResultService.GetUnitDataCount(id);
                var result = _itemResultService.UnitClearData(id);
                if (result == count)
                {
                    //更新执行状态表
                    _procStateService.DataItemDoClear(model.SD_ID.Value, primaryKey);
                    //更新执行日志表
                    var proLog = new PDP_PROC_LOG()
                    {
                        SD_ID = model.SD_ID,
                        PROC_CAT_CODE = _procLogService.DataItemCatCode(),
                        PROC_CONTENT_ID = model.SD_ITEM_ID.ToString(),
                        PATIENT_ID = "",
                        PROC_URL = "",
                        PROC_STAT_CODE = 3
                    };
                    _procLogService.Insert(proLog);
                    //更新数据项
                    _itemService.Update(model);
                }
                var proc = _procStateService.Get(item => item.PROC_CONTENT_ID == primaryKey && item.SD_ID == model.SD_ID.Value);
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
                _itemService.InsertErrorLog(model.SD_ID.Value, 0, "", "清库失败！" + ex.Message, "2");
                return Error("清库失败！");
            }
        }
        #endregion
        #region 添加依赖
        /// <summary>
        /// 添加依赖
        /// </summary>
        /// <param name="primaryKey"></param>
        /// <param name="itemIdList"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult AddDepend(string primaryKey, string itemIdList)
        {
            if (string.IsNullOrWhiteSpace(itemIdList)) return Info("您没有选择任何数据！");
            var idList = itemIdList.Split(',');
            var id = int.Parse(primaryKey);
            var count = 0;
            for (var index = 0; index < idList.Length; index++)
            {
                var depId = int.Parse(idList[index]);
                if (_itemDepService.Exists(r => r.SD_ITEM_ID == id && r.DEP_SD_ITEM_ID == depId))
                    continue;
                var model = new SD_ITEM_DEP()
                {
                    SD_ITEM_ID = id,
                    DEP_SD_ITEM_ID = depId,
                };
                var entity = _itemDepService.Insert(model);
                if (entity != null)
                    count++;
            }
            return count == idList.Length ? Success() : Error();
        }
        #endregion
    }
}