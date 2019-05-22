using Elight.Entity;
using Elight.Infrastructure;
using Elight.Web.Controllers;
using Elight.Web.Filters;
using System.Web.Mvc;
using TestingCenterSystem.Service.KPI;
using TestingCenterSystem.ViewModels.KPI;
using TestingCenterSystem.Service.KPI.Interface;
using System.Collections.Generic;
using TestingCenterSystem.Service.comm.Interface;
using TestingCenterSystem.Service.Project.Interface;
using TestingCenterSystem.Service.InGroup;
using TestingCenterSystem.Service.Project;
using TestingCenterSystem.Models.PDP;
using System;
using TestingCenterSystem.ViewModels.Project;
using TestingCenterSystem.Service.Comm.Interface;
using TestingCenterSystem.Service.Comm;
using System.Linq;
using System.Threading;
using TestingCenterSystem.Utils.Hubs;
using TestingCenterSystem.Service.DataItem.Interface;
using TestingCenterSystem.Service.DataItem;
using System.Dynamic;
using TestingCenterSystem.Service.InGroup.Interface;
using System.Threading.Tasks;
using HJ.PDP.IService.KPI;
using Newtonsoft.Json;
using TestingCenterSystem.Models.Unit;
using TestingCenterSystem.Service.SDataItem.Interface;
using TestingCenterSystem.Service.SDataItem;

namespace Elight.Web.Areas.TestCenterSystem.Controllers
{
    [LoginChecked]
    public class KPIController : BaseController
    {

        private static readonly ICatConfService _catconfService = new CatConfService();
        private readonly IKPIService _kpiService = new KPIService(r => r.SD_ID == ProjectProvider.Instance.Current.SD_ID, x => x.ORDER_NO);
        private static readonly IKPIParamService _kpiparamService = new KPIParamService();
        private static readonly IKPISetService _kpisetService = new KPISetService();
        private static readonly IProcStateService _procStateService = new ProcStateService();
        private static readonly IProjectService _projectService = new ProjectService();
        private static readonly IKPIValueService _kpiValueService = new KPIValueService();
        private static readonly IUnitKpiValueService _unitKpiValueService = new UnitKPIValueService();
        private static readonly IUnitResultDetailService _unitCpatDetailService = new UnitResultDetail();
        private static readonly ISDataItemService _sdataitemService = new SDataItemService();
        private static IOrderNoService<SD_EKPI_INFO> _orderServie = new OrderNoService<SD_EKPI_INFO>(r => r.SD_ID == ProjectProvider.Instance.Current.SD_ID, x => x.ORDER_NO);
        private static readonly IDataItemService _itemService = new DataItemService();
        private static readonly IEkpiItemService _ekpiItemService = new EkpiItemService();
        //private ICanStoreKpiValue _storeKPIResultService = null;
        //private ICanStoreKpiValue _unitKpiValueLockService = null;
        //Mutex instance = null;
        [HttpGet, AuthorizeChecked]
        public ActionResult Index()
        {
            var SD_ID = ProjectProvider.Instance.Current.SD_ID;
            var EKPI_CAT_PA_CODE = _sdService.Get(r => r.SD_ID == SD_ID).EKPI_CAT_PA_CODE;
            ViewBag.ItemType = _catconfService.GetSearchList(s => s.CAT_PA_CODE == EKPI_CAT_PA_CODE);
            return View();
        }

        public ActionResult Index2()
        {
            var SD_ID = ProjectProvider.Instance.Current.SD_ID;
            var sd = _sdService.Get(r => r.SD_ID == SD_ID);
            var EKPI_CAT_PA_CODE = (sd == null) ? "" : sd.EKPI_CAT_PA_CODE;
            ViewBag.ItemType = _catconfService.GetSearchList(s => s.CAT_PA_CODE == EKPI_CAT_PA_CODE);
            return View();
        }

        [HttpPost, AuthorizeChecked]
        public ActionResult Index(int pageIndex, int pageSize, string keyWord)
        {
            var pageData = _kpiService.GetPage(pageIndex, pageSize, keyWord);
            var result = new LayPadding<KPI_VIEWMODEL>()
            {
                result = true,
                msg = "success",
                list = pageData.Items,
                count = pageData.TotalItems
            };
            var SD_ID = ProjectProvider.Instance.Current.SD_ID;
            var sd = _sdService.Get(r => r.SD_ID == SD_ID);
            var EKPI_CAT_PA_CODE = (sd == null) ? "" : sd.EKPI_CAT_PA_CODE;
            ViewBag.ItemType = _catconfService.GetSearchList(s => s.CAT_PA_CODE == EKPI_CAT_PA_CODE);
            return Content(result.ToJson());
        }

        [HttpGet]
        public ActionResult GetList(int page, int limit, string field, string type, string keyWord = "")
        {
            var temp = _kpiService.GetPage(page, limit, field, type, keyWord);
            var result = new Dictionary<string, object>() {
                { "code", 0},
                { "msg","success"},
                { "data", temp.Item2},
                { "count", temp.Item1}
            };
            return Content(result.ToJson());
        }

