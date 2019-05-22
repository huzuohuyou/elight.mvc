using Elight.Entity;
using Elight.Infrastructure;
using Elight.Web.Controllers;
using Elight.Web.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Newtonsoft.Json;
using TestingCenterSystem.Models;
using TestingCenterSystem.Models.PDP;
using TestingCenterSystem.Models.Unit;
using TestingCenterSystem.Service.comm.Interface;
using TestingCenterSystem.Service.DataItem;
using TestingCenterSystem.Service.DataItem.Interface;
using TestingCenterSystem.Service.InGroup;
using TestingCenterSystem.Service.KPI;
using TestingCenterSystem.Service.KPI.Interface;
using TestingCenterSystem.Service.Project;
using TestingCenterSystem.Service.Project.Interface;
using TestingCenterSystem.Utils.Hubs;
using TestingCenterSystem.ViewModels.DataItem;
using TestingCenterSystem.ViewModels.Project;

namespace Elight.Web.Areas.TestCenterSystem.Controllers
{
    [LoginChecked]
    public class DataItemController : BaseController
    {
        private IDataItemService _itemService = new DataItemService();
        private readonly ICatConfService _catconfService = new CatConfService();
        private readonly IProcStateService _procStateService = new ProcStateService();
        private readonly IProjectService _projectService = new ProjectService();
        private readonly IItemDepService _itemDepService = new ItemDepService();
        private readonly IItemOptionService _itemOptionService = new ItemOptionService();
        private readonly IKPIParamService _kpiParamService = new KPIParamService();
        private readonly IItemResultService _itemResultService = new ItemResultService();
        private readonly ItemValueLockService _itemValueLockService = new ItemValueLockService();
        private readonly ISDService _sdService = new SDService();
        // GET: TestCenterSystem/DataItem
        #region 数据项主页面获取(Layui1.0)
        /// <summary>
        /// 数据项主页面获取
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult Index()
        {
            int? iIsCom = ProjectProvider.Instance.Current.IS_COMMON;
            if (iIsCom != null && iIsCom == 1)
            {
                return Redirect("/TestCenterSystem/CommonProjectItem/Index");
            }
            else
            {
                ViewBag.SD_NAME = _projectService.GetCurrentSD().SD_NAME;
                ViewBag.TC_PROJECT_NAME = _projectService.GetCurrentSD().TC_PROJECT_NAME;
                var catCode = _projectService.GetCurrentSD().ITEM_CAT_PA_CODE;
                ViewBag.ItemType = _catconfService.GetSearchList(s => s.CAT_PA_CODE == catCode);
                return View();
            }
        }
        #endregion
        #region 数据项主页面获取(Layui2.0)
        /// <summary>
        /// 数据项主页面获取
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult Index1()
        {
            int? iIsCom = ProjectProvider.Instance.Current.IS_COMMON;
            if (iIsCom != null && iIsCom == 1)
            {
                return Redirect("/TestCenterSystem/CommonProjectItem/Index");
            }
            else
            {
                ViewBag.SD_NAME = _projectService.GetCurrentSD().SD_NAME;
                ViewBag.TC_PROJECT_NAME = _projectService.GetCurrentSD().TC_PROJECT_NAME;
                var catCode = _projectService.GetCurrentSD().ITEM_CAT_PA_CODE;
                ViewBag.ItemType = _catconfService.GetSearchList(s => s.CAT_PA_CODE == catCode);
                return View();
            }
        }
        #endregion
        #region 执行进度页面获取
        /// <summary>
        /// 执行进度页面获取
        /// </summary>
        /// <param name="clientKey"></param>
        /// <param name="url"></param>
        /// <param name="ids"></param>
        /// <returns></returns>
        public ActionResult ExecuteProgress(string clientKey, string url, string ids, string type)
        {
            ViewBag.ClientKey = clientKey;
            ViewBag.Url = url;
            ViewBag.ExeData = ids;
            ViewBag.Type = type;
            return View();
        }
        #endregion
        #region
        /// <summary>
        /// 批量执行界面获取
        /// </summary>
        /// <param name="clientKey"></param>
        /// <param name="hasExe"></param>
        /// <param name="noExe"></param>
        /// <returns></returns>
        public ActionResult SelectDataItemExe(string clientKey, string hasExe, string noExe)
        {
            var hasExeitemIdList = hasExe.Split(',').ToList();
            var noExeItemIdList = noExe.Split(',').ToList();
            var hasExeItems = _itemService.GetManay(i => hasExeitemIdList.Contains(i.SD_ITEM_ID.ToString())).Select(i => i.SD_ITEM_ID);
            var noExeItems = _itemService.GetManay(i => noExeItemIdList.Contains(i.SD_ITEM_ID.ToString())).Select(i => i.SD_ITEM_ID);
            ViewBag.HasExeData = hasExeItems.ToJson();
            ViewBag.NoExeData = noExeItems.ToJson();
            ViewBag.ClientKey = clientKey;
            return View();
        }
        [HttpGet]
        public ActionResult GetHasExeDataItem(string ids)
        {
            var itemIdList = ids.Split(',').ToList();
            var hasExeItems = _itemService.GetManay(i => itemIdList.Contains(i.SD_ITEM_ID.ToString()));
            var result = new Dictionary<string, object>() {
                { "code", 0},
                { "msg","success"},
                { "data", hasExeItems},
                { "count", hasExeItems.Count}
            };
            return Content(result.ToJson());
        }
        #endregion
        #region 分页获取数据项（Layui1.0）
        /// <summary>
        /// 分页获取数据项列表
        /// </summary>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="keyWord"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult Index(int pageIndex, int pageSize, string keyWord)
        {
            var pageData = _itemService.GetPage(pageIndex, pageSize, keyWord);
            var result = new LayPadding<DATAITEM_VIEWMODEL>()
            {
                result = true,
                msg = "success",
                list = pageData.Items,
                count = pageData.TotalItems
            };
            return Content(result.ToJson());
        }
        #endregion
        #region 初始获取所有数据项数据（Layui2.0）
        /// <summary>
        /// 初始获取所有数据项数据
        /// </summary>
        /// <param name="page"></param>
        /// <param name="limit"></param>
        /// <param name="field"></param>
        /// <param name="type"></param>
        /// <param name="keyWord"></param>
        /// <returns></returns>
        [HttpGet]
        public ActionResult GetList(int page, int limit, string field, string type, string keyWord = "")
        {
            var pageData = _itemService.GetPage(page, limit, field, type, keyWord);
            var result = new Dictionary<string, object>() {
                { "code", 0},
                { "msg","success"},
                { "data", pageData.Items},
                { "count", pageData.TotalItems}
            };
            return Content(result.ToJson());
        }
        #endregion
        #region 删除数据项
        /// <summary>
        /// 删除数据项
        /// </summary>
        /// <param name="primaryKey"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult Delete(string primaryKey)
        {
            var id = int.Parse(primaryKey);
            if (_itemService.Exists(r => r.SD_ITEM_ID == id))
            {
                var catCode = _procStateService.DataItemProcCatCode();
                var procState = _procStateService.Get(
                    item => item.PROC_CAT_CODE == catCode && item.PROC_CONTENT_ID == primaryKey.Trim());
                if (procState != null)
                {
                    //var executeState = procState.PROC_STAT_CODE;
                    //if (executeState == _procStateService.HadExedProcStatCode() )
                    //    return Error("已被执行，请清库后再进行删除！");
                    var depCount = _itemDepService.GetManay(dep => dep.DEP_SD_ITEM_ID == id).Count;
                    if (depCount > 0)
                        return Error("已被其它数据项依赖，不可删除！");
                    var kpiCount = _kpiParamService.GetManay(param => param.SD_ITEM_ID == id).Count;
                    if (kpiCount > 0)
                        return Error("已被评价指标当作参数使用，不可删除！");
                }
                //清除单元库
                _itemResultService.UnitClearData(id);
                //清除PDP锁定数据
                _itemResultService.Delete(i => i.SD_ITEM_ID == id);
                //应先删除关联表  
                _procStateService.Delete(item => item.PROC_CAT_CODE == catCode && item.PROC_CONTENT_ID == primaryKey.Trim());
                _itemDepService.Delete(item => item.SD_ITEM_ID == id);
                _itemOptionService.Delete(item => item.SD_ITEM_ID == id);
                //删除主表
                var row = _itemService.Delete(id);
                return row > 0 ? Success("删除成功！") : Error("删除失败！");
            }
            return Success();
        }
        #endregion
        [HttpPost]
        public ActionResult GetForm(string primaryKey)
        {
            var id = int.Parse(primaryKey);
            var entity = _itemService.Get(item => item.SD_ITEM_ID == id);
            return Content(entity.ToJson());
        }
        [HttpGet]
        public ActionResult Form(int ItemCount = 1)
        {
            var sdId = ProjectProvider.Instance.Current.SD_ID;
            var models = _itemService.GetManay(r => r.SD_ID == sdId && r.ORDER_NO != null);
            var no = models.Select(i => i.ORDER_NO).OrderByDescending(i => i.Value).FirstOrDefault();
            if (no != null)
                ViewBag.ItemCount = no + 1;
            else
                ViewBag.ItemCount = 1;
            //var catCode = _projectService.GetCurrentSD().ITEM_CAT_PA_CODE;
            var catCode = _sdService.Get(s => s.SD_ID == sdId).ITEM_CAT_PA_CODE;
            ViewBag.ItemType = _catconfService.GetSearchList(s => s.CAT_PA_CODE == catCode);
            return View();
        }
        [HttpGet]
        public ActionResult Detail(int ItemCount = 1)
        {
            ViewBag.ItemCount = ItemCount;
            var sdId = ProjectProvider.Instance.Current.SD_ID;
            //var catCode = _projectService.GetCurrentSD().ITEM_CAT_PA_CODE;
            var catCode = _sdService.Get(s => s.SD_ID == sdId).ITEM_CAT_PA_CODE;
            ViewBag.ItemType = _catconfService.GetSearchList(s => s.CAT_PA_CODE == catCode);
            return View();
        }
        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Form(string strEnum, SD_ITEM_INFO model)
        {
            model.SD_ITEM_ALGO = HttpUtility.UrlDecode(model.SD_ITEM_ALGO, Encoding.Default);
            model.SD_ITEM_NAME = HttpUtility.UrlDecode(model.SD_ITEM_NAME, Encoding.Default);
            model.SD_ITEM_ALIAS = HttpUtility.UrlDecode(model.SD_ITEM_ALIAS, Encoding.Default);
            var row = 0;
            SD_ITEM_INFO entity = null;
            model.SD_ITEM_ALGO = model.SD_ITEM_ALGO.Replace("'", "\"");
            //保存或修改并更新到数据库
            var exist = _itemService.Exists(r => r.SD_ITEM_ID == model.SD_ITEM_ID);
            if (exist)
            {
                entity = _itemService.GetWithTrace(r => r.SD_ITEM_ID == model.SD_ITEM_ID);
                entity.SD_ITEM_CODE = model.SD_ITEM_CODE;
                entity.SD_ITEM_NAME = model.SD_ITEM_NAME;
                entity.SD_ITEM_ALIAS = model.SD_ITEM_ALIAS;
                entity.SD_ITEM_CAT_ID = model.SD_ITEM_CAT_ID;
                entity.SD_ITEM_DATA_TYPE = model.SD_ITEM_DATA_TYPE;
                entity.SD_ITEM_UNIT = model.SD_ITEM_UNIT;
                entity.NUM_PRECISION = model.NUM_PRECISION;
                entity.SD_ITEM_SRC = model.SD_ITEM_SRC;
                entity.IS_RESULT = model.IS_RESULT;
                entity.ORDER_NO = model.ORDER_NO;
                entity.SD_ITEM_ALGO = model.SD_ITEM_ALGO;
                entity.VALID_FLAG = model.VALID_FLAG;
                //entity.SD_EKPI_DESC = model.SD_EKPI_DESC;
                entity.IS_PUBLIC = model.IS_PUBLIC;
                row = _itemService.Update(entity);
            }
            else
            {
                SD_ITEM_INFO value = new SD_ITEM_INFO()
                {
                    SD_ITEM_CODE = _projectService.GetCurrentSD().SD_CODE + "_TEST",
                    SD_ITEM_NAME = model.SD_ITEM_NAME,
                    SD_ITEM_ALIAS = model.SD_ITEM_ALIAS,
                    SD_ITEM_CAT_ID = model.SD_ITEM_CAT_ID,
                    SD_ITEM_DATA_TYPE = model.SD_ITEM_DATA_TYPE,
                    SD_ITEM_UNIT = model.SD_ITEM_UNIT,
                    NUM_PRECISION = model.NUM_PRECISION,
                    SD_ITEM_SRC = 2,
                    IS_RESULT = model.IS_RESULT,
                    ORDER_NO = model.ORDER_NO,
                    SD_ITEM_ALGO = model.SD_ITEM_ALGO,
                    VALID_FLAG = model.VALID_FLAG,
                    //SD_EKPI_DESC = model.SD_EKPI_DESC,
                    IS_PUBLIC = 0,
                };
                entity = _itemService.Insert(value);
                model.SD_ITEM_ID = entity.SD_ITEM_ID;
            }
            if (!string.IsNullOrWhiteSpace(strEnum))
            {
                var enumList = JsonConvert.DeserializeObject<Dictionary<string, string>>(strEnum);//strEnum.Split(',');
                var index = 0;
                foreach (var enumV in enumList)
                {
                    var id = enumV.Key.Contains("new") ? 0 : int.Parse(enumV.Key);
                    var enumModel = new SD_ITEM_OPTION()
                    {
                        ITEM_OPTION_ID = id,
                        SD_ITEM_ID = model.SD_ITEM_ID,
                        ITEM_OPTION_NAME = enumV.Value,
                        ORDER_NO = index + 1
                    };
                    if (_itemOptionService.Exists(r => r.SD_ITEM_ID == model.SD_ITEM_ID && r.ITEM_OPTION_ID == id))
                        _itemOptionService.Update(enumModel);
                    else
                    {
                        _itemOptionService.Insert(enumModel);
                    }
                }
            }
            if (exist)
                return row > 0 ? Success() : Error();
            return entity != null ? Success() : Error();
        }

