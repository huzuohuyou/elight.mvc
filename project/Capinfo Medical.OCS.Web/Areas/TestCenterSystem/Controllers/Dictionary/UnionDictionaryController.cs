using Elight.Infrastructure;
using Elight.Web.Controllers;
using ExcelAspose;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Elight.Web.Filters;
using TestingCenterSystem.Service.Dict;
using TestingCenterSystem.Service.Dict.Interface;
using TestingCenterSystem.Service.Dictionary;
using TestingCenterSystem.ViewModels.Project;

namespace Elight.Web.Areas.TestCenterSystem.Controllers.Dictionary
{
    [LoginChecked]
    public class UnionDictionaryController : BaseController
    {
        private static readonly IUnionDictionaryService _unionDictionaryService = new UnionDictionaryService();
        private static readonly IOthTermMapService _othTermMapService = new OthTermMapService();
        private static Dictionary<Tuple<string, string>, string> ImportResult = new Dictionary<Tuple<string, string>, string>();
        // GET: TestCenterSystem/Dictionary
        public ActionResult Index()
        {
            return View();
        }
        [HttpGet]
        public ActionResult GetCheckTestList(int type)
        {
            var pageData = _unionDictionaryService.GetTERMPage(type);
            var result = new Dictionary<string, object>() {
                { "code", 0},
                { "msg","success"},
                { "data", pageData.Items}
            };
            return Content(result.ToJson());
        }
        [HttpGet]
        public ActionResult GetDrugList()
        {
            var pageData = _unionDictionaryService.GetDurgPage();
            var result = new Dictionary<string, object>() {
                { "code", 0},
                { "msg","success"},
                { "data", pageData.Items}
            };
            return Content(result.ToJson());
        }

        [HttpPost]
        public ActionResult InsertDictionaryData(HttpPostedFileBase file)
        {
            try
            {
                var filePath = Server.MapPath(string.Format("~/{0}", "File"));
                var fullPath = Path.Combine(filePath, file.FileName);
                file.SaveAs(fullPath);
                DataSet ds = _unionDictionaryService.ExcelRead(fullPath);
                DataTable dtError= _unionDictionaryService.ImportDictionaryData(ds);
             
                if (dtError != null && dtError.Rows.Count > 0)
                {
                    var res = dtError.ToJson().Replace("\r\n", "").Replace(" ", "").Replace("   ", "").Replace("\"", "'");
                    var guid = Guid.NewGuid().ToString();
                    var result = new Tuple<string, string>("error", guid);
                    ImportResult.Add(result, res);
                    return Content(result.ToJson());
                }
                else
                {
                    return Content(dtError.ToJson());
                }

            }
            catch (Exception ex)
            {
                var result = new Tuple<string, string>("catch", ex.Message);
                return Content(result.ToJson());
                
            }
        }

        [HttpPost]
        public ActionResult GetErrorInfo(string resultdata)
        {
            var errorData = new Tuple<string, string>("error", resultdata);
            var data = ImportResult[errorData];
            ImportResult.Clear();
            var model = JsonConvert.DeserializeObject<DataTable>(data);
            var result = new Dictionary<string, object>()
            {
                { "code", 0},
                { "msg","success"},
                { "data", model},
                { "count", model==null?0:model.Rows.Count}
            };
            return Content(result.ToJson());
        }
        [HttpGet]
        public ActionResult ErrorDetail(string data)
        {
            ViewBag.result = data;
            return View();
        }




        public ActionResult ExportDictTemplate()
        {
            var filePath = Server.MapPath(string.Format("~/{0}", "File"));
            string fullPath = Path.Combine(filePath, "DICTIONARY.xlsx");
            _unionDictionaryService.ExportDictData(fullPath);
            return Content(("../../File/DICTIONARY.xlsx").ToJson());
        }
    }
}