        [HttpGet, AuthorizeChecked]
        public ActionResult Form(string primaryKey)
        {
            var sdid = ProjectProvider.Instance.Current.SD_ID;
            var models = _kpiService.GetManay(r => r.SD_ID == sdid);
            var count = models.Select(i => i.ORDER_NO).OrderByDescending(i => i.Value).FirstOrDefault();
            if (count == null)
            {
                ViewBag.count = 1;
            }
            else if (string.IsNullOrEmpty(primaryKey))
            {
                ViewBag.count = count + 1;
            }
            else
            {
                ViewBag.count = count;
            }
            var SD_ID = ProjectProvider.Instance.Current.SD_ID;
            var sd = _sdService.Get(r => r.SD_ID == SD_ID);
            var EKPI_CAT_PA_CODE = (sd == null) ? "" : sd.EKPI_CAT_PA_CODE;
            ViewBag.ItemType = _catconfService.GetSearchList(s => s.CAT_PA_CODE == EKPI_CAT_PA_CODE);
            return View();
        }

        public ActionResult Form(SD_EKPI_INFO_VIEWMODEL model)
        {
            var sdId = ProjectProvider.Instance.Current.SD_ID;
            if (_kpiService.Exists(r => r.SD_EKPI_ID == model.SD_EKPI_ID))
            {
                var entity = _kpiService.GetWithTrace(r => r.SD_EKPI_ID == model.SD_EKPI_ID);
                entity.SD_EKPI_CODE = model.SD_EKPI_CODE;
                entity.SD_EKPI_NAME = model.SD_EKPI_NAME;
                entity.SD_EKPI_ALIAS = model.SD_EKPI_ALIAS;
                entity.ORDER_NO = model.ORDER_NO;
                entity.SD_EKPI_CAT = model.SD_EKPI_CAT;
                entity.IS_POSITIVE = model.IS_POSITIVE;
                entity.SD_EKPI_ALGO = model.SD_EKPI_ALGO.Replace("'", "”");
                entity.SD_EKPI_DESC = model.SD_EKPI_DESC == null ? null : model.SD_EKPI_DESC.Replace("'", "”");
                entity.VALID_FLAG = model.VALID_FLAG;
                entity.IS_RSN = (model.IS_RSN == true) ? 1 : 0;
                entity.IS_DISTRIBUTION = (model.IS_DISTRIBUTION == true) ? 1 : 0;
                entity.IS_TREND = (model.IS_TREND == true) ? 1 : 0;
                entity.NUM_PRECISION = model.NUM_PRECISION;
                entity.SD_EKPI_TYPE = model.SD_EKPI_TYPE;
                entity.IS_NUM = model.IS_NUM;
                entity.VALUE_TABLE_ID = model.VALUE_TABLE_ID;// ProjectProvider.Instance.Current.SD_TYPE_CODE==0?1: model.SD_EKPI_TYPE;
                entity.IS_DYNAMIC = model.IS_DYNAMIC;
                entity.SD_EKPI_CONVER = model.SD_EKPI_CONVER;
                entity.SD_EKPI_UNIT = model.SD_EKPI_UNIT;
                entity.SD_EKPI_PLATONAME = model.SD_EKPI_PLATONAME;
                var row = _kpiService.Update(entity);
                //先清除参数
                _ekpiItemService.ExDelete(i => i.SD_EKPI_ID == model.SD_EKPI_ID);
                if (!string.IsNullOrWhiteSpace(model.SD_EKPI_ITEM))
                {
                    //更新显示参数
                    var sdEkpiItem = JsonConvert.DeserializeObject<List<SD_EKPI_ITEM>>(model.SD_EKPI_ITEM);
                    foreach (var item in sdEkpiItem)
                    {
                        item.SD_EKPI_ID = model.SD_EKPI_ID;
                        item.UPD_DATE = DateTime.Now;
                        item.UPD_USER_ID = OperatorProvider.Instance.Current.UserId;
                    }
                    _ekpiItemService.BulkInsert(sdEkpiItem);
                }
                //_kpiService.RefreshOrder();
                return row > 0 ? Success() : Error();
            }
            else
            {
                SD_EKPI_INFO value = new SD_EKPI_INFO()
                {
                    SD_EKPI_ID = model.SD_EKPI_ID,
                    SD_ID = sdId,
                    SD_CODE = ProjectProvider.Instance.Current.SD_CODE,
                    SD_EKPI_CODE = _projectService.GetCurrentSD().SD_CODE + "_TEST",////model.SD_EKPI_CODE,
                    SD_EKPI_NAME = model.SD_EKPI_NAME,
                    SD_EKPI_ALIAS = model.SD_EKPI_ALIAS,
                    ORDER_NO = model.ORDER_NO,
                    SD_EKPI_CAT = model.SD_EKPI_CAT,
                    IS_POSITIVE = model.IS_POSITIVE,
                    SD_EKPI_ALGO = model.SD_EKPI_ALGO.Replace("'", "“"),
                    SD_EKPI_DESC = model.SD_EKPI_DESC == null ? null : model.SD_EKPI_DESC.Replace("'", "”"),
                    VALID_FLAG = 0,
                    IS_RSN = (model.IS_RSN == true) ? 1 : 0,
                    IS_DISTRIBUTION = (model.IS_DISTRIBUTION == true) ? 1 : 0,
                    IS_TREND = (model.IS_TREND == true) ? 1 : 0,
                    NUM_PRECISION = model.NUM_PRECISION,
                    SD_EKPI_TYPE = model.SD_EKPI_TYPE,
                    IS_NUM = model.IS_NUM,
                    IS_DYNAMIC = model.IS_DYNAMIC,
                    SD_EKPI_PLATONAME = model.SD_EKPI_PLATONAME,
                    VALUE_TABLE_ID = model.VALUE_TABLE_ID,// ProjectProvider.Instance.Current.SD_TYPE_CODE == 0 ? 1 : model.SD_EKPI_TYPE,
                    SD_EKPI_CONVER = model.SD_EKPI_CONVER,
                    SD_EKPI_UNIT = model.SD_EKPI_UNIT
                };

                var entity = _kpiService.Insert(value);
                entity.SD_EKPI_CODE = entity.SD_EKPI_CODE.Replace("TEST", entity.SD_EKPI_ID.ToString());
                _kpiService.Update(entity);
                _procStateService.KPINonExcute(sdId, entity.SD_EKPI_ID.ToString());
                _procLogService.Insert(new PDP_PROC_LOG()
                {
                    PROC_CONTENT_ID = entity.SD_EKPI_ID.ToString(),
                    SD_ID = sdId,
                    PROC_CAT_CODE = "3",
                    PROC_STAT_CODE = 1,
                });
                //编码由病种编码+ID组成
                //entity.SD_EKPI_CODE = $"{entity.SD_CODE}_{entity.SD_EKPI_ID}";
                //var row = _kpiService.Update(entity);
                //_kpiService.RefreshOrder();

                //更新展示参数
                if (!string.IsNullOrWhiteSpace(model.SD_EKPI_ITEM))
                {
                    var sdEkpiItem = JsonConvert.DeserializeObject<List<SD_EKPI_ITEM>>(model.SD_EKPI_ITEM);
                    foreach (var item in sdEkpiItem)
                    {
                        item.SD_EKPI_ID = entity.SD_EKPI_ID;
                        item.UPD_DATE = DateTime.Now;
                        item.UPD_USER_ID = OperatorProvider.Instance.Current.UserId;
                    }
                    _ekpiItemService.BulkInsert(sdEkpiItem);
                }
                return entity != null ? Success() : Error();
            }
        }

