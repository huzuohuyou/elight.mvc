using Elight.Entity;
using Elight.Infrastructure;
using Elight.Web.Controllers;
using Elight.Web.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using TestingCenterSystem.Models.PDP;
using TestingCenterSystem.Service.DataItem;
using TestingCenterSystem.Service.DataItem.Interface;
using TestingCenterSystem.Service.InGroup;
using TestingCenterSystem.Service.InGroup.Interface;
using TestingCenterSystem.Service.KPI;
using TestingCenterSystem.Service.KPI.Interface;
using TestingCenterSystem.Service.Project;
using TestingCenterSystem.Service.Project.Interface;
using TestingCenterSystem.ViewModels.Comm;
using TestingCenterSystem.ViewModels.DataItem;
using TestingCenterSystem.ViewModels.Project;

namespace Elight.Web.Areas.TestCenterSystem.Controllers
{
    [LoginChecked]
    public class EDRDataCompareController : BaseController
    {
        private static readonly IUnitKPIValueLockService _unitValueLockService = new UnitKPIValueLockService();
        private static readonly ISDCDRInfoService _sdCDRInfoService = new SDCDRInfoService();
        private static readonly IDataItemService _itemService = new DataItemService();
        private static readonly IDataItemCompareService _itemCompareService = new DataItemCompareService();
        private static readonly IInGroupResultCompareService _inGroupResultCompareService = new InGroupResultCompareService();
        public ActionResult Index()
        {
            var projId = ProjectProvider.Instance.Current.TC_PROJ_ID;
            var listTree = new List<TreeSelect>();
            _sdCDRInfoService.GetManay(r => r.PROJECT_ID == projId).ForEach(cdr => listTree.Add(new TreeSelect()
            {
                id = cdr.CDR_ID.ToString(),
                text = cdr.CDR_NAME
            }));
            ViewBag.CDRNames = listTree;
            return View();
        }

        public ActionResult CompareInGroup(DataCompareParamViewModel para)
        {
            var compareResult = _inGroupResultCompareService.GetIngroupCompareResult(para);//_itemCompareService.GetCompareResult(cdrId1, cdrId2, startTime, endTime, itemInfoList);
            var result = new Dictionary<string, object>() {
                { "code", 0},
                { "msg","success"},
                { "data", compareResult},
                { "count",compareResult.Count}
            };
            return Content(result.ToJson());
        }

        public ActionResult InGroupResultDetail(string route)
        {
            ViewBag.route = new MvcHtmlString(route);//表示不应再次进行编码的 HTML 编码的字符串
            return View();
        }

        public ActionResult CompareDataItem(int cdrId1, int cdrId2, string startTime, string endTime)
        {
            var sdId = ProjectProvider.Instance.Current.SD_ID;
            var itemInfoList = _itemService.GetManay(m => m.SD_ID == sdId);
            var compareResult = _itemCompareService.GetCompareResult(cdrId1, cdrId2, startTime, endTime, itemInfoList);
            var result = new Dictionary<string, object>() {
                { "code", 0},
                { "msg","success"},
                { "data", compareResult},
                { "count",compareResult.Count}
            };
            return Content(result.ToJson());
        }

        /// <summary>
        /// 获取入组比较结果
        /// </summary>
        /// <param name="para"></param>
        /// <param name="flag">0:cpats；1:cpatdetail(住院)；2:cpatdetail(门诊)；3:cpatdetail(急诊)</param>
        /// <returns></returns>
        public ActionResult GetCpatsOrDetailsCompareValues(DataCompareParamViewModel para, int flag)
        {
            var pageData = _inGroupResultCompareService.GetCpatsOrDetailsCompareValues(para, flag);
            if (pageData.Item1)
            {
                var data = (Tuple<List<DataCompareResultViewModel>, double, double, double>)pageData.Item2;
                return Content((new Tuple<bool, Tuple<List<DataCompareResultViewModel>, double, double, double>, string, string>(true, data, para.cdrName1, para.cdrName2)).ToJson());
            }
            else
                return Content(pageData.ToJson());
        }

