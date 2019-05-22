using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Elight.Entity;
using System.Web.Mvc;
using Elight.Infrastructure;
using Elight.Web.Controllers;
using TestingCenterSystem.Models.PDP;
using TestingCenterSystem.Service.comm;
using TestingCenterSystem.Service.comm.Interface;
using TestingCenterSystem.Service.InGroup;
using TestingCenterSystem.Service.Project;
using TestingCenterSystem.ViewModels.Project;
using TestingCenterSystem.Service.Project.Interface;
using System.Data.Entity.Core.EntityClient;
using System.Data;
using TestingCenterSystem.Models.Unit;
using Elight.Web.Filters;

namespace Elight.Web.Areas.TestCenterSystem.Controllers
{
    [LoginChecked]
    public class ProjectController : BaseController
    {
        private readonly ITeamService _service = new TeamService();
        private readonly IDbConfService _dbConfService = new DbConfService();
        private readonly IProjectService _projectService = new ProjectService();
        private readonly IProcStateService _procStateService = new ProcStateService();
        private readonly ISDService _sdService = new SDService();
        private readonly ISDCDRInfoService _sdCDRInfoService = new SDCDRInfoService();
        //private const string ConnctStr = "metadata=\"res://*/Unit.UTModel.csdl|res://*/Unit.UTModel.ssdl|res://*/Unit.UTModel.msl\";provider=System.Data.SqlClient;provider connection string=\"Data Source = {0};initial catalog={1}; User Id = {2}; Password = {3}; MultipleActiveResultSets = True; App = EntityFramework\";";

        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Index(int pageIndex, int pageSize, string keyWord)
        {
            var teamPage = _service.SelectTeam(pageIndex, pageSize, keyWord);
            var models = new LayPadding<TeamViewModel>
            {
                result = true,
                msg = "success",
                count = teamPage.TotalItems,
                list = teamPage.Items
            };
            return Content(models.ToJson());
        }