        public ActionResult Switch(string primaryKey, string valueFlag)
        {
            var id = int.Parse(primaryKey);
            var entity = _kpiService.GetWithTrace(x => x.SD_EKPI_ID == id);
            entity.VALID_FLAG = int.Parse(valueFlag) == 1 ? 0 : 1;
            var count = _kpiService.Update(entity);
            return count > 0 ? Success() : Error();
        }

        public ActionResult Configure()
        { return View(); }

        public ActionResult KYConfigure()
        {
            ViewBag.SD_SITEM_INFOS = _sdataitemService.GetManay(r => r.SD_ID == ProjectProvider.Instance.Current.SD_ID.ToString())?.ToList();
            return View();
        }

        [HttpGet]
        public ActionResult Detail()
        {
            return View();
        }

        [HttpGet]
        public ActionResult OrderNo()
        {
            var models = _kpiService.GetManay(r => r.SD_ID == ProjectProvider.Instance.Current.SD_ID);
            return Content(new { count = models.Count }.ToJson());
        }

        [HttpGet]
        public ActionResult ProcLog(string primaryKey)
        {
            List<PDP_PROC_LOG> list = _procLogService.GetManay(r => r.PROC_CAT_CODE == "3" && r.PROC_CONTENT_ID == primaryKey);

            if (list?.OrderByDescending(r => r.UPD_DATE).ToList().Count > 0)
            {
                var value = list?.OrderByDescending(r => r.UPD_DATE).ToList()[0];
                return Content(new
                {
                    startDate = value.START_TIME == null ? null : DateTime.Parse(value.START_TIME.ToString()).ToString("yyyy-MM-dd"),
                    endDate = value.END_TIME == null ? null : DateTime.Parse(value.END_TIME.ToString()).ToString("yyyy-MM-dd"),
                    patientId = value.PATIENT_ID
                }.ToJson());
            }
            return Error();
        }

        [HttpGet]
        public ActionResult BtnStatus(int kpiId)
        {
            var msg = string.Empty;
            var state = _procStateService.Get(r => r.SD_ID == ProjectProvider.Instance.Current.SD_ID && r.PROC_CAT_CODE == "3" && r.PROC_CONTENT_ID == kpiId.ToString());
            if (state?.PROC_STAT_CODE == "4")
            {
                msg = "锁定成功！！！";
            }
            else if (state?.PROC_STAT_CODE == "2")
            {
                msg = "执行成功！！！";
            }
            return Content(new { PROC_STAT_CODE = state?.PROC_STAT_CODE, msg }.ToJson());
        }


        [HttpPost]
        public ActionResult GetForm(string primaryKey)
        {
            int id = int.Parse(primaryKey);
            var entity = _kpiService.Get(r => r.SD_EKPI_ID == id);
            return Content(new SD_EKPI_INFO_VIEWMODEL
            {
                SD_EKPI_ID = entity.SD_EKPI_ID,
                SD_ID = entity.SD_ID,
                SD_CODE = entity.SD_CODE,
                SD_EKPI_CODE = entity.SD_EKPI_CODE,
                SD_EKPI_NAME = entity.SD_EKPI_NAME,
                SD_EKPI_ALIAS = entity.SD_EKPI_ALIAS,
                ORDER_NO = entity.ORDER_NO,
                SD_EKPI_CAT = entity.SD_EKPI_CAT,
                IS_POSITIVE = entity.IS_POSITIVE,
                SD_EKPI_ALGO = entity.SD_EKPI_ALGO,
                SD_EKPI_DESC = entity.SD_EKPI_DESC,
                VALID_FLAG = entity.VALID_FLAG,
                IS_RSN = (entity.IS_RSN == 1) ? true : false,
                IS_DISTRIBUTION = (entity.IS_DISTRIBUTION == 1) ? true : false,
                IS_TREND = (entity.IS_TREND == 1) ? true : false,
                NUM_PRECISION = entity.NUM_PRECISION,
                SD_EKPI_TYPE = entity.SD_EKPI_TYPE,
                IS_NUM = entity.IS_NUM,
                IS_DYNAMIC = entity.IS_DYNAMIC,
                UPD_USER_ID = entity.UPD_USER_ID,
                UPD_DATE = entity.UPD_DATE,
                VALUE_TABLE_ID = entity.VALUE_TABLE_ID,
                SD_EKPI_CONVER = entity.SD_EKPI_CONVER,
                SD_EKPI_UNIT = entity.SD_EKPI_UNIT,
                SD_EKPI_PLATONAME = entity.SD_EKPI_PLATONAME
            }.ToJson());
        }