        public ActionResult IsHasTask(string clientKey)
        {
            var result = _itemService.IsHasTaskExe(clientKey);
            var dict = new Dictionary<string, string> { { "data", result.ToString() } };
            return Content(dict.ToJson());
        }
        #region 获取枚举列表
        /// <summary>
        /// 获取枚举列表
        /// </summary>
        /// <param name="primaryKey"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult GetEnumList(string primaryKey)
        {
            var id = int.Parse(primaryKey);
            var entity = _itemOptionService.GetSearchList(item => item.SD_ITEM_ID == id);
            return Content(entity.ToJson());
        }
        #endregion
        #region 批量导出数据项结果
        /// <summary>
        /// 批量导出数据项结果
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public ActionResult ExportValue(string data)
        {
            var itemIdList = data.Split(',').ToList();
            var listDic = _itemService.ExportAllDataItem(itemIdList);
            //var result = new Dictionary<string, List<Dictionary<string, string>>>();
            //var dic = listDic.GetRange(0, 1);
            //var value = listDic.GetRange(1, listDic.Count - 1);
            //result.Add("title", dic);
            //result.Add("data", value);
            return Content(listDic.ToJson());
        }
        public ActionResult ExportResult(string route, string data = null)
        {
            try
            {
                ViewBag.route = route;
                ViewBag.data = data;
                return View();
            }
            catch (Exception ex)
            {
                LogHelper.Error("导出结果:" + ex.ToString());
                throw;
            }
        }
        #endregion
        #region 批量执行数据项
        /// <summary>
        /// 批量执行数据项
        /// </summary>
        /// <param name="clientKey"></param>
        /// <param name="url"></param>
        /// <param name="ids"></param>
        /// <returns></returns>
        public ActionResult ExecuteAll(string clientKey, string url, string ids, string type)
        {
            var cpats = new List<UN_SD_CPATS>();
            var param = new TEST_PARAM_VIEWMODEL()
            {
                clientKey = clientKey,
                endTime = DateTime.MinValue,
                patientId = "",
                startTime = DateTime.MinValue,
                url = url.Trim()
            };
            if (!url.EndsWith("ManyDataItemApi"))
                return Error("url输入错误！接口应为：ManyDataItemApi");
            var itemIdList = ids.Split(',').ToList();
            var items = _itemService.GetManay(i => itemIdList.Contains(i.SD_ITEM_ID.ToString()));
            if (items.Count == 0) return null;
            _itemResultService.GetCpatList(items.FirstOrDefault().SD_ID.Value, param.startTime, param.endTime, param.patientId, ref cpats);
            if (cpats.Count == 0)
                return Info("没有找到符合条件的入组信息！");
            _itemService.ExcuteAllDataItem1(param.url, clientKey, items, param);
            return Success("正在执行数据项！");
        }
        #endregion 
        #region 有效标识——依赖条件设置（弃用）
        //public ActionResult IsSwitch(string primaryKey)
        //{
        //    var id = int.Parse(primaryKey);
        //    var data = new Dictionary<string, string> { { "state", "1" }, { "message", "" } };