        [HttpPost]
        public ActionResult Delete(int projectID)
        {
            //判断是否存在病种，若存在则提示
            var sdCount = _sdService.GetManay(sd => sd.TC_PROJ_ID == projectID).Count;
            if (sdCount > 0)
                return Error("该项目下存在病种，无法删除！");
            _dbConfService.Delete(db => db.TC_PROJ_ID == projectID);
            var result = _service.DeleteTeam(projectID);
            return result > 0 ? Success() : Error();
        }
        [HttpGet]
        public ActionResult AddForm()
        {
            return View();
        }
        #region 添加或修改项目
        /// <summary>
        /// 添加或修改项目
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public ActionResult Form(ProjectDataBaseViewModel model)
        {
            try {
                var row = 0;
                PDP_PROJECT result = null;
                var projectModel = new PDP_PROJECT()
                {
                    TC_PROJ_ID = model.TC_PROJ_ID,
                    TC_PROJ_CODE = model.TC_PROJ_CODE,
                    TC_PROJ_NAME = model.TC_PROJ_NAME,
                    TC_PROJ_TYPE = model.TC_PROJ_TYPE,
                    IS_COMMON = 0
                };
                //保存或修改并更新到数据库
                if (_projectService.Exists(r => r.TC_PROJ_ID == model.TC_PROJ_ID))
                {
                    row = _projectService.Update(projectModel);
                    result = _projectService.Get(pro => pro.TC_PROJ_ID == projectModel.TC_PROJ_ID);
                }
                else
                {
                    result = _projectService.Insert(projectModel);
                    row = result != null ? 1 : 0;
                }
                var cdrDbConf = new PDP_DB_CONF()
                {
                    DB_CONF_ID = model.CDR_DB_CONF_ID,
                    SERVER_NAME = model.CDR_SERVER_NAME,
                    DB_NAME = model.CDR_DB_NAME,
                    DB_USER = model.CDR_DB_USER,
                    DB_PWD = model.CDR_DB_PWD,
                    DB_CONF_TYPE = _dbConfService.CDR_CONF_FLAG(),
                    TC_PROJ_ID = result.TC_PROJ_ID
                };
                row += UpdateDbConf(cdrDbConf, result.TC_PROJ_ID);
                var unitDbConf = new PDP_DB_CONF()
                {
                    DB_CONF_ID = model.UNIT_DB_CONF_ID,
                    SERVER_NAME = model.UNIT_SERVER_NAME,
                    DB_NAME = model.UNIT_DB_NAME,
                    DB_USER = model.UNIT_DB_USER,
                    DB_PWD = model.UNIT_DB_PWD,
                    DB_CONF_TYPE = _dbConfService.UNIT_CONF_FLAG(),
                    TC_PROJ_ID = result.TC_PROJ_ID
                };
                if (ProjectProvider.Instance.Current.TC_PROJ_ID == model.TC_PROJ_ID)
                    _sdService.Switch(ProjectProvider.Instance.Current.SD_ID);
                row += UpdateDbConf(unitDbConf, result.TC_PROJ_ID);

                //更新或插入SD_CDR_INFO表
                if (model.TC_PROJ_ID == 0)
                    model.TC_PROJ_ID = result.TC_PROJ_ID;
                if (_sdCDRInfoService.Exists(r => r.PROJECT_ID == model.TC_PROJ_ID && r.CDR_NAME == model.CDR_DB_NAME))
                    row += _sdCDRInfoService.Update(_sdCDRInfoService.Get(r => r.PROJECT_ID == model.TC_PROJ_ID && r.CDR_NAME == model.CDR_DB_NAME));
                else
                {
                    var CDRInsertResult = _sdCDRInfoService.Insert(new SD_CDR_INFO()
                    {
                        CDR_NAME = model.CDR_DB_NAME,
                        PROJECT_ID = model.TC_PROJ_ID
                    });
                    row += CDRInsertResult == null ? 0 : 1;
                }
                return row == 4 ? Success() : Error("项目或数据库连接内容保存失败！");
            }
            catch (Exception ex)
            {
                return Error(ex.Message);
            }
        }
        #endregion
        #region 更新数据库连接（内部方法）
        /// <summary>
        /// 更新数据库连接
        /// </summary>
        /// <param name="model"></param>
        /// <param name="projectId"></param>
        /// <returns></returns>
        private int UpdateDbConf(PDP_DB_CONF model, int projectId)
        {
            var row = 0;
            //保存或修改并更新到数据库
            if (_dbConfService.Exists(r => r.TC_PROJ_ID == projectId && r.DB_CONF_TYPE == model.DB_CONF_TYPE))
                row = _dbConfService.Update(model);
            else
            {
                var entity = _dbConfService.Insert(model);
                row = entity == null ? 0 : 1;
            }
            return row;
        }
        #endregion
        #region CDR数据库连接测试
        /// <summary>
        /// CDR数据库连接测试
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public ActionResult TestCDRConnect(ProjectDataBaseViewModel model)
        {
            var dbModel = new PDP_DB_CONF()
            {
                DB_NAME = model.CDR_DB_NAME,
                SERVER_NAME = model.CDR_SERVER_NAME,
                DB_USER = model.CDR_DB_USER,
                DB_PWD = model.CDR_DB_PWD,
            };
            var dbNameList = _dbConfService.TestConnect(dbModel);
            return dbNameList != null ? Success("连接成功！", dbNameList) : Error("连接失败！");
        }
        #endregion
        #region UNIT数据库连接测试
        /// <summary>
        /// UNIT数据库连接测试
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public ActionResult TestUnitConnect(ProjectDataBaseViewModel model)
        {
            var dbModel = new PDP_DB_CONF()
            {
                DB_NAME = model.UNIT_DB_NAME,
                SERVER_NAME = model.UNIT_SERVER_NAME,
                DB_USER = model.UNIT_DB_USER,
                DB_PWD = model.UNIT_DB_PWD,
            };
            var dbNameList = _dbConfService.TestConnect(dbModel);
            return dbNameList != null ? Success("连接成功！", dbNameList) : Error("连接失败！");
        }
        #endregion
        public ActionResult ManageIndex()
        {
            return View();
        }
        #region 项目数据加载
        /// <summary>
        /// 项目数据加载
        /// </summary>
        /// <param name="projectId"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult GetForm(string primaryKey)
        {
            var id = int.Parse(primaryKey);
            var project = _projectService.Get(pro => pro.TC_PROJ_ID == id);
            var cdrflag = _dbConfService.CDR_CONF_FLAG();
            var cdrConnect = _dbConfService.Get(db => db.TC_PROJ_ID == id && db.DB_CONF_TYPE == cdrflag);
            var unitflag = _dbConfService.UNIT_CONF_FLAG();
            var unitConnect = _dbConfService.Get(db => db.TC_PROJ_ID == id && db.DB_CONF_TYPE == unitflag);
            var model = new ProjectDataBaseViewModel()
            {
                TC_PROJ_ID = id,
                TC_PROJ_CODE = project.TC_PROJ_CODE,
                TC_PROJ_NAME = project.TC_PROJ_NAME,
                TC_PROJ_TYPE = project.TC_PROJ_TYPE,
                CDR_DB_CONF_ID = cdrConnect == null ? 0 : cdrConnect.DB_CONF_ID,
                CDR_SERVER_NAME = cdrConnect == null ? "" : cdrConnect.SERVER_NAME,
                CDR_DB_NAME = cdrConnect == null ? "" : cdrConnect.DB_NAME,
                CDR_DB_USER = cdrConnect == null ? "" : cdrConnect.DB_USER,
                CDR_DB_PWD = cdrConnect == null ? "" : cdrConnect.DB_PWD,
                CDR_DB_CONF_TYPE = cdrConnect == null ? 0 : cdrConnect.DB_CONF_TYPE,
                IS_COMMON = project.IS_COMMON,
                UNIT_DB_CONF_ID = unitConnect == null ? 0 : unitConnect.DB_CONF_ID,
                UNIT_SERVER_NAME = unitConnect == null ? "" : unitConnect.SERVER_NAME,
                UNIT_DB_USER = unitConnect == null ? "" : unitConnect.DB_USER,
                UNIT_DB_PWD = unitConnect == null ? "" : unitConnect.DB_PWD,
                UNIT_DB_NAME = unitConnect == null ? "" : unitConnect.DB_NAME,
                UNIT_DB_CONF_TYPE = unitConnect == null ? 0 : unitConnect.DB_CONF_TYPE
            };
            return Content(model.ToJson());
        }
        #endregion
        #region 导出项目脚本
        /// <summary>
        /// 导出数据项脚本
        /// </summary>
        /// <returns></returns>
        public ActionResult ExportSql(string primaryKey)
        {
            var projectId = int.Parse(primaryKey);//_projectService.GetCurrentSD().TC_PROJ_ID;
            var sqlStr = _projectService.ExportSQL(projectId);
            var data = new Dictionary<string, string> { { "data", sqlStr } };
            var content = data.ToJson();
            byte[] bytes = Encoding.Default.GetBytes(content);
            var decode = Encoding.Default.GetString(bytes);
            return Content(decode);
        }
        #endregion

