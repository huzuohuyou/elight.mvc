using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Elight.Infrastructure;
using Elight.Web.Controllers;
using Elight.Web.Filters;
using TestingCenterSystem.Models.PDP;
using TestingCenterSystem.Models.Unit;
using TestingCenterSystem.Service.comm.Interface;
using TestingCenterSystem.Service.DataItem;
using TestingCenterSystem.Service.DataItem.Interface;
using TestingCenterSystem.Service.InGroup;
using TestingCenterSystem.Service.SDataItem;
using TestingCenterSystem.Service.SDataItem.Interface;
using TestingCenterSystem.Service.SKPI.Interface;
using TestingCenterSystem.Utils.Hubs;
using TestingCenterSystem.ViewModels.DataItem;
using TestingCenterSystem.ViewModels.Project;
using TestingCenterSystem.ViewModels.SDataItem;

namespace Elight.Web.Areas.TestCenterSystem.Controllers
{
    [LoginChecked]
    public class SDataItemController : BaseController
    {
        private readonly ISDataItemService _sDataItemService = new SDataItemService();
        private readonly IProcStateService _procStateService = new ProcStateService();
        private readonly SItemResultService _itemResultService = new SItemResultService();
        private readonly SKpiPNOService _sKpiPnoService = new SKpiPNOService();
        private readonly SPNOService _spnoService = new SPNOService();
        [HttpGet]
        public ActionResult Index()
        {
            return View();
        }
        [HttpGet]
        public ActionResult GetList(int page, int limit, string field, string type, string keyWord = "")
        {
            var pageData = _sDataItemService.GetPage(page, limit, field, type, keyWord);
            var result = new Dictionary<string, object>() {
                { "code", 0},
                { "msg","success"},
                { "data", pageData.Items},
                { "count", pageData.TotalItems}
            };
            return Content(result.ToJson());
        }
        [HttpGet]
        public ActionResult Detail()
        {
            return View();
        }
        [HttpPost]
        public ActionResult GetForm(string primaryKey)
        {
            var id = int.Parse(primaryKey);
            var entity = _sDataItemService.Get(item => item.SITEM_ID == id);
            return Content(entity.ToJson());
        }
        [HttpGet]
        public ActionResult Form()
        {
            return View();
        }
        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Form(SD_SITEM_INFO model)
        {
            var row = 0;
            SD_SITEM_INFO entity = null;
            //保存或修改并更新到数据库
            var exist = _sDataItemService.Exists(r => r.SITEM_ID == model.SITEM_ID);
            if (exist)
                row = _sDataItemService.Update(model);
            else
            {
                model.SITEM_CODE = ProjectProvider.Instance.Current.SD_CODE + "_TEST";
                entity = _sDataItemService.Insert(model);
            }
            if (exist)
                return row > 0 ? Success() : Error();
            return entity != null ? Success() : Error();
        }
        [HttpPost]
        public ActionResult Delete(string primaryKey)
        {
            var id = int.Parse(primaryKey);
            if (_sDataItemService.Exists(r => r.SITEM_ID == id))
            {
                var catCode = _procStateService.SDataItemProcCatCode();
                var procState = _procStateService.Get(
                    item => item.PROC_CAT_CODE == catCode && item.PROC_CONTENT_ID == primaryKey.Trim());
                if (procState != null)
                {
                    //var kpiCount = _kpiParamService.GetManay(param => param.SITEM_ID == id).Count;
                    //if (kpiCount > 0)
                    //    return Error("已被评价指标当作参数使用，不可删除！");
                }
                //清除单元库
                _itemResultService.ClearData(id);
                //应先删除关联表  
                _procStateService.Delete(item => item.PROC_CAT_CODE == catCode && item.PROC_CONTENT_ID == primaryKey.Trim());
                //删除主表
                var row = _sDataItemService.Delete(i => i.SITEM_ID == id);
                return row > 0 ? Success("删除成功！") : Error("删除失败！");
            }
            return Success();
        }

        #region 批量执行数据项
        /// <summary>
        /// 批量执行数据项
        /// </summary>
        /// <param name="clientKey"></param>
        /// <param name="url"></param>
        /// <param name="ids"></param>
        /// <returns></returns>
        public ActionResult ExecuteAll(string clientKey, string url, string ids)
        {
            var param = new TEST_PARAM_VIEWMODEL()
            {
                clientKey = clientKey,
                endTime = DateTime.MinValue,
                patientId = "",
                startTime = DateTime.MinValue,
                url = url.Trim()
            };
            var itemIdList = ids.Split(',').ToList();
            var items = _sDataItemService.GetManay(i => itemIdList.Contains(i.SITEM_ID.ToString()));
            _sDataItemService.ExeAllItem(param.url, clientKey, items, param);
            return Success("正在执行数据项！");
        }
        #endregion

