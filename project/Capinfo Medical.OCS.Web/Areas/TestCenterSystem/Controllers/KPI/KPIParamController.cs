using Elight.Entity;
using Elight.Infrastructure;
using Elight.Web.Controllers;
using Elight.Web.Filters;
using System.Web.Mvc;
using TestingCenterSystem.Models;
using TestingCenterSystem.Models.PDP;
using TestingCenterSystem.Service.KPI;
using TestingCenterSystem.Service.KPI.Interface;
using TestingCenterSystem.ViewModels.KPI;

namespace Elight.Web.Areas.TestCenterSystem.Controllers
{
    [LoginChecked]
    public class KPIParamController : BaseController
    {
        private readonly IKPIParamService _kpiparamService = new KPIParamService();
        
        [HttpGet, AuthorizeChecked]
        public ActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="projectId">项目Id</param>
        /// <param name="sdId">病种id</param>
        /// <param name="kpiId">kpi id</param>
        /// <returns></returns>
        public ActionResult Index(int projectId,int sdId,int kpiId)
        {
            var pageData = _kpiparamService.GetList(projectId, sdId, kpiId);
            var result = new LayPadding<PPARAM_VIEWMODEL>()
            {
                result = true,
                msg = "success",
                list = pageData.Items,
                count = pageData.TotalItems
            };
            return Content(result.ToJson());
        }



        public ActionResult Delete(string primaryKey)
        {
            //##projectid
            int SD_EKPI_ID = int.Parse(primaryKey.Split('_')[0]);
            int SD_ITEM_ID = int.Parse(primaryKey.Split('_')[1]);
            var row = _kpiparamService.Delete(r => r.SD_ITEM_ID == SD_ITEM_ID && r.SD_EKPI_ID == SD_EKPI_ID);
            return row > 0 ? Success() : Error();
        }

        public ActionResult Add(int projectId,int sdId, int kpiid, string dataitemid)
        {
            var viewmodel = new PPARAM_VIEWMODEL(projectId, sdId, kpiid) { ProjectId = projectId, DataItemID = dataitemid };
            var entity = new SD_EKPI_PARAM() { SD_EKPI_ID = (int)viewmodel.KPIId, SD_ITEM_ID = int.Parse(viewmodel.DataItemID), SD_EKPI_PARAM_NAME = viewmodel.ParamCode };
            int id = int.Parse(viewmodel.DataItemID);
            if (_kpiparamService.Exists(r => r.SD_EKPI_ID == viewmodel.KPIId && r.SD_ITEM_ID == id))
            {
               return Error("数据项已存在！！！");
            }
            else
            {
                _kpiparamService.Insert(entity);
                return Success();
            }
        }
    }
}