        public ActionResult Delete(string primaryKey)
        {

            var id = int.Parse(primaryKey.Trim());
            //if (_unitKpiValueService.GetManay(r => r.SD_EKPI_ID == id)?.Count > 0)
            //{
            //    return Error("已被执行，请清库后再进行删除");
            //}
            _kpiValueService.ExDelete(r => r.SD_EKPI_ID == id);
            _unitKpiValueService.ExDelete(r => r.SD_EKPI_ID == id);
            int paramRow = _kpiparamService.Delete(r => r.SD_EKPI_ID == id);
            int setrow = _kpisetService.Delete(r => r.SD_EKPI_ID == id);
            int row = _kpiService.Delete(r => r.SD_EKPI_ID == id);
            //_kpiService.RefreshOrder();
            return row > 0 ? Success() : Error();
        }

        public ActionResult Test(KPI_EXCUTE_VIEWMODEL vm)
        {
            try
            {
                var msg = string.Empty;
                _procLogService.Log(_procLogService.KPICatCode(), vm?.startDate, vm?.endDate, vm.primaryKey);
                var pageData = _kpiService.Test(vm);

                var result = new LayPadding<TC_SD_EPKI_VALUE_VIEWMODEL>()
                {
                    result = true,
                    msg = msg,
                    list = pageData.Items,
                    count = pageData.TotalItems
                };
                return Content(result.ToJson());
            }
            catch (Exception e)
            {
                _errorLogService.LogErr(e.ToString(), ConvertExeFlag(vm.flag));
                return NoPage();
            }
        }

        public ActionResult Lock(KPI_EXCUTE_VIEWMODEL vm)
        {
            try
            {
                return Success(_kpiService.LockKPI(vm));
            }
            catch (Exception e)
            {
                _errorLogService.LogErr(e.ToString(), ConvertExeFlag(vm.flag));
                return Error(e.Message);
            }
        }

        public ActionResult Excute(KPI_EXCUTE_VIEWMODEL vm)
        {
            try
            {
                var msg = string.Empty;
                _procStateService.KPIDoExecute(ProjectProvider.Instance.Current.SD_ID, vm.primaryKey);
                _procLogService.Log(_procLogService.KPICatCode(), vm?.startDate, vm?.endDate, vm.primaryKey);
                //消息队列执行方式
                //_kpiService.MQExcute(vm);
                //多线程执行模式
                _kpiService.TaskExcute(new List<KPI_EXCUTE_VIEWMODEL>() { vm });
                _procLogService.Insert(new PDP_PROC_LOG()
                {
                    PROC_CONTENT_ID = vm.primaryKey,
                    SD_ID = ProjectProvider.Instance.Current.SD_ID,
                    PROC_CAT_CODE = "3",
                    PROC_STAT_CODE = 2,
                });
                return Success("执行进行中，可以关闭页面，在首页查看进度！！！");
            }
            catch (Exception e)
            {
                _errorLogService.LogErr(e.ToString(), ConvertExeFlag(vm.flag));
                return Error("执行进行中，可以关闭页面，在首页查看进度！！！");
            }
        }

        public ActionResult IsExitExeKpi(string clientKey, string kpiArray)
        {
            var list = new List<KPI_EXCUTE_VIEWMODEL>();
            foreach (var item in kpiArray?.Trim('|').Split('|'))
            {
                var id = int.Parse(item.ToString());
                var kpi = _kpiService.Get(r => r.SD_EKPI_ID == id);
                if (kpi.VALUE_TABLE_ID == 1)
                {
                    list.Add(new KPI_EXCUTE_VIEWMODEL() { primaryKey = item.ToString(), clientKey = clientKey });
                }
            }
            return Content(list.ToJson());
        }
        public ActionResult ExcuteAll(string kpiString)//string clientKey, string kpiArray)
        {
            try
            {
                var list = JsonConvert.DeserializeObject<List<KPI_EXCUTE_VIEWMODEL>>(kpiString);
                if (list.Count > 0)
                {
                    foreach (var kpi in list)
                    {
                        _procLogService.Insert(new PDP_PROC_LOG()
                        {
                            PROC_CONTENT_ID = kpi.primaryKey,
                            SD_ID = ProjectProvider.Instance.Current.SD_ID,
                            PROC_CAT_CODE = "3",
                            PROC_STAT_CODE = 2,
                        });
                    }
                    _kpiService.TaskExcute(list);
                    ViewBag.Error = "";
                }
                else
                    ViewBag.Error = "";
                return View("Progress");
            }
            catch (Exception e)
            {
                return Error(e.Message);
            }

        }