        public ActionResult ExportAllData(string idList)
        {
            var itemIdList = idList.Split(',').ToList();
            var itemList = _sDataItemService.GetManay(i => itemIdList.Contains(i.SITEM_ID.ToString()));
            var data = new Dictionary<string, object> { { "overflow", "" } };
            var itemValueList = new Dictionary<int, List<UN_SPNO>>();
            var showType = new Dictionary<string, string>();
            var title = new Dictionary<string, string>();
            var countList = itemIdList.Select(int.Parse).ToDictionary(id => id, id => _spnoService.Count(i => i.SITEM_ID == id));
            var count = countList.Select(item => item.Value).Concat(new[] { 0 }).Max();
            //数据量过大
            if (count > 200000)
            {
                var itemId = 0;
                foreach (var itemValue in countList.Where(itemValue => itemValue.Value == count))
                {
                    itemId = itemValue.Key;
                }
                var itemName = _sDataItemService.Get(i => i.SITEM_ID == itemId).SITEM_NAME;
                data.Add("data", null);
                data["overflow"] = $"{itemName}数据量太大，无法导出！";
                return Content(data.ToJson());
            }
            //正常导出
            foreach (var id in itemIdList.Select(itemId => int.Parse(itemId)))
            {
                showType.Add($"SITEM_NAME_{id}", itemList.Where(i => i.SITEM_ID == id).FirstOrDefault().SITEM_NAME);
                title.Add($"PATIENT_NO_{id}", "PATIENT_NO");
                title.Add($"ACTUAL_VALUE_{id}", "结果值");
                itemValueList.Add(id, _spnoService.GetManay(i => i.SITEM_ID == id).ToList());
            }
            var resList = new List<Dictionary<string, string>>();
            for (var index = 0; index < count; index++)
            {
                var row = (Dictionary<string, string>)Clone(title);
                foreach (var id in itemIdList.Select(itemId => int.Parse(itemId)))
                {
                    if (itemValueList[id].Count > index)
                    {
                        row[$"PATIENT_NO_{id}"] = itemValueList[id][index].PATIENT_NO;
                        row[$"ACTUAL_VALUE_{id}"] = itemValueList[id][index].ACTUAL_VALUE.ToString();
                    }
                    else
                    {
                        row[$"PATIENT_NO_{id}"] = "";
                        row[$"ACTUAL_VALUE_{id}"] = "";
                    }
                }
                resList.Add(row);
            }
            data.Add("showType", new List<Dictionary<string, string>>() { showType });
            data.Add("title", new List<Dictionary<string, string>>() { title });
            data.Add("data", resList);
            return Content(data.ToJson());
        }
        public ActionResult Switch(string primaryKey, string valueFlag)
        {
            var id = int.Parse(primaryKey);
            var entity = _sDataItemService.Get(x => x.SITEM_ID == id);
            entity.VALID_FLAG = int.Parse(valueFlag) == 1 ? 0 : 1;
            var count = _sDataItemService.Update(entity);
            return count > 0 ? Success() : Error();
        }
        public ActionResult IsHasTask(string clientKey)
        {
            var result = _sDataItemService.IsHasTaskExe(clientKey);
            var dict = new Dictionary<string, string> { { "data", result.ToString() } };
            return Content(dict.ToJson());
        }
        public object Clone(Dictionary<string, string> dic)
        {
            BinaryFormatter Formatter = new BinaryFormatter(null, new StreamingContext(StreamingContextStates.Clone));
            MemoryStream stream = new MemoryStream();
            Formatter.Serialize(stream, dic);
            stream.Position = 0;
            object clonedObj = Formatter.Deserialize(stream);
            stream.Close();
            return clonedObj;
        }

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
        public ActionResult ClearAllData(string clientKey, string idList, string type)
        {
            var itemIdList = idList.Split(',').ToList();
            Task.Factory.StartNew(() =>
            {
                double index = 0;
                itemIdList.ForEach(r =>
                {
                    index += 1;
                    var percent = Math.Round(index * 100 / itemIdList.Count, 2);
                    var primaryKey = int.Parse(r);
                    var model = _sDataItemService.Get(i => i.SITEM_ID == primaryKey);
                    //var count = _sKpiPnoService.Count(i => i.SITEM_ID == primaryKey);
                    var result = _itemResultService.ClearData(primaryKey);
                    //if (result == count)
                    //{
                    //更新执行状态表
                    _procStateService.DataItemDoClear(int.Parse(model.SD_ID), primaryKey.ToString());
                    //更新执行日志表
                    var proLog = new PDP_PROC_LOG()
                    {
                        SD_ID = int.Parse(model.SD_ID),
                        PROC_CAT_CODE = _procLogService.DataItemCatCode(),
                        PROC_CONTENT_ID = model.SITEM_ID.ToString(),
                        PATIENT_ID = "",
                        PROC_URL = "",
                        PROC_STAT_CODE = 3
                    };
                    _procLogService.Insert(proLog);
                    //更新数据项
                    _sDataItemService.Update(model);
                    //}
                    ProcessHub.GetInstance()
                        .Send(clientKey, percent.ToString(), "清库", $"统计类数据项：{model.SITEM_NAME}清库成功，共清除 {result}条！！！", "");
                });
            });
            return View("ExecuteProgress");
        }

    }
}