        public ActionResult ExportIngroupCompareResult(DataCompareParamViewModel para)
        {
            var data = new Dictionary<string, object>();
            try
            {
                var result = _inGroupResultCompareService.GetIngroupCompareResult(para);
                if (result.Count == 0) return Content(new TestingCenterSystem.ViewModels.InGroup.COMPARE_RESULT_MODEL().ToJson());
                var title = new
                {
                    INGROUP_TYPE_ID = "入组结果类别ID",
                    INGROUP_TYPE = "入组结果类别",
                    LEFT_CDR_COUNT = $"{para.cdrName1}库数量",
                    RIGHT_CDR_COUNT = $"{para.cdrName2}库数量",
                    IS_DIFFERENT = "是否差异",
                };
                data.Add("title", new List<object>() { title });
                data.Add("data", result);
            }
            catch (Exception ex)
            {
                data.Add("data", null);
            }
            return Content(data.ToJson());
        }
        public ActionResult DataItemResultDetail(string route, string cdr1Name, string cdr2Name, int id)
        {
            ViewBag.route = new MvcHtmlString(route);//表示不应再次进行编码的 HTML 编码的字符串
            ViewBag.cdr1Name = cdr1Name;
            ViewBag.cdr2Name = cdr2Name;
            var itemInfo = _itemService.Get(i => i.SD_ITEM_ID == id);
            ViewBag.itemName = itemInfo.SD_ITEM_NAME;
            return View();
        }
        public ActionResult ExportDataItem(int cdrId1, int cdrId2, string startTime, string endTime, string cdr1Name, string cdr2Name)
        {
            var data = new Dictionary<string, List<COMPARE_RESULT_MODEL>>();
            try
            {
                var itemList = _itemService.GetManay(i => i.SD_ID == ProjectProvider.Instance.Current.SD_ID);
                var result = _itemCompareService.GetCompareResult(cdrId1, cdrId2, startTime, endTime, itemList);
                if (result.Count == 0) return Content(new COMPARE_RESULT_MODEL().ToJson());
                var title = new COMPARE_RESULT_MODEL()
                {
                    SD_ITEM_ID = "数据项ID",
                    SD_ITEM_CODE = "数据项代码",
                    SD_ITEM_NAME = "数据项名称",
                    IS_DIFFERENT = "是否差异",
                    LEFT_CDR_COUNT = $"{cdr1Name}库数量",
                    RIGHT_CDR_COUNT = $"{cdr2Name}库数量"
                };
                data.Add("title", new List<COMPARE_RESULT_MODEL>() { title });
                data.Add("data", result);
            }
            catch (Exception ex)
            {
                data.Add("data", null);
            }
            return Content(data.ToJson());
        }

        public ActionResult DataItemResult(int cdrId1, int cdrId2, string startTime, string endTime, string id)
        {
            int itemId = 0;
            if (!string.IsNullOrWhiteSpace(id))
                itemId = int.Parse(id);
            var result = _itemCompareService.GetCompareResultDetail(cdrId1, cdrId2, startTime, endTime, itemId);
            if (result == null) return Error("对比失败！");
            if (result.Count == 0) return Content(new COMPARE_DETAIL_MODEL().ToJson());
            return Content(result.ToJson());
        }

        public ActionResult CompareKpi(int leftCdrid, int rightCdrId)
        {
            var pageData = _unitValueLockService.GetCompareResult(leftCdrid, rightCdrId);//.GetPage(pageIndex, pageSize, keyWord);
            var result = new Dictionary<string, object>()
            {
                { "code", 0},
                { "msg","success"},
                { "data", pageData},
                { "count", pageData.Count}
            };
            return Content(result.ToJson());
        }
    }
}

