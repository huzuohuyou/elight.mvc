using Elight.Entity;
using Elight.Infrastructure;
using Elight.Web.Controllers;
using Elight.Web.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TestingCenterSystem.Models.PDP;
using TestingCenterSystem.Service.comm.Interface;
using TestingCenterSystem.Service.InGroup;
using TestingCenterSystem.Service.KPI;
using TestingCenterSystem.Service.KPI.Interface;
using TestingCenterSystem.Service.Layer;
using TestingCenterSystem.Service.Layer.Interface;
using TestingCenterSystem.ViewModels.Layer;
using TestingCenterSystem.ViewModels.Project;

namespace Elight.Web.Areas.TestCenterSystem.Controllers.LayerInfo
{
    public class LayerInfoController : BaseController
    {
        private static readonly ISdLayerInfoService _layerinfoService = new SdLayerInfoService();
        private static readonly IUnSdLayerValueService _unLayerValueService = new UnSdLayerValueService();
        private static readonly ISdLayerValueService _layerValueService = new SdLayerValueService();
        private static readonly IProcStateService _procStateService = new ProcStateService();
        private static readonly ISdLayerEkpiService _sdLayerEkpiService = new SdLayerEkpiService();
        private static readonly ISdLayerParamService _sdLayerParamService = new SdLayerParamService();
        private static readonly IKPIService _kpiService = new KPIService();
        // GET: TestCenterSystem/LayerInfo
        public ActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public ActionResult GetList(int page, int limit, string keyWord = "")
        {
            var data = _layerinfoService.GetList(page, limit, keyWord);
            var result = new Dictionary<string, object>() {
                { "code", 0},
                { "msg","success"},
                { "data", data.Item1},
                { "count", data.Item2}
            };
            return Content(result.ToJson());
        }

        public ActionResult Configure()
        { return View(); }

        [HttpGet]
        public ActionResult Form(int? primaryKey)
        {
            var sdId = ProjectProvider.Instance.Current.SD_ID;
            if (primaryKey == null)
                ViewBag.ParentInfo = _layerinfoService.GetSearchList(s => s.SD_ID == sdId);//_catconfService.GetSearchList(s => s.CAT_PA_CODE == catCode);
            else
                ViewBag.ParentInfo = _layerinfoService.GetSearchList(s => s.SD_ID == sdId && s.SD_LAYER_ID != primaryKey);
            return View();
        }

        [HttpPost]
        public ActionResult GetListTreeSelect(int? primaryKey)
        {
            var listKPI = new List<SD_EKPI_INFO>();
            var sdId = ProjectProvider.Instance.Current.SD_ID;
            var layerIDs = new List<int>();
            if (primaryKey == null)
                layerIDs = _layerinfoService.GetManay(r => r.SD_ID == sdId).Select(s => s.SD_LAYER_ID).ToList();
            else
                layerIDs = _layerinfoService.GetManay(r => r.SD_ID == sdId && r.SD_LAYER_ID != primaryKey).Select(s => s.SD_LAYER_ID).ToList();
            var kpiIDs = _sdLayerEkpiService.GetManay(r => layerIDs.Contains(r.SD_LAYER_ID)).Select(s => s.SD_EKPI_ID).ToList();
            listKPI = _kpiService.GetManay(r => r.SD_ID == sdId && !kpiIDs.Contains(r.SD_EKPI_ID));
            var listTree = new List<TreeSelect>();
            foreach (var item in listKPI)
            {
                TreeSelect model = new TreeSelect();
                model.id = item.SD_EKPI_ID.ToString();
                model.text = item.SD_EKPI_NAME;
                listTree.Add(model);
            }
            return Content(listTree.ToJson());
        }

        [HttpPost]
        public ActionResult Validate(SD_LAYER_INFO model)
        {
            var pass = _layerinfoService.ValidateFormula(model);
            if (!pass.Item2)
            {
                //throw new Exception(pass.Item1);
                return Error(pass.Item1);
            }
            return Success();
        }


        public ActionResult Test(LAYER_TEST_PARAM_VIEWMODEL vm)
        {
            try
            {
                var msg = string.Empty;
                //_procLogService.Log(_procLogService.KPICatCode(), vm?.START_DATE, vm?.END_DATE, vm.LAYER_ID);
                var list = _layerinfoService.GetLayerValue(vm);

                var result = new LayPadding<SD_LAYER_VALUE>()
                {
                    result = true,
                    msg = msg,
                    list = list,
                    count = list.Count
                };
                return Content(result.ToJson());
            }
            catch (Exception e)
            {
                //_errorLogService.LogErr(e.ToString(), ConvertExeFlag(vm.flag));
                return NoPage();
            }
        }

