using Elight.Infrastructure;
using Elight.Web.Controllers;
using Elight.Web.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TestingCenterSystem.Models.PDP;
using TestingCenterSystem.Service.SKPI;
using TestingCenterSystem.Service.SKPI.Interface;
using TestingCenterSystem.ViewModels.KPI;
using TestingCenterSystem.ViewModels.Project;

namespace Elight.Web.Areas.TestCenterSystem.Controllers.KPI
{
    [LoginChecked]
    public class SKPIController : BaseController
    {
        private static readonly ISdSkpiSitemService _skpiService = new SdSkpiSitemService();



        public ActionResult Form(SD_SKPI_SITEM_VIEWMODEL model)
        {
            var sdId = ProjectProvider.Instance.Current.SD_ID;
            _skpiService.ExDelete2(r => r.SD_EKPI_ID == model.KPI_ID);
            if (model.FEN_ZI != 0)
            {
                _skpiService.Insert(new SD_SKPI_SITEM()
                {
                    SD_EKPI_ID = model.KPI_ID,
                    SITEM_TYPE = 1,
                    SITEM_ID = model.FEN_ZI,
                    SITEM_DESC = model.Z_NAME,
                    RESULT_TYPE = model.Z_RESULT_TYPE
                });
            }
            if (model.FEN_MU != 0)
            {
                _skpiService.Insert(new SD_SKPI_SITEM()
                {
                    SD_EKPI_ID = model.KPI_ID,
                    SITEM_TYPE = 2,
                    SITEM_ID = model.FEN_MU,
                    SITEM_DESC = model.M_NAME,
                    RESULT_TYPE = model.M_RESULT_TYPE
                });
            }
            return Success();
        }

        [HttpPost]
        public ActionResult GetForm(string primaryKey)
        {
            int id = int.Parse(primaryKey);
            var entity = _skpiService.GetManay(r => r.SD_EKPI_ID == id).ToList();
            
            return Content(new 
            {
                KPI_ID=primaryKey,
                FEN_ZI = entity.FirstOrDefault(r=>r.SITEM_TYPE==1)?.SITEM_ID,
                FEN_MU = entity.FirstOrDefault(r => r.SITEM_TYPE == 2)?.SITEM_ID,
                Z_RESULT_TYPE = entity.FirstOrDefault(r => r.SITEM_TYPE == 1)?.RESULT_TYPE,
                M_RESULT_TYPE = entity.FirstOrDefault(r => r.SITEM_TYPE == 2)?.RESULT_TYPE,
                Z_NAME=entity.FirstOrDefault(r => r.SITEM_TYPE == 1)?.SITEM_DESC,
                M_NAME = entity.FirstOrDefault(r => r.SITEM_TYPE == 2)?.SITEM_DESC,
            }.ToJson());
        }
    }
}