        //    var item = _itemService.Get(x => x.SD_ITEM_ID == id);
        //    //判断数据项是否被依赖
        //    var depended = _itemDepService.GetSearchList(x => x.DEP_SD_ITEM_ID == id).Select(x => x.SD_ITEM_ID).ToList();
        //    var valDepended = _itemService.GetSearchList(i => depended.Contains(i.SD_ITEM_ID)).Where(i => i.VALID_FLAG == 1).ToList();
        //    if (item.IS_RESULT == 0)
        //    {
        //        data = new Dictionary<string, string> { { "state", "-1" }, { "message", "当前数据项非结果值，不能设为无效！" } };
        //        return Content(data.ToJson());
        //    }
        //    if (valDepended.Count > 0)
        //    {
        //        data = new Dictionary<string, string> { { "state", "-2" }, { "message", "当前数据项已被依赖，不能设为无效！" } };
        //        return Content(data.ToJson());
        //    }
        //    var depend = _itemDepService.GetSearchList(x => x.SD_ITEM_ID == id).Select(x => x.DEP_SD_ITEM_ID).ToList();
        //    var valDep =
        //        _itemService.GetSearchList(i => depend.Contains(i.SD_ITEM_ID)).Where(i => i.VALID_FLAG == 0).ToList();
        //    if (valDep.Count > 0)
        //    {