        public ActionResult LockAll(string clientKey, string kpiArray)
        {
            try
            {
                var list = new List<KPI_EXCUTE_VIEWMODEL>();
                foreach (var item in kpiArray?.Trim('|').Split('|'))
                {
                    var id = int.Parse(item.ToString());
                    var kpi = _kpiService.Get(r => r.SD_EKPI_ID == id);
                    if (kpi.VALUE_TABLE_ID == 1)
                    {
                        list.Add(new KPI_EXCUTE_VIEWMODEL() { primaryKey = item.ToString(), KPI_NAME = kpi.SD_EKPI_NAME, clientKey = clientKey });
                    }
                }

                Task.Factory.StartNew(() =>
                {
                    double index = 0;
                    list.ForEach(r =>
                    {
                        index += 1;
                        var percent = Math.Round(index * 100 / list.Count, 2);
                        var result = _kpiService.LockKPI(r);
                        ProcessHub.GetInstance().Send(clientKey, $"{percent}%", "批量锁定", $"评价指标：{r.KPI_NAME} 信息：{result}！！！", "");
                    });
                });

                return View("Progress"); ;
            }
            catch (Exception e)
            {
                return Error(e.Message);
            }

        }

        public ActionResult ClearDataAll(string clientKey, string kpiArray)
        {
            try
            {
                var list = new List<KPI_EXCUTE_VIEWMODEL>();
                foreach (var item in kpiArray?.Trim('|').Split('|'))
                {
                    var id = int.Parse(item.ToString());
                    var kpi = _kpiService.Get(r => r.SD_EKPI_ID == id);
                    if (kpi.VALUE_TABLE_ID == 1)
                    {
                        list.Add(new KPI_EXCUTE_VIEWMODEL() { primaryKey = item.ToString(), KPI_NAME = kpi.SD_EKPI_NAME, clientKey = clientKey });
                    }
                }

                Task.Factory.StartNew(() =>
                {
                    double index = 0;
                    list.ForEach(r =>
                    {
                        index += 1;
                        var percent = Math.Round(index * 100 / list.Count, 2);
                        _procStateService.KPIDoClear(ProjectProvider.Instance.Current.SD_ID, r.primaryKey);
                        var result = _kpiService.Clear(r.primaryKey);
                        _procLogService.Insert(new PDP_PROC_LOG()
                        {
                            PROC_CONTENT_ID = r.primaryKey,
                            SD_ID = ProjectProvider.Instance.Current.SD_ID,
                            PROC_CAT_CODE = "3",
                            PROC_STAT_CODE = 3,
                        });
                        ProcessHub.GetInstance().Send(clientKey, $"{percent}%", "批量清库", $"评价指标：{r.KPI_NAME} 清库成功，共清除 {result}条！！！", "");
                    });
                });

                return View("Progress"); ;
            }
            catch (Exception e)
            {
                return Error(e.Message);
            }

        }

        private int ConvertExeFlag(string flag)
        {
            switch (flag)
            {
                case "execute":
                    return 1;
                case "test":
                    return 2;
                case "export":
                    return 3;
                default:
                    return 0;
            }
        }

        public ActionResult Clear(string primaryKey)
        {
            _procStateService.KPIDoClear(ProjectProvider.Instance.Current.SD_ID, primaryKey);
            _kpiService.Clear(primaryKey);
            _procLogService.Insert(new PDP_PROC_LOG()
            {
                PROC_CONTENT_ID = primaryKey,
                SD_ID = ProjectProvider.Instance.Current.SD_ID,
                PROC_CAT_CODE = "3",
                PROC_STAT_CODE = 3,
            });
            return NoPage();
        }

        public ActionResult ExportResult(KPI_EXCUTE_VIEWMODEL vm)
        {
            try
            {
                var id = int.Parse(vm.primaryKey);
                var kpiname = _kpiService.Get(r => r.SD_EKPI_ID == id).SD_EKPI_NAME;
                //表头
                var title = new KPI_RESULT_VIEWMODEL()
                {
                    PATIENT_ID = "PATIENT_ID",
                    SD_CPAT_NO = "入组样本ID",
                    VALUE = "结果值"
                };
                //数据体
                var pageData = _kpiService.GetKpiResult(vm);
                //百分比
                var fenzi = pageData.Where(r => r.VALUE != "0.0000").ToList().Count * 100;
                var percent = Math.Round((double)fenzi / (double)pageData.Count, 2);
                pageData.Add(new KPI_RESULT_VIEWMODEL()
                {
                    PATIENT_ID = "百分比",
                    SD_CPAT_NO = "******",
                    VALUE = $"{percent}%"
                });
                var data = new Dictionary<string, List<KPI_RESULT_VIEWMODEL>>();
                data.Add("title", new List<KPI_RESULT_VIEWMODEL>() { title });
                data.Add("data", pageData);
                return Content(new { title = kpiname, data = data }.ToJson());
            }
            catch (Exception e)
            {
                return Error(e.ToString());
            }
        }

        [HttpGet]
        public ActionResult ExportAllKPIValueDetail()
        {
            try
            {
                ViewBag.route = "/TestCenterSystem/KPI/GetAllKPIValueDetail";
                return View("ValueDetail");
                //return Redirect("Common/Json2CSV");
                //return View("~Areas/TestCenterSystem/View/Common/Json2CSV.cshtml");
                //return RedirectToRoute(new { controller = "Common", action = "Json2CSV", route = "/TestCenterSystem/KPI/GetAllKPIValueDetail" });
                //return RedirectToRoute(new { controller = "Common", action = "Json2CSV",area= "TestCenterSystem", route = "/TestCenterSystem/KPI/GetAllKPIValueDetail" });
            }
            catch (Exception ex)
            {
                LogHelper.Error("ExportAllKPIValueDetail:" + ex.ToString());
                throw;
            }

        }