        public ActionResult Excute(LAYER_TEST_PARAM_VIEWMODEL vm)
        {
            try
            {
                var msg = string.Empty;
                _procLogService.Log(_procLogService.LayerCatCode(), vm?.START_DATE, vm?.END_DATE, vm.SD_LAYER_ID.ToString());
                _procStateService.LayerDoExcute(vm.SD_LAYER_ID.ToString());
                var list = _layerinfoService.GetLayerValue(vm);
                _unLayerValueService.BulkInsert("SD_LAYER_VALUE", list);
                return Success();
            }
            catch (Exception e)
            {
                return Error(e.Message);
            }
        }

        public ActionResult Lock(LAYER_TEST_PARAM_VIEWMODEL vm)
        {
            try
            {
                var utLayerValue = _unLayerValueService.GetManay(r => r.SD_LAYER_ID == vm.SD_LAYER_ID);
                _layerValueService.ExDelete(r => r.SD_LAYER_ID == vm.SD_LAYER_ID);
                _layerValueService.BulkInsert("SD_LAYER_VALUE", utLayerValue);
                _procStateService.LayerDoLock(vm.SD_LAYER_ID.ToString());
                return Success();
            }
            catch (Exception e)
            {
                return Error(e.Message);
            }
        }

        public ActionResult Clear(int? primaryKey)
        {
            _unLayerValueService.ExDelete(r => r.SD_LAYER_ID == primaryKey);
            _procStateService.LayerDoClear(primaryKey.ToString());
            //_procStateService.KPIDoClear(ProjectProvider.Instance.Current.SD_ID, primaryKey);
            //_kpiService.Clear(primaryKey);
            //_procLogService.Insert(new PDP_PROC_LOG()
            //{
            //    PROC_CONTENT_ID = primaryKey,
            //    SD_ID = ProjectProvider.Instance.Current.SD_ID,
            //    PROC_CAT_CODE = "3",
            //    PROC_STAT_CODE = 3,
            //});
            return Success();
        }

        public ActionResult UpdateFormula(SD_LAYER_INFO entity)
        {

            var model = _layerinfoService.GetWithTrace(r => r.SD_LAYER_ID == entity.SD_LAYER_ID);
            model.SD_LAYER_FORMULA = entity.SD_LAYER_FORMULA;
            var row = _layerinfoService.Update(model);
            return row > 0 ? Success() : Error();

        }


        public ActionResult Form(SD_LAYER_INFO entity, string EKPI_IDS)
        {
            var model = new SD_LAYER_INFO();
            var row = 0;
            var sdId = ProjectProvider.Instance.Current.SD_ID;
            try
            {
                if (_layerinfoService.Exists(r => r.SD_LAYER_ID == entity.SD_LAYER_ID))//编辑
                {
                    if (_layerinfoService.Exists(r => (r.SD_LAYER_CODE == entity.SD_LAYER_CODE || r.SD_LAYER_NAME == entity.SD_LAYER_NAME) && r.SD_LAYER_ID != entity.SD_LAYER_ID && r.SD_ID == entity.SD_ID))
                        return Error("已存在相同代码或名称的层级");
                    model = _layerinfoService.GetWithTrace(r => r.SD_LAYER_ID == entity.SD_LAYER_ID);
                    model.SD_LAYER_CODE = entity.SD_LAYER_CODE;
                    model.SD_LAYER_NAME = entity.SD_LAYER_NAME;
                    model.SD_LAYER_ALIAS = entity.SD_LAYER_ALIAS;
                    model.SD_LAYER_DESC = entity.SD_LAYER_DESC;
                    model.LAYER_PARIENT_ID = entity.LAYER_PARIENT_ID;
                    model.ORDER_NO = entity.ORDER_NO;
                    model.VALID_FLAG = 1;
                    row += _layerinfoService.Update(model);
                }
                else//添加
                {
                    entity.SD_ID = ProjectProvider.Instance.Current.SD_ID;
                    entity.SD_CODE = ProjectProvider.Instance.Current.SD_CODE;
                    entity.VALID_FLAG = 1;
                    //先判断添加的层级名称和代码是否存在相同的，add 2018.04.17
                    if (_layerinfoService.Exists(r => (r.SD_LAYER_CODE == entity.SD_LAYER_CODE || r.SD_LAYER_NAME == entity.SD_LAYER_NAME) && r.SD_ID == entity.SD_ID))
                        return Error("已存在相同代码或名称的层级");
                    model = _layerinfoService.Insert(entity);
                    _procStateService.LayerNonExcute(model.SD_LAYER_ID.ToString());
                    row += (model != null ? 1 : 0);
                }
                if (row != 1)
                    return Error("保存SD_LAYER_INFO信息失败！");
                _sdLayerEkpiService.ExDelete(r => r.SD_LAYER_ID == model.SD_LAYER_ID);
                if (!string.IsNullOrWhiteSpace(EKPI_IDS))
                {
                    var EKPIIDList = EKPI_IDS.Split(',').ToList();
                    var LayerEkpiList = new List<SD_LAYER_EKPI>();
                    EKPIIDList.ForEach(kpiId => LayerEkpiList.Add(new SD_LAYER_EKPI()
                    {
                        SD_EKPI_ID = int.Parse(kpiId),
                        SD_LAYER_ID = model.SD_LAYER_ID,
                        UPD_USER_ID = OperatorProvider.Instance.Current.UserId,
                        UPD_DATE = DateTime.Now
                    }));
                    _sdLayerEkpiService.BulkInsert("SD_LAYER_EKPI", LayerEkpiList);
                    #region 如果存在，则更新信息，如果不存在，则插入  逻辑错误
                    //EKPIIDList.ForEach(kpiId =>
                    //{
                    //    var kpiid = int.Parse(kpiId);
                    //    var LayerEkpi = _sdLayerEkpiService.GetWithTrace(r => r.SD_LAYER_ID == model.SD_LAYER_ID && r.SD_EKPI_ID == kpiid);
                    //    if (LayerEkpi == null)
                    //        _sdLayerEkpiService.Insert(new SD_LAYER_EKPI()
                    //        {
                    //            SD_EKPI_ID = int.Parse(kpiId),
                    //            SD_LAYER_ID = model.SD_LAYER_ID,
                    //        });
                    //    else
                    //        _sdLayerEkpiService.Update(LayerEkpi);
                    //}); 
                    #endregion
                }
                return Success();
            }
            catch (Exception ex)
            {
                return Error(ex.Message);
            }
        }

