using Elight.Entity;
using Elight.Infrastructure;
using Elight.Web.Controllers;
using Elight.Web.Filters;
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
using TestingCenterSystem.Models.PDP;
using TestingCenterSystem.Service.comm;
using TestingCenterSystem.Service.comm.Interface;
using TestingCenterSystem.Service.KPI;
using TestingCenterSystem.Service.KPI.Interface;
using TestingCenterSystem.Service.Project;
using TestingCenterSystem.Service.Project.Interface;
using TestingCenterSystem.Utils;
using TestingCenterSystem.ViewModels.Project;
using TestingCenterSystem.ViewModels.SD;
namespace Elight.Web.Areas.TestCenterSystem.Controllers
{
    [LoginChecked]
    public class SDController : BaseController
    {
        private static readonly ISdCatConfService _sdCatConfService = new SdCatConfService();
        private static ISDService _sdService = new SDService();
        private static readonly IProjectService _projectService = new ProjectService();
        private static readonly IKPIService _kpiService = new KPIService();//(r => r.SD_ID == ProjectProvider.Instance.Current.SD_ID, x => x.ORDER_NO);
        private static readonly IDbConfService _dbConfService = new DbConfService();
        private static Dictionary<Tuple<string, string>, string> ImportResult = new Dictionary<Tuple<string, string>, string>();

        // GET: TestCenterSystem/SD
        public ActionResult Index(string TC_PROJ_ID)
        {
            ViewBag.TC_PROJ_ID = TC_PROJ_ID;
            return View();
        }

        [HttpPost]
        public ActionResult Index(int pageIndex, int pageSize, SDViewModels vm)
        {
            var pageData = _sdService.GetPage(pageIndex, pageSize, vm);
            var result = new Entity.LayPadding<SD_INFOViewModel>()
            {
                result = true,
                msg = "success",
                list = pageData.Items,
                count = pageData.TotalItems
            };
            return Content(result.ToJson());
        }

        [HttpGet]
        public ActionResult Form(string TC_PROJ_ID, string primaryKey)
        {
            var projectId = int.Parse(TC_PROJ_ID);
            var models = _sdService.GetManay(r => r.TC_PROJ_ID == projectId);
            if (models.Count == 0 || models == null)
            {
                ViewBag.count = 1;
            }
            else if (string.IsNullOrEmpty(primaryKey))
            {
                ViewBag.count = models.Count + 1;
            }
            else
            {
                ViewBag.count = models.Count;
            }
            ViewBag.TC_PROJ_ID = TC_PROJ_ID;
            return View();
        }