        [HttpGet]
        public ActionResult ExportAllKPIValueUnion()
        {
            try
            {
                ViewBag.route = "/TestCenterSystem/KPI/GetAllKPIValueUnion";
                return View("ValueDetail");
            }
            catch (Exception ex)
            {
                LogHelper.Error("GetAllKPIValueUnion:" + ex.ToString());
                throw;
            }

        }

        [HttpGet]
        public ActionResult GetAllKPIValueDetail()
        {
            try
            {
                var kpiValueCache = _unitKpiValueService.GetManay(r => r.SD_ID == ProjectProvider.Instance.Current.SD_ID).ToList();
                //表头
                var kpis = _kpiService.GetManayByOrder(r => r.SD_ID == ProjectProvider.Instance.Current.SD_ID && r.VALUE_TABLE_ID == 1, m => m.ORDER_NO);
                Dictionary<Tuple<string, int>, object> temp = new Dictionary<Tuple<string, int>, object>();
                kpis.ForEach(r =>
                {
                    temp.Add(new Tuple<string, int>($"[{r.SD_EKPI_CODE}]{ r.SD_EKPI_NAME}", r.SD_EKPI_ID), $"[{r.SD_EKPI_CODE}]{ r.SD_EKPI_NAME}");
                });

                dynamic title_code = new ExpandoObject();
                ((IDictionary<string, object>)title_code).Add("PNO", "PNO");
                foreach (KeyValuePair<Tuple<string, int>, object> item in temp)
                {
                    ((IDictionary<string, object>)title_code).Add(item.Key.Item1, item.Value);
                }

                //数据体
                var pageData = new List<dynamic>();
                var cpats = _unitCpatDetailService.GetManay(r => r.SD_ID == ProjectProvider.Instance.Current.SD_ID && r.IN_FLAG == "I");
                cpats.ForEach(
                    r =>
                    {
                        dynamic value = new ExpandoObject();
                        value.PNO = r.PATIENT_NO;
                        foreach (KeyValuePair<Tuple<string, int>, object> item in temp)
                        {
                            ((IDictionary<string, object>)value).Add(item.Key.Item1, kpiValueCache.FirstOrDefault(v => v.SD_CPAT_NO == r.SD_CPAT_NO && v.SD_EKPI_ID == item.Key.Item2)?.INDEX_VALUE.ToString());
                        }
                        pageData.Add(value);
                    }
                );
                var data = new Dictionary<string, List<dynamic>>();
                data.Add("title", new List<dynamic>() { title_code });
                data.Add("data", pageData);
                return Content(pageData.ToJson());

            }
            catch (Exception e)
            {
                return Error(e.ToString());
            }
        }