        [HttpPost]
        public ActionResult GetForm(int? primaryKey)
        {
            var entity = _layerinfoService.Get(r => r.SD_LAYER_ID == primaryKey);
            var KPI = _sdLayerEkpiService.GetManay(r => r.SD_LAYER_ID == primaryKey).Select(s => s.SD_EKPI_ID).ToList();
            var resultEntity = new
            {
                SD_LAYER_ID = entity.SD_LAYER_ID,
                SD_ID = entity.SD_ID,
                SD_CODE = entity.SD_CODE,
                SD_LAYER_CODE = entity.SD_LAYER_CODE,
                SD_LAYER_NAME = entity.SD_LAYER_NAME,
                SD_LAYER_ALIAS = entity.SD_LAYER_ALIAS,
                SD_LAYER_FORMULA = entity.SD_LAYER_FORMULA,
                SD_LAYER_DESC = entity.SD_LAYER_DESC,
                LAYER_PARIENT_ID = entity.LAYER_PARIENT_ID,
                ORDER_NO = entity.ORDER_NO,
                VALID_FLAG = entity.VALID_FLAG,
                UPD_USER_ID = entity.UPD_USER_ID,
                UPD_DATE = entity.UPD_DATE,
                CREATE_USER_ID = entity.CREATE_USER_ID,
                CREATE_DATE = entity.CREATE_DATE,
                KPI = KPI
            };
            return Content(resultEntity.ToJson());
        }

        public ActionResult Delete(int? primaryKey)
        {
            var sdId = ProjectProvider.Instance.Current.SD_ID;
            if (_layerinfoService.GetManay(r => r.SD_ID == sdId && r.LAYER_PARIENT_ID == primaryKey).Count() > 0)
                return Error("该层级被其他层级使用，不能删除！");
            int row = _sdLayerEkpiService.Delete(r => r.SD_LAYER_ID == primaryKey);
            if (row < 0)
                return Error("删除SD_LAYER_EKPI表数据失败！");
            row = _sdLayerParamService.Delete(r => r.SD_LAYER_ID == primaryKey);
            if (row < 0)
                return Error("删除SD_LAYER_PARAM表数据失败！");
            row = _layerValueService.Delete(r => r.SD_LAYER_ID == primaryKey);
            if (row < 0)
                return Error("删除SD_LAYER_VALUE表数据失败！");
            row = _layerinfoService.Delete(r => r.SD_ID == sdId && r.SD_LAYER_ID == primaryKey);
            return row > 0 ? Success() : Error();
        }

        public ActionResult Switch(int? primaryKey, int? valueFlag)
        {
            var entity = _layerinfoService.GetWithTrace(x => x.SD_LAYER_ID == primaryKey);
            entity.VALID_FLAG = valueFlag.Value == 0 ? 1 : 0;
            var count = _layerinfoService.Update(entity);
            return count > 0 ? Success() : Error();
        }

        public ActionResult ExportResult(int primaryKey)
        {
            var data = new Dictionary<string, object>();
            try
            {
                var sdId = ProjectProvider.Instance.Current.SD_ID;
                var pageData = _layerValueService.GetManay(r => r.SD_ID == sdId && r.SD_LAYER_ID == primaryKey).Select(s => new { SD_CPAT_NO = s.SD_CPAT_NO }).ToList();
                var title = new { SD_CPAT_NO = "入组样本ID" };
                data.Add("title", new List<dynamic> { title });
                data.Add("data", pageData);
            }
            catch (Exception ex)
            {
                data.Add("title", null);
            }
            return Content(data.ToJson());
        }
    }
}