        #region 判断用户访问数据库权限
        /// <summary>
        /// 判断用户访问数据库权限
        /// </summary>
        /// <param name="model"></param>
        /// <param name="flag"></param>
        /// <returns></returns>
        public ActionResult JudgeAuthority(ProjectDataBaseViewModel model, string flag)
        {
            #region 判断连接
            //string connect = null;
            //var message = new Tuple<bool, string>(false, null);
            //if (flag == "CDR" && !model.CDR_DB_NAME.IsNullOrEmpty())
            //    connect = string.Format(ConnctStr, model.CDR_SERVER_NAME, model.CDR_DB_NAME, model.CDR_DB_USER, model.CDR_DB_PWD);
            //else if (flag == "UNIT" && !model.UNIT_DB_NAME.IsNullOrEmpty())
            //    connect = string.Format(ConnctStr, model.UNIT_SERVER_NAME, model.UNIT_DB_NAME, model.UNIT_DB_USER, model.UNIT_DB_PWD);
            //if (!connect.IsNullOrEmpty())
            //{
            //    var conn = new EntityConnection(connect);
            //    try
            //    {
            //        conn.Open();
            //        message = new Tuple<bool, string>(true, "数据库测试成功！");// { true,"数据库测试成功！" };
            //    }
            //    catch (Exception ex)
            //    {
            //        conn.Close();
            //        message = new Tuple<bool, string>(false, ex.InnerException.Message);
            //    }
            //    if (conn.State == ConnectionState.Open)
            //        conn.Close();
            //}
            //else
            //    message = new Tuple<bool, string>(false, "数据库名称为空！");
            //return Content(message.ToJson());
            #endregion

            var result = _projectService.JudgeAuthority(model, flag);
            return Content(result.ToJson());
        }
        #endregion
    }
}