        [HttpGet]
        public ActionResult GetAllKPIValueUnion()
        {
            try
            {
                var pageData = new List<dynamic>();
                var sdId = ProjectProvider.Instance.Current.SD_ID;
                var kpiValueCache = _unitKpiValueService.GetManay(r => r.SD_ID == sdId).ToList();
                //表头
                var kpis = _kpiService.GetManayByOrder(r => r.SD_ID == sdId && r.VALUE_TABLE_ID == 1, m => m.ORDER_NO);
                Dictionary<Tuple<string, int>, object> temp = new Dictionary<Tuple<string, int>, object>();
                kpis.ForEach(r =>
                {
                    temp.Add(new Tuple<string, int>($"{r.SD_EKPI_CODE}分子", r.SD_EKPI_ID), r.SD_EKPI_CODE);
                    temp.Add(new Tuple<string, int>($"{r.SD_EKPI_CODE}分母", r.SD_EKPI_ID), r.SD_EKPI_CODE);
                });

                dynamic title = new ExpandoObject();
                //((IDictionary<string, object>)title).Add("PNO", "PNO");
                foreach (KeyValuePair<Tuple<string, int>, object> item in temp)
                {
                    ((IDictionary<string, object>)title).Add(item.Key.Item1, item.Value);
                }
                //二级标题
                //dynamic head2 = new ExpandoObject();
                //foreach (KeyValuePair<Tuple<string, int>, object> item in temp)
                //{
                //    if (item.Key.Item1.Contains("分子"))
                //    {
                //        ((IDictionary<string, object>)head2).Add(item.Key.Item1, "分子");
                //    }
                //    else
                //    {
                //        ((IDictionary<string, object>)head2).Add(item.Key.Item1, "分母");
                //    }

                //}
                //pageData.Add(head2);
                //三级标题
                dynamic head3 = new ExpandoObject();
                foreach (KeyValuePair<Tuple<string, int>, object> item in temp)
                {
                    var conver = kpis.FirstOrDefault(k => k.SD_EKPI_ID == item.Key.Item2).SD_EKPI_CONVER;
                    if (conver == 0)
                    {
                        if (item.Key.Item1.Contains("分子"))
                        {
                            ((IDictionary<string, object>)head3).Add(item.Key.Item1, kpiValueCache.Where(h3 => h3.SD_EKPI_ID == item.Key.Item2).Count());
                        }
                        else
                        {
                            ((IDictionary<string, object>)head3).Add(item.Key.Item1, kpiValueCache.Where(h3 => h3.SD_EKPI_ID == item.Key.Item2).Count());
                        }
                    }
                    else
                    {
                        if (item.Key.Item1.Contains("分子"))
                        {
                            ((IDictionary<string, object>)head3).Add(item.Key.Item1, kpiValueCache.Where(h3 => h3.SD_EKPI_ID == item.Key.Item2 && h3.INDEX_VALUE == 1).Count());
                        }
                        else
                        {
                            ((IDictionary<string, object>)head3).Add(item.Key.Item1, kpiValueCache.Where(h3 => h3.SD_EKPI_ID == item.Key.Item2).Count());
                        }
                    }




                }
                pageData.Add(head3);
                //数据体

                var cpats = _unitCpatDetailService.GetManay(r => r.SD_ID == sdId && r.IN_FLAG == "I");
                for (var i = 0; i < cpats.Count; i++)
                {
                    dynamic value = new ExpandoObject();
                    foreach (KeyValuePair<Tuple<string, int>, object> item in temp)
                    {
                        var conver = kpis.FirstOrDefault(k => k.SD_EKPI_ID == item.Key.Item2).SD_EKPI_CONVER;
                        if (conver == 0)
                        {
                            if (item.Key.Item1.Contains("分子"))
                            {
                                var z = kpiValueCache.Where(v => v.SD_EKPI_ID == item.Key.Item2).Skip(i).Take(1).FirstOrDefault();
                                if (z?.INDEX_VALUE != null)
                                {
                                    ((IDictionary<string, object>)value).Add(item.Key.Item1, z.SD_CPAT_NO);
                                }
                                else
                                {
                                    ((IDictionary<string, object>)value).Add(item.Key.Item1, null);
                                }
                            }
                            else
                            {
                                var m = kpiValueCache.Where(v => v.SD_EKPI_ID == item.Key.Item2).Skip(i).Take(1).FirstOrDefault();
                                if (m == null)
                                {
                                    ((IDictionary<string, object>)value).Add(item.Key.Item1, null);
                                }
                                else
                                {
                                    ((IDictionary<string, object>)value).Add(item.Key.Item1, m.SD_CPAT_NO);
                                }
                            }
                        }
                        else
                        {
                            if (item.Key.Item1.Contains("分子"))
                            {
                                var z = kpiValueCache.Where(v => v.SD_EKPI_ID == item.Key.Item2 && v.INDEX_VALUE == 1).Skip(i).Take(1).FirstOrDefault();
                                if (z?.INDEX_VALUE == 1)
                                {
                                    ((IDictionary<string, object>)value).Add(item.Key.Item1, z.SD_CPAT_NO);
                                }
                                else
                                {
                                    ((IDictionary<string, object>)value).Add(item.Key.Item1, null);
                                }
                            }
                            else
                            {
                                var m = kpiValueCache.Where(v => v.SD_EKPI_ID == item.Key.Item2 && (v.INDEX_VALUE == 1 || v.INDEX_VALUE == 0)).Skip(i).Take(1).FirstOrDefault();
                                if (m == null)
                                {
                                    ((IDictionary<string, object>)value).Add(item.Key.Item1, null);
                                }
                                else
                                {
                                    ((IDictionary<string, object>)value).Add(item.Key.Item1, m.SD_CPAT_NO);
                                }
                            }
                        }

                    }
                    pageData.Add(value);
                }


                //var cpats = _unitCpatDetailService.GetManay(r => r.SD_ID == sdId && r.IN_FLAG == "I");
                //cpats.ForEach(
                //    r =>
                //    {
                //        dynamic value = new ExpandoObject();
                //        foreach (KeyValuePair<Tuple<string, int>, object> item in temp)
                //        {
                //            var k = kpiValueCache.FirstOrDefault(v => v.SD_CPAT_NO == r.SD_CPAT_NO && v.SD_EKPI_ID == item.Key.Item2)?.INDEX_VALUE;
                //            if (item.Key.Item1.Contains("分子"))
                //            {
                //                if (k == 1)
                //                {
                //                    ((IDictionary<string, object>)value).Add(item.Key.Item1, r.SD_CPAT_NO);
                //                }
                //                else 
                //                {
                //                    ((IDictionary<string, object>)value).Add(item.Key.Item1, null);
                //                }
                //            }
                //            else
                //            {
                //                if (k == null)
                //                {
                //                    ((IDictionary<string, object>)value).Add(item.Key.Item1, null);
                //                }
                //                else
                //                {
                //                    ((IDictionary<string, object>)value).Add(item.Key.Item1, r.SD_CPAT_NO);
                //                }
                //            }
                //        }
                //        pageData.Add(value);
                //    }
                //);
                var data = new Dictionary<string, List<dynamic>>();
                data.Add("title", new List<dynamic>() { title });
                data.Add("data", pageData);
                return Content(pageData.ToJson());

            }
            catch (Exception e)
            {
                return Error(e.ToString());
            }
        }

        public ActionResult ExportAll()
        {
            ViewBag.route = "/TestCenterSystem/KPI/GetAllKPIResult";
            return View("ValueDetail");
            //return RedirectToRoute(new { controller = "Common", action = "Json2CSV", route = "/TestCenterSystem/KPI/GetAllKPIResult" });

        }

        public List<SD_CAT_CONF> GetCatNameCache()
        {
            var sdId = ProjectProvider.Instance.Current.SD_ID;
            var EKPI_CAT_PA_CODE = _sdService.Get(r => r.SD_ID == sdId).EKPI_CAT_PA_CODE;
            return _catconfService.GetManay(r => r.CAT_PA_CODE == EKPI_CAT_PA_CODE);

        }