        //        data = new Dictionary<string, string> { { "state", "-3" }, { "message", "当前的依赖数据项存在无效数据项，是否将依赖数据项全部设置为有效？" } };
        //        return Content(data.ToJson());//Info("当前数据项存在依赖，是否将依赖数据项全部设置为无效！");
        //    }
        //    return Content(data.ToJson());
        //}
        #endregion
        #region 开关（有效标志修改）
        /// <summary>
        /// 开关（有效标志修改）
        /// </summary>
        /// <param name="primaryKey"></param>
        /// <param name="valueFlag"></param>
        /// <returns></returns>
        public ActionResult Switch(string primaryKey, string valueFlag)//, bool isDepend
        {
            var id = int.Parse(primaryKey);
            var entity = _itemService.Get(x => x.SD_ITEM_ID == id);
            //if (isDepend)
            //{
            //var dependList = _itemDepService.GetSearchList(x => x.SD_ITEM_ID == id).Select(item => item.DEP_SD_ITEM_ID);
            //var itemList = _itemService.GetSearchList(item => dependList.Contains(item.SD_ITEM_ID));
            //foreach (var dep in itemList)
            //{
            //    dep.VALID_FLAG = int.Parse(valueFlag) == 1 ? 0 : 1;
            //    _itemService.Update(dep);
            //}
            //}
            entity.VALID_FLAG = int.Parse(valueFlag) == 1 ? 0 : 1;
            var count = _itemService.Update(entity);
            return count > 0 ? Success() : Error();
        }
        #endregion
        #region 导出数据项脚本(弃用)
        ///// <summary>
        ///// 导出数据项脚本
        ///// </summary>
        ///// <returns></returns>
        //public ActionResult ExportSql()
        //{
        //    var sqlStr = _itemService.ExportSqlFile();
        //    var data = new Dictionary<string, string> { { "data", sqlStr } };
        //    return Content(data.ToJson());
        //}
        #endregion

