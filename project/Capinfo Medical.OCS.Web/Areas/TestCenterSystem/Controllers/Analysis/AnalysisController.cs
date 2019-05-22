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
using TestingCenterSystem.Service.Project;
using TestingCenterSystem.Service.Project.Interface;
using TestingCenterSystem.ViewModels.Project;

namespace Elight.Web.Areas.TestCenterSystem.Controllers.Analysis
{
    [LoginChecked]
    public class AnalysisController : BaseController
    {
        private static readonly IProjectService _projectService = new ProjectService();
        // GET: TestCenterSystem/Analysis
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Default()
        {
            List<PDP_PROJECT> listRole = _projectService.GetManay(r => r.TC_PROJ_ID != -1);
            var listTree = new List<TreeSelect>();
            foreach (var item in listRole)
            {
                TreeSelect model = new TreeSelect();
                model.id = item.TC_PROJ_ID.ToString();
                model.text = item.TC_PROJ_NAME;
                listTree.Add(model);
            }
            ViewBag.ProjectInfo = listTree;
            return View();
        }

        [HttpPost]
        public ActionResult GetSDList(string primaryKey)
        {
            var listTree = new List<TreeSelect>();
            try
            {
                if (!primaryKey.IsNullOrEmpty() && primaryKey != "all")
                {
                    var projId = int.Parse(primaryKey);
                    List<SD_INFO> listRole = _sdService.GetManay(r => r.TC_PROJ_ID == projId && r.SD_TYPE_CODE == 0);
                    foreach (var item in listRole)
                    {
                        TreeSelect model = new TreeSelect();
                        model.id = item.SD_ID.ToString();
                        model.text = item.SD_NAME;
                        listTree.Add(model);
                    }
                }
                return Content(listTree.ToJson());
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public ActionResult GetStatList(string keyWord)
        {
            var resultData = new Tuple<bool, object>(true, null);
            var projId = keyWord.Split(';')[0].ToString();
            var sdId = keyWord.Split(';')[1].ToString();
            if (projId.IsNullOrEmpty())
                resultData = _projectService.GetProjStatisticList();
            else
                resultData = _sdService.GetSDStatisticList(projId,sdId);
            return Content(resultData.ToJson());

            #region V1.3
            //var resultData = _projectService.GetList(keyWord);
            //return Content(resultData.ToJson()); 
            #endregion
        }

    }
}