        [HttpGet]
        public ActionResult GetAllKPIResult()
        {
            try
            {
                var CatNameCache = GetCatNameCache();
                var result = new List<ALL_KPI_RESULT_VIEWMODEL>();
                //数据体
                var kpis = _kpiService.GetManayByOrder(r => r.SD_ID == ProjectProvider.Instance.Current.SD_ID && r.VALUE_TABLE_ID == 1, m => m.ORDER_NO);
                kpis.ForEach(r =>
                {
                    if (r.SD_EKPI_ID == 10366)
                    {
                        var a = 1;
                    }
                    var kpiname = _kpiService.Get(k => k.SD_EKPI_ID == r.SD_EKPI_ID).SD_EKPI_NAME;
                    var pageData = _kpiService.GetKpiResult(new KPI_EXCUTE_VIEWMODEL() { primaryKey = r.SD_EKPI_ID.ToString() });
                    //百分比
                    var fenmu = pageData.Count;// r.SD_EKPI_NAME.Contains("平均") ? pageData.Where(z => z.DOUBLE_VALUE != 0.0000).Count() : pageData.Count;
                    var fenzi = 0.00;// r.SD_EKPI_TYPE != 0 ? pageData.Where(z => z.VALUE !="0.0000").ToList().Count : pageData?.Count;
                    if (r.SD_EKPI_CONVER == 0)
                    {
                        fenzi = pageData.Select(f => f.DOUBLE_VALUE).Sum();
                    }
                    else
                    {
                        fenzi = pageData.Where(z => z.VALUE != "0.0000").ToList().Count;
                    }
                    double percent = 0;
                    var percentoravg = string.Empty;
                    if (pageData.Count != 0)
                    {
                        if (kpiname.Contains("平均"))
                        {
                            var sum = pageData.Select(s => s.DOUBLE_VALUE).Sum();
                            percent = Math.Round(sum / fenmu, 4);
                            percentoravg = percent.ToString("0.00");
                        }
                        else
                        {
                            percent = Math.Round((double)fenzi / (double)pageData.Count, 4);
                            percentoravg = $"{(percent * 100).ToString("0.00")}%";
                        }
                    }
                    result.Add(new ALL_KPI_RESULT_VIEWMODEL()
                    {
                        序号 = r.ORDER_NO.ToString(),
                        编码 = r.SD_EKPI_CODE,
                        针对群体的指标名称 = r.SD_EKPI_NAME,
                        针对个体的质控点名称 = r.SD_EKPI_ALIAS,
                        指标类别 = CatNameCache.FirstOrDefault(cr => cr.CAT_ID == r.SD_EKPI_CAT).CAT_NAME,
                        是否为统计指标 = r.SD_EKPI_TYPE == 1 ? "非统计类" : "统计类",
                        指标正负向 = r.IS_POSITIVE == 1 ? "正向" : (r.IS_POSITIVE == 0 ? "负向" : "其他"),
                        数据范围 = r.SD_EKPI_CONVER == 1 ? "百分比" : (r.SD_EKPI_CONVER == 2 ? "千分比" : "正常模式"),
                        单位 = r.SD_EKPI_UNIT,
                        是否开启趋势 = r.IS_TREND == 1 ? "是" : "否",
                        是否开启科室 = r.IS_DISTRIBUTION == 1 ? "是" : "否",
                        是否开启原因分析 = r.IS_RSN == 1 ? "是" : "否",
                        分母是否为一 = r.IS_NUM == 1 ? "是" : "否",
                        动态基线 = r.IS_DYNAMIC == 1 ? "是" : "否",
                        分子 = fenzi.ToString("0.00"),
                        分母 = fenmu.ToString(),// pageData?.Count.ToString(),
                        比率或值 = percentoravg
                    });
                });

                return Content(result.ToJson());
            }
            catch (Exception e)
            {
                return Error(e.ToString());
            }
        }

        public ActionResult ExportSQL()
        {
            throw new Exception("由项目统一导出SQL");
        }

        public ActionResult Demo(string primaryKey)
        {
            for (int i = 0; i < 10; i++)
            {
                Thread.Sleep(4000);
                Random ran = new Random();
                ProcessHub.GetInstance().Send(primaryKey, ran.NextDouble().ToString());
            }

            return null;
        }

        public ActionResult Razor()
        {
            return View();
        }
        //[HttpGet, AuthorizeChecked(true)]
        public ActionResult ShowParam()
        {
            return View();
        }
        public ActionResult GetEkpiItem(string kpiId)
        {
            var id = int.Parse(kpiId);
            var ekpiItemList = _ekpiItemService.GetManay(i => i.SD_EKPI_ID == id);
            var itemIdList = ekpiItemList.Select(i => i.SD_ITEM_ID).ToList();
            var itemInfo = _itemService.GetManay(i => itemIdList.Contains(i.SD_ITEM_ID)).ToList();
            foreach (var item in ekpiItemList)
            {
                item.UPD_USER_ID =
                    itemInfo.Where(i => i.SD_ITEM_ID == item.SD_ITEM_ID).ToList().FirstOrDefault().SD_ITEM_NAME;
            }
            return Content(ekpiItemList.ToJson());
        }
        [HttpGet]
        public ActionResult GetSdItemParam(int page, int limit, string sdItemId, string keyWord = "")
        {
            int count = 0;
            var pageData = _itemService.GetPage(page, limit, "", "", keyWord);
            var result = new Dictionary<string, object>() {
                { "code", 0},
                { "msg","success"},
                { "data", pageData.Items},
                { "count", pageData.TotalItems}
            };
            //var listDataItemViewModel = _dataItemServices.GetPage(page, limit, "SdItemId", "desc", out count, sdItemId, keyWord);
            //return Success(listDataItemViewModel, count);
            return Content(result.ToJson());
        }

    }
}