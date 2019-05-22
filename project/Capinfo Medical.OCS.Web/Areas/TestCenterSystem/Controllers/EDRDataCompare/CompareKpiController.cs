using Elight.Infrastructure;
using Elight.Web.Controllers;
using Elight.Web.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TestingCenterSystem.Service.KPI;
using TestingCenterSystem.Service.KPI.Interface;
using TestingCenterSystem.Service.Project;
using TestingCenterSystem.Service.Project.Interface;
using TestingCenterSystem.ViewModels.KPI;

namespace Elight.Web.Areas.TestCenterSystem.Controllers.EDRDataCompare
{
    [LoginChecked]
    public class CompareKpiController : BaseController
    {
        private static  IUnitKPIValueLockService _unitValueLockService ;
        private static readonly ISDCDRInfoService _sdCDRInfoService = new SDCDRInfoService();
        // GET: TestCenterSystem/CompareKpi
        public ActionResult Index()
        {
            return View();
        }

        
        public ActionResult CompareKpi(int leftCdrid, int rightCdrId, string startTime, string endTime)
        {
            //DateTime startTime = DateTime.MinValue; DateTime endTime=DateTime.MaxValue;
               _unitValueLockService = new UnitKPIValueLockService(startTime, endTime);
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

        public ActionResult ExportDetail(int leftCdrid, int rightCdrId, int kpiId, string startTime, string endTime)
        {
            try
            {
                var cdrCaches = _sdCDRInfoService.GetManay(r => 1 == 1);
                ViewBag.cdr1 = cdrCaches.FirstOrDefault(r => r.CDR_ID == leftCdrid).CDR_NAME;
                ViewBag.cdr2 = cdrCaches.FirstOrDefault(r => r.CDR_ID == rightCdrId).CDR_NAME;
                ViewBag.route = $"/TestCenterSystem/CompareKpi/GetDetail?unionParam={leftCdrid.ToString()+"-"+ rightCdrId.ToString()+"-"+kpiId.ToString() + "-" + startTime.ToString() + "-" + endTime.ToString()}";
                return View("ValueDetail");
            }
            catch (Exception ex)
            {
                LogHelper.Error("ExportDetail:" + ex.ToString());
                throw;
            }

        }


        public ActionResult GetDetail(string unionParam)//(int leftCdrid, int rightCdrId, int kpiId)
        {
            try
            {
                var leftCdrid = int.Parse(unionParam.Split('-')[0]);
                var rightCdrId = int.Parse(unionParam.Split('-')[1]);
                var kpiId = int.Parse(unionParam.Split('-')[2]);
                var startTime = unionParam.Split('-')[3];
                var endTime = unionParam.Split('-')[4];
                //DateTime.TryParse(unionParam.Split('-')[3], out startTime);
                //DateTime.TryParse(unionParam.Split('-')[4], out endTime);
                _unitValueLockService = new UnitKPIValueLockService(startTime, endTime);
                var result = _unitValueLockService.CompareDetail(leftCdrid, rightCdrId, kpiId);
                var cdrCaches = _sdCDRInfoService.GetManay(r => 1 == 1);
                var cdr1 = cdrCaches.FirstOrDefault(r => r.CDR_ID == leftCdrid).CDR_NAME;
                var cdr2 = cdrCaches.FirstOrDefault(r => r.CDR_ID == rightCdrId).CDR_NAME;
                return Content(result.ToJson().Replace("左CAPTNO",$"{ cdr1}（CPATNO）").Replace("右CAPTNO", $"{ cdr2}（CPATNO）").Replace("CDR1库值", $"{ cdr1}（值）").Replace("CDR2库值",$"{ cdr2}（值）"));
            }
            catch (Exception e)
            {
                return Error(e.ToString());
            }
        }

        public ActionResult ExportKPICompareResult(int cdrId1, int cdrId2, string startTime, string endTime, string cdr1Name, string cdr2Name)
        {
            var data = new Dictionary<string, List<KPI_COMPARE_FOR_DOWNLOAD_VIEWMODEL>>();
            try
            {
                _unitValueLockService = new UnitKPIValueLockService(startTime, endTime);
                var temp = _unitValueLockService.GetCompareResult(cdrId1, cdrId2);// _itemCompareService.GetCompareResult(cdrId1, cdrId2, startTime, endTime, itemList);
                if (temp.Count == 0) return Content(new KPI_COMPARE_FOR_DOWNLOAD_VIEWMODEL().ToJson());
                var title = new KPI_COMPARE_FOR_DOWNLOAD_VIEWMODEL
                {
                    指标ID = "指标ID",
                    指标名称 = "指标名称",
                    左分子 = "左分子",
                    左分母 = "左分母",
                    左比率 = "左比率",
                    右分子 = "右分子",
                    右分母 = "右分母",
                    右比率 = "右比率",
                    差异 = "差异"
                };
                var result = new List<KPI_COMPARE_FOR_DOWNLOAD_VIEWMODEL>();
                temp.ForEach(r =>
                result.Add(new KPI_COMPARE_FOR_DOWNLOAD_VIEWMODEL(r))
                );
                data.Add("title", new List<KPI_COMPARE_FOR_DOWNLOAD_VIEWMODEL>() { title });
                data.Add("data", result);
            }
            catch (Exception ex)
            {
                data.Add("data", null);
            }
            return Content(data.ToJson());
        }

        public ActionResult ExportReport(int leftCdrid, int rightCdrId, string startTime, string endTime)
        {
            try
            {
                
                ViewBag.route = $"/TestCenterSystem/CompareKpi/GetUnionReport?unionParam={leftCdrid.ToString() + "-" + rightCdrId.ToString() + "-" + startTime.ToString() + "-" + endTime.ToString()}";
                return View("ValueDetail");
            }
            catch (Exception ex)
            {
                LogHelper.Error("ExportDetail:" + ex.ToString());
                throw;
            }

        }


        public ActionResult GetUnionReport(string unionParam)//(int leftCdrid, int rightCdrId)
        {
            try
            {
                var leftCdrid = int.Parse(unionParam.Split('-')[0]);
                var rightCdrId = int.Parse(unionParam.Split('-')[1]);
                var startTime = unionParam.Split('-')[2];
                var endTime = unionParam.Split('-')[3];
                //DateTime.TryParse(unionParam.Split('-')[2], out startTime);
                //DateTime.TryParse(unionParam.Split('-')[3], out endTime);
                _unitValueLockService = new UnitKPIValueLockService(startTime, endTime);
                var result = _unitValueLockService.GetCompareResult(leftCdrid, rightCdrId);//.CompareDetail(leftCdrid, rightCdrId, kpiId);

                return Content(result.ToJson());
            }
            catch (Exception e)
            {
                return Error(e.ToString());
            }
        }
    }
}