        [HttpGet]
        public ActionResult Switch()
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
        public ActionResult Switch(int primaryKey)
        {
            var model = _sdService.Switch(primaryKey);
            return Success(model.ToString());
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="model">SD_TYPE_CODE:0:专病，1:全院，2:专科</param>
        /// <returns></returns>
        public ActionResult Form(SD_INFOViewModel model)
        {
            try
            {
                var service = new SDService(r => r.TC_PROJ_ID == model.TC_PROJ_ID, x => x.ORDER_NO);
                var projType = (_projectService.Get(r => r.TC_PROJ_ID == model.TC_PROJ_ID) ?? new PDP_PROJECT()).TC_PROJ_TYPE;
                if (_sdService.Exists(r => r.SD_ID == model.SD_ID))
                {
                    var oldValue = _sdService.Get(r => r.SD_ID == model.SD_ID);
                    var newValue = new SD_INFO
                    {
                        TC_PROJ_TYPE = oldValue.TC_PROJ_TYPE,//ProjectProvider.Instance.Current.TC_PROJ_TYPE,
                        SD_ID = model.SD_ID,
                        TC_PROJ_ID = oldValue.TC_PROJ_ID,//model.TC_PROJ_ID,
                        SD_CODE = model.SD_CODE,
                        SD_NAME = model.SD_NAME,
                        SD_ALIAS = model.SD_ALIAS,
                        ITEM_CAT_PA_CODE = model.ITEM_CAT_PA_CODE,
                        CITEM_CAT_PA_CODE = model.CITEM_CAT_PA_CODE,
                        EKPI_CAT_PA_CODE = model.EKPI_CAT_PA_CODE,
                        ORDER_NO = model.ORDER_NO,
                        VALID_FLAG = model.VALID_FLAG,
                        SD_TYPE_CODE = model.SD_TYPE_CODE,
                        CREATE_USER_ID = oldValue.CREATE_USER_ID,
                        CREATE_DATE = oldValue.CREATE_DATE
                    };
                    var row = _sdService.Update(newValue);
                    service.RefreshOrder();
                    return row > 0 ? Success() : Error();
                }
                else
                {
                    var value = new SD_INFO()
                    {
                        TC_PROJ_TYPE = projType,//ProjectProvider.Instance.Current.TC_PROJ_TYPE,
                        TC_PROJ_ID = model.TC_PROJ_ID,
                        SD_CODE = model.SD_CODE,
                        SD_NAME = model.SD_NAME,
                        SD_ALIAS = model.SD_ALIAS,
                        ITEM_CAT_PA_CODE = model.ITEM_CAT_PA_CODE,
                        CITEM_CAT_PA_CODE = model.CITEM_CAT_PA_CODE,
                        EKPI_CAT_PA_CODE = model.EKPI_CAT_PA_CODE,
                        ORDER_NO = model.ORDER_NO,
                        VALID_FLAG = 1,
                        SD_TYPE_CODE = model.SD_TYPE_CODE
                    };
                    var entity = _sdService.Insert(value);
                    service.RefreshOrder();
                    return entity != null ? Success() : Error();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [HttpPost]
        public ActionResult GetForm(string primaryKey)
        {
            int id = int.Parse(primaryKey);
            var model = _sdService.Get(r => r.SD_ID == id);
            return Content(new SD_INFOViewModel
            {
                SD_ID = model.SD_ID,
                TC_PROJ_ID = model.TC_PROJ_ID,
                TC_PROJ_TYPE = model.TC_PROJ_TYPE,
                SD_CODE = model.SD_CODE,
                SD_NAME = model.SD_NAME,
                SD_ALIAS = model.SD_ALIAS,
                ITEM_CAT_PA_CODE = model.ITEM_CAT_PA_CODE,
                CITEM_CAT_PA_CODE = model.CITEM_CAT_PA_CODE,
                EKPI_CAT_PA_CODE = model.EKPI_CAT_PA_CODE,
                ORDER_NO = model.ORDER_NO,
                VALID_FLAG = model.VALID_FLAG,
                SD_TYPE_CODE = model.SD_TYPE_CODE,
            }.ToJson());
        }

        public ActionResult Delete(string primaryKey)
        {
            var id = int.Parse(primaryKey.Trim());
            var model = _sdService.Get(r => r.SD_ID == id);
            var service = new SDService(r => r.TC_PROJ_ID == model.TC_PROJ_ID, x => x.ORDER_NO);
            int row = _sdService.Delete(r => r.SD_ID == id);
            service.RefreshOrder();
            return row > 0 ? Success() : Error();
        }

        public ActionResult ExportSQL(int primaryKey)
        {
            try
            {
                var path = _sdService.ExportSQL(primaryKey);
                return Success(path);
            }
            catch (Exception e)
            {
                return Error(e.ToString());
            }
        }

        [HttpPost]
        public ActionResult GetITEM_CAT_PA_CODE()
        {
            List<SD_CAT_CONF> listRole = _sdCatConfService.GetManay(r => r.CAT_TYPE_CODE == "1");
            List<SD_CAT_CONF> listGroup = listRole.Distinct(new Compare<SD_CAT_CONF>((x, y) => (null != x && null != y) && (x.CAT_PA_CODE == y.CAT_PA_CODE))).ToList();
            var listTree = new List<TreeSelect>();
            foreach (var item in listGroup)
            {
                TreeSelect model = new TreeSelect();
                model.id = item.CAT_PA_CODE;
                model.text = item.CAT_PA_NAME;
                listTree.Add(model);
            }
            return Content(listTree.ToJson());
        }

        [HttpPost]
        public ActionResult GetCITEM_CAT_PA_CODE()
        {
            List<SD_CAT_CONF> listRole = _sdCatConfService.GetManay(r => r.CAT_TYPE_CODE == "2");
            List<SD_CAT_CONF> listGroup = listRole.Distinct(new Compare<SD_CAT_CONF>((x, y) => (null != x && null != y) && (x.CAT_PA_CODE == y.CAT_PA_CODE))).ToList();
            var listTree = new List<TreeSelect>();
            foreach (var item in listGroup)
            {
                TreeSelect model = new TreeSelect();
                model.id = item.CAT_PA_CODE;
                model.text = item.CAT_PA_NAME;
                listTree.Add(model);
            }
            return Content(listTree.ToJson());
        }

        [HttpPost]
        public ActionResult GetEKPI_CAT_PA_CODE()
        {
            List<SD_CAT_CONF> listRole = _sdCatConfService.GetManay(r => r.CAT_TYPE_CODE == "3");
            List<SD_CAT_CONF> listGroup = listRole.Distinct(new Compare<SD_CAT_CONF>((x, y) => (null != x && null != y) && (x.CAT_PA_CODE == y.CAT_PA_CODE))).ToList();
            var listTree = new List<TreeSelect>();
            foreach (var item in listGroup)
            {
                TreeSelect model = new TreeSelect();
                model.id = item.CAT_PA_CODE;
                model.text = item.CAT_PA_NAME;
                listTree.Add(model);
            }
            return Content(listTree.ToJson());
        }

        [HttpPost]
        public ActionResult CurrentSD()
        {
            //return Success(_sdService.CurrentSD().ToJson());
            return Content(_sdService.CurrentSD().ToJson());
        }

        [HttpPost]
        public ActionResult GetSD_PROJECT()
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
            return Content(listTree.ToJson());
        }
        [HttpGet]
        public ActionResult Import(string primaryKey)
        {
            return View();
        }
        [HttpPost]
        public ActionResult InsertRSNData(HttpPostedFileBase filebase, string IsRsnOrDict, string primaryKey, string TC_PROJ_ID)//zlt HttpPostedFileBase
        {
            try
            {
                if (string.IsNullOrEmpty(primaryKey) || string.IsNullOrEmpty(TC_PROJ_ID))
                {
                    var result = new Tuple<string, string>("catch", "病种id或项目id为空");
                    return Content(result.ToJson());
                }
                DataTable error = null;
                var id = int.Parse(primaryKey);
                var projId = int.Parse(TC_PROJ_ID);
                //var connStr = ConfigurationManager.ConnectionStrings["connStr"].ToString();
                var sdInfo = _sdService.Get(r => r.SD_ID == id) ?? new SD_INFO();
                var dbConfig = _dbConfService.Get(r => r.TC_PROJ_ID == projId && r.DB_CONF_TYPE == 2) ?? new PDP_DB_CONF();
                //if (sdInfo.TC_PROJ_ID == ProjectProvider.Instance.Current.TC_PROJ_ID)
                //{
                var connStr = $"Data Source='" + dbConfig.SERVER_NAME + "';Initial Catalog='" + dbConfig.DB_NAME + "';User ID='" + dbConfig.DB_USER + "';Password='" + dbConfig.DB_PWD + "'";
                //var connStr = $"Data Source='" + ProjectProvider.Instance.Current.UNIT_DATA_SOURCE + "';Initial Catalog='" + ProjectProvider.Instance.Current.UNIT_INITIAL_CATALOG + "';User ID='" + ProjectProvider.Instance.Current.UNIT_USER_ID + "';Password='" + ProjectProvider.Instance.Current.UNIT_PASSWORD + "'";
                var sdCode = sdInfo.SD_CODE;
                var filePath = Server.MapPath(string.Format("~/{0}", "File"));
                var fullPath = Path.Combine(filePath, filebase.FileName);
                filebase.SaveAs(fullPath);
                var excelBll = new ExcelBLL(connStr, primaryKey, sdCode);
                DataSet ds = ExcelOperater.Read(fullPath);
                if (IsRsnOrDict == "0")
                {
                    if (IsPossible(ds))
                    {
                        DataTable dtKpiInfo = new DataTable();
                        dtKpiInfo.Columns.Add("SD_EKPI_ID");
                        dtKpiInfo.Columns.Add("SD_EKPI_NAME");
                        var kpiInfoList = _kpiService.GetManay(r => r.SD_ID == id).ToList();
                        if (kpiInfoList != null && kpiInfoList.Count > 0)
                        {
                            foreach (var kpiInfo in kpiInfoList)
                            {
                                DataRow dr = dtKpiInfo.NewRow();
                                dr["SD_EKPI_ID"] = kpiInfo.SD_EKPI_ID;
                                dr["SD_EKPI_NAME"] = kpiInfo.SD_EKPI_NAME;
                                dtKpiInfo.Rows.Add(dr);
                            }
                            _sdService.ClearData(primaryKey, IsRsnOrDict);
                            error = excelBll.InsertRSNData(ds, OperatorProvider.Instance.Current.RealName, dtKpiInfo);
                        }
                        else
                        {
                            var result = new Tuple<string, string>("catch", "该病种下无KPI数据！");
                            return Content(result.ToJson());
                        }
                    }
                    else
                    {
                        var result = new Tuple<string, string>("catch", "导入的excel文件模板有问题！");
                        return Content(result.ToJson());
                    }
                }
                else
                {
                    var result = new Tuple<string, string>("catch", "该功能暂时未实现！");
                    return Content(result.ToJson());
                }
                //}
                //else
                //{
                //    var result = new Tuple<string, string>("catch", "请切换该项目为当前项目！");
                //    return Content(result.ToJson());
                //}
                if (error != null && error.Rows.Count > 0)
                {
                    var res = error.ToJson().Replace("\r\n", "").Replace(" ", "").Replace("   ", "").Replace("\"", "'");
                    var guid = Guid.NewGuid().ToString();
                    var result = new Tuple<string, string>("error", guid);
                    ImportResult.Add(result, res);
                    return Content(result.ToJson());
                }
                else
                {
                    var result = new Tuple<string, string>("success", null);
                    return Content(result.ToJson());
                }
            }
            catch (Exception ex)
            {
                var result = new Tuple<string, string>("catch", ex.Message);
                return Content(result.ToJson());
            }
        }

        private bool IsPossible(DataSet ds)
        {
            bool isDictRsn = false;
            bool isEkpiRsn = false;
            foreach (DataTable dt in ds.Tables)
            {
                if (dt.TableName.Contains("原因分类"))
                    isDictRsn = true;
                if (dt.TableName.Contains("原因分析"))
                    isEkpiRsn = true;
            }
            if (isDictRsn && isEkpiRsn)
                return true;
            else
                return false;
        }
        [HttpGet]
        public ActionResult Detail(string data)//string data
        {
            ViewBag.result = data;
            return View();
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
        #region 获取数据集
        //protected DataSet GetDataSet(string filePath)
        //{

        //    //string strConn = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + Server.MapPath("~/App_Data/Excel/Model.xls") + ";Extended Properties='Excel 12.0;HDR=Yes;IMEX=1';";
        //    //OleDbConnection objConn = null;
        //    //objConn = new OleDbConnection(strConn);
        //    //objConn.Open();
        //    using (var db = new PDPEntities())
        //    {
        //        DataSet ds = new DataSet();
        //        List<string> List = new List<string> { };
        //        DataTable dtSheetName = db.Database.Connection.GetSchema("table", new string[] { null, null, null, "TABLE" }); //objConn.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, new object[] { null, null, null, "TABLE" });
        //        foreach (DataRow dr in dtSheetName.Rows)
        //        {
        //            if (dr["Table_Name"].ToString().Contains("$") && !dr[2].ToString().EndsWith("$"))
        //            {
        //                continue;
        //            }
        //            string s = dr["Table_Name"].ToString();
        //            List.Add(s);
        //        }
        //        try
        //        {
        //            for (int i = 0; i < List.Count; i++)
        //            {
        //                ds.Tables.Add(List[i]);
        //                string SheetName = List[i];
        //                string strSql = "select * from [" + SheetName + "]";
        //                OleDbDataAdapter odbcCSVDataAdapter = new OleDbDataAdapter(strSql, db.Database.Connection.ConnectionString); //new OleDbDataAdapter(strSql, objConn);
        //                DataTable dt = ds.Tables[i];
        //                odbcCSVDataAdapter.Fill(dt);
        //            }
        //            //objConn.Close();
        //            return ds;
        //        }
        //        catch (Exception ex)
        //        {
        //            return null;
        //        }
        //    }
        //}
        #endregion
    }
}