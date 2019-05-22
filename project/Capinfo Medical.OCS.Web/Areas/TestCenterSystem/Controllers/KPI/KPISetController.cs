using Elight.Infrastructure;
using Elight.Web.Controllers;
using Elight.Web.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TestingCenterSystem.Models;
using TestingCenterSystem.Models.PDP;
using TestingCenterSystem.Service.KPI;
using TestingCenterSystem.Service.KPI.Interface;
using TestingCenterSystem.Service.Project;
using TestingCenterSystem.Service.Project.Interface;
using TestingCenterSystem.ViewModels.KPI;

namespace Elight.Web.Areas.TestCenterSystem.Controllers
{
    [LoginChecked]
    public class KPISetController : BaseController
    {
        private readonly IKPISetService _kpisetService = new KPISetService();
        private readonly IKPIParamService _kpiparamService = new KPIParamService();
        private readonly IProjectService _projectService = new ProjectService();
        // GET: TestCenterSystem/KPISet
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public ActionResult GetForm(string primaryKey)
        {
            int kpiid = int.Parse(primaryKey);
            var entity = _kpisetService.Get(r => r.SD_EKPI_ID == kpiid);
            return Content(new {
                SD_ID = _projectService.GetCurrentSD().SD_ID,
                SD_EKPI_ID = primaryKey,
                NUM_FORMULA =entity?.NUM_FORMULA,
                FRA_FORMULA =entity?.FRA_FORMULA}.ToJson());
        }


        [HttpPost]
        public ActionResult Validate(VALIDATE_FORMULA_VIEWMODEL model) {
            var pass = _kpisetService.ValidateFormula(new SD_EKPI_FORMULA() { SD_EKPI_ID = model.KPI_ID,NUM_FORMULA=model.NUM_FORMULA,FRA_FORMULA=model.FRA_FORMULA }, _kpiparamService.GetSearchList(r => r.SD_EKPI_ID == model.KPI_ID));
            if (!pass.Item2)
            {
                //throw new Exception(pass.Item1);
                return Error(pass.Item1);
            }
            return Success();
        }

        [HttpPost]
        public ActionResult Form(SD_EKPI_FORMULA model)
        {

            var pass=_kpisetService.ValidateFormula(model, _kpiparamService.GetSearchList(r=>r.SD_EKPI_ID == model.SD_EKPI_ID));
            if (!pass.Item2)
            {
                //throw new Exception(pass.Item1);
                return  Error(pass.Item1);
            }
            if (_kpisetService.Exists(r => r.SD_EKPI_ID == model.SD_EKPI_ID))
            {
                var row = _kpisetService.Update(model);
                return row > 0 ? Success() : Error();
            }
            else
            {
                var entity = _kpisetService.Insert(model);
                return entity != null ? Success() : Error();
            }
        }

    }
}