        public ActionResult VerificationDataItem(string ids)
        {
            var itemIdList = ids.Split(',').ToList();
            var noExeCount = 0;
            itemIdList.ForEach(r =>
            {
                var sdId = _projectService.GetCurrentSD().SD_ID;
                var itemState = _procStateService.DataItemProcCatCode();
                var procState = _procStateService.Get(proc => proc.PROC_CONTENT_ID == r && proc.PROC_CAT_CODE == itemState && proc.SD_ID == sdId);
                if (procState.PROC_STAT_CODE != _procStateService.HadExedProcStatCode() &&
                    procState.PROC_STAT_CODE != _procStateService.LockProcStatCode())
                    noExeCount++;
            });
            return noExeCount > 0 ? Error($"当前所选数据项有{noExeCount}个状态为未执行或已清空，请选择是否继续锁定?") : Success();
        }
        public ActionResult LockDataAll(string clientKey, string ids,string type)
        {
            var itemIdList = ids.Split(',').ToList();
            Task.Factory.StartNew(() =>
            {
                double index = 0;
                itemIdList.ForEach(r =>
                {
                    index += 1;
                    var percent = Math.Round(index * 100 / itemIdList.Count, 2);
                    var primaryKey = int.Parse(r);
                    var model = _itemService.Get(i => i.SD_ITEM_ID == primaryKey);
                    var itemState = _procStateService.DataItemProcCatCode();
                    var sdId = _projectService.GetCurrentSD().SD_ID;
                    var procState = _procStateService.Get(proc => proc.PROC_CONTENT_ID == r && proc.PROC_CAT_CODE == itemState && proc.SD_ID == sdId);
                    if (procState.PROC_STAT_CODE != _procStateService.HadExedProcStatCode() && procState.PROC_STAT_CODE != _procStateService.LockProcStatCode())
                        ProcessHub.GetInstance().Send(clientKey, percent.ToString(), "批量锁定", $"数据项：{model.SD_ITEM_NAME} 信息：数据项未执行，无法锁定！！！", "");
                    else if (model.IS_RESULT == 0)
                        ProcessHub.GetInstance().Send(clientKey, percent.ToString(), "批量锁定", $"数据项：{model.SD_ITEM_NAME} 信息：非结果数据项，不需要锁定！！！", "");
                    else
                    {
                        var result = _itemValueLockService.LockData(primaryKey, model);
                        ProcessHub.GetInstance()
                            .Send(clientKey, percent.ToString(), "批量锁定", $"数据项：{model.SD_ITEM_NAME} 信息：{result.Item2}！！！", "");
                    }
                });
            });
            return View("ExecuteProgress"); ;
        }

        public ActionResult ClearDataAll(string clientKey, string ids, string type)
        {
            var itemIdList = ids.Split(',').ToList();
            Task.Factory.StartNew(() =>
            {
                double index = 0;
                itemIdList.ForEach(r =>
                {
                    index += 1;
                    var percent = Math.Round(index * 100 / itemIdList.Count, 2);
                    var primaryKey = int.Parse(r);
                    var model = _itemService.Get(i => i.SD_ITEM_ID == primaryKey);
                    var count = _itemResultService.GetUnitDataCount(primaryKey);
                    var result = _itemResultService.UnitClearData(primaryKey);
                    if (result == count)
                    {
                        //更新执行状态表
                        _procStateService.DataItemDoClear(model.SD_ID.Value, primaryKey.ToString());
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
                    ProcessHub.GetInstance()
                        .Send(clientKey, percent.ToString(), "清库", $"数据项：{model.SD_ITEM_NAME}清库成功，共清除 {result}条！！！", "");
                });
            });
            return View("ExecuteProgress"); ;
        }
    }
}