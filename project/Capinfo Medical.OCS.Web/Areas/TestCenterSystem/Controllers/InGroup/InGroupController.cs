using Elight.Entity;
using Elight.Infrastructure;
using Elight.Web.Controllers;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq.Expressions;
using System.Web.Mvc;
using TestingCenterSystem.Models.PDP;
using TestingCenterSystem.Service.comm.Interface;
using TestingCenterSystem.Service.InGroup;
using TestingCenterSystem.Service.InGroup.Interface;
using TestingCenterSystem.ViewModels.InGroup;
using TestingCenterSystem.ViewModels.Project;
using TestingCenterSystem.Service.Comm.Interface;
using TestingCenterSystem.Service.Comm;
using System.Linq;
using TestingCenterSystem.ViewModels.Comm;
using System.Text;
using System.Web;
using TestingCenterSystem.Service.Sys.Interface;
using TestingCenterSystem.Service.Sys;
using Elight.Web.Filters;

namespace Elight.Web.Areas.TestCenterSystem.Controllers
{
    [LoginChecked]
    public class InGroupController : BaseController
    {
        private readonly IInGroupService _ingroupService = new InGroupService();
        private readonly IProcStateService _inGroupStateService = new ProcStateService();
        private readonly IInGroupResultService _inGroupResultService = new InGroupResultService();
        private static readonly IUserService _userService = new UserService();

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Index2()
        {
            return View();
        }

        #region 获取查询数据1.0版本
        [HttpPost]//, AuthorizeChecked
        public ActionResult Index(int pageIndex, int pageSize)//, string keyWord
        {
            var pageData = _ingroupService.GetList(pageIndex, pageSize, "", "");
            var result = new LayPadding<InGroupEntity>()
            {
                result = true,
                msg = "success",
                list = pageData.Items,
                count = pageData.TotalItems
            };
            return Content(result.ToJson());
        }
        #endregion

        #region 获取查询数据2.0版本
        [HttpGet]
        public ActionResult GetList(int page, int limit, string field, string type)
        {
            var pageData = _ingroupService.GetList(page, limit, field, type);
            var result = new Dictionary<string, object>()
            {
                { "code", 0},
                { "msg","success"},
                { "data", pageData.Items},
                { "count", pageData.TotalItems}
            };
            return Content(result.ToJson());
        }
        #endregion

        [HttpGet]//, AuthorizeChecked
        public ActionResult Form(string primaryKey)
        {
            var count = _ingroupService.GetManay(r => r.SD_ID == ProjectProvider.Instance.Current.SD_ID).Count() > 0 ? 1 : 0;
            ViewBag.count = string.IsNullOrEmpty(primaryKey) ? count + 1 : count;
            return View();
        }

        [HttpGet]//, AuthorizeChecked
        public ActionResult Detail(string primaryKey)
        {
            return View();
        }

        [HttpPost]//, AuthorizeChecked, ValidateAntiForgeryToken
        public ActionResult Form(SD_FILTER_INFO model)
        {
            model.SD_FILTER_ALGO = HttpUtility.UrlDecode(model.SD_FILTER_ALGO, Encoding.Default);
            if (model.SD_FILTER_ID == 0)
            {
                try
                {
                    var primaryKey = _ingroupService.Insert(model);
                    _procLogService.Insert(new PDP_PROC_LOG()
                    {
                        SD_ID = primaryKey.SD_ID,
                        PROC_CAT_CODE = _procLogService.InGroupCatCode(),
                        PROC_CONTENT_ID = primaryKey.SD_FILTER_ID.ToString(),
                        PROC_URL = null,
                        START_TIME = null,
                        END_TIME = null,
                        PATIENT_ID = null,
                        PROC_STAT_CODE = 1
                    });
                    return Success();
                }
                catch (Exception ex)
                {
                    return Error(ex.Message);
                }
            }
            else
            {
                var primaryKey = _ingroupService.EditOrUpdate(model);
                return primaryKey >= 0 ? Success() : Error();
            }
        }

        [HttpPost]
        public ActionResult GetForm(string primaryKey)
        {
            int id = int.Parse(primaryKey);
            Expression<Func<SD_FILTER_INFO, bool>> ex = x => x.SD_FILTER_ID == id;
            var entity = _ingroupService.Get(ex);
            return Content(entity.ToJson());
        }

        [HttpPost]//, AuthorizeChecked
        public ActionResult Delete(string primaryKey)
        {
            var stat = _inGroupStateService.Get(ex => ex.SD_ID == ProjectProvider.Instance.Current.SD_ID && ex.PROC_CONTENT_ID == primaryKey && ex.PROC_CAT_CODE == "1").PROC_STAT_CODE;
            if (stat == "1" || stat == "3")//当状态为未执行或已清空时，才可以删除
            {
                int count = _ingroupService.Delete(primaryKey);
                return count >= 0 ? Success() : Error();
            }
            else
            {
                return Error("尚未清库，请清库后再进行删除");
            }
        }

        public ActionResult Switch(string primaryKey)
        {
            int id = int.Parse(primaryKey);
            Expression<Func<SD_FILTER_INFO, bool>> ex = x => x.SD_FILTER_ID == id;
            var entity = _ingroupService.Get(ex);
            entity.VALID_FLAG = entity.VALID_FLAG == 1 ? 0 : 1;
            int count = _ingroupService.Update(entity);
            return count > 0 ? Success() : Error();
        }

        public ActionResult Config(string primaryKey)
        {
            int id = int.Parse(primaryKey);
            var entity = _ingroupService.Get(filter => filter.SD_FILTER_ID == id);
            ViewBag.SD_FILTER_NAME = entity.SD_FILTER_NAME;
            ViewBag.SD_FILTER_CODE = entity.SD_FILTER_CODE;
            ViewBag.SD_FILTER_ALGO = entity.SD_FILTER_ALGO;

            var prolog = _procLogService.GetManay(log => log.SD_ID == ProjectProvider.Instance.Current.SD_ID && log.PROC_CAT_CODE == "1" && log.PROC_CONTENT_ID == primaryKey).OrderByDescending(r => r.UPD_DATE).FirstOrDefault() ?? new PDP_PROC_LOG();
            ViewBag.startTime = prolog.START_TIME.ToDateString();
            ViewBag.endTime = prolog.END_TIME.ToDateString();
            ViewBag.patientId = prolog.PATIENT_ID;
            ViewBag.url = prolog.PROC_URL;
            ViewBag.realurl = prolog.PROC_URL;

            ViewBag.ProgressGuid = Guid.NewGuid().ToString("N");//获取进程guid zlt
            var state = _inGroupStateService.Get(ex => ex.SD_ID == ProjectProvider.Instance.Current.SD_ID && ex.PROC_CONTENT_ID == primaryKey.ToString() && ex.PROC_CAT_CODE == "1");
            if (state.PROC_STAT_CODE != "1")
            {
                ViewBag.UserName = _userService.GetUserNameById(state.UPD_USER_ID);
                ViewBag.UpdateDate = state.UPD_DATE;
            }
            else//未执行
            {
                ViewBag.UserName = null;
                ViewBag.UpdateDate = null;
            }
            ViewBag.state = state.PROC_STAT_CODE;
            return View();
        }

        public ActionResult Config2(string primaryKey)
        {
            try
            {
                int id = int.Parse(primaryKey);
                var entity = _ingroupService.Get(filter => filter.SD_FILTER_ID == id);
                ViewBag.SD_FILTER_NAME = entity.SD_FILTER_NAME;
                ViewBag.SD_FILTER_CODE = entity.SD_FILTER_CODE;
                ViewBag.SD_FILTER_ALGO = entity.SD_FILTER_ALGO;
                var prolog = _procLogService.GetManay(log => log.SD_ID == ProjectProvider.Instance.Current.SD_ID && log.PROC_CAT_CODE == "1" && log.PROC_CONTENT_ID == primaryKey).OrderByDescending(r => r.UPD_DATE).FirstOrDefault() ?? new PDP_PROC_LOG();
                ViewBag.startTime = prolog.START_TIME == null ? null : prolog.START_TIME.ToDateString();
                ViewBag.endTime = prolog.END_TIME == null ? null : prolog.END_TIME.ToDateString();
                ViewBag.patientId = prolog.PATIENT_ID;
                ViewBag.url = prolog.PROC_URL;
                ViewBag.realurl = prolog.PROC_URL;
                ViewBag.ProgressGuid = Guid.NewGuid().ToString("N");//获取进程guid zlt
                var state = _inGroupStateService.Get(ex => ex.SD_ID == ProjectProvider.Instance.Current.SD_ID && ex.PROC_CONTENT_ID == primaryKey.ToString() && ex.PROC_CAT_CODE == "1") ?? new PDP_PROC_STAT();
                if (state.PROC_STAT_CODE != "1")
                {
                    ViewBag.UserName = _userService.GetUserNameById(state.UPD_USER_ID);
                    ViewBag.UpdateDate = state.UPD_DATE;
                }
                else//未执行
                {
                    ViewBag.UserName = null;
                    ViewBag.UpdateDate = null;
                }
                ViewBag.state = state.PROC_STAT_CODE;
                return View();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #region  入组测试1.0版本
        /// <summary>
        /// 入组测试
        /// </summary>
        /// <param name="model"></param>
        /// <param name="state">ture:测试，false:取消</param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult Test(TestParamViewModel model, bool state)
        {
            if (state)//测试
            {
                try
                {
                    var id = int.Parse(model.primaryKey);
                    var testResult = _ingroupService.GroupRuleTest(model);
                    if (testResult.Item1)
                    {
                        var filterInfo = _ingroupService.Get(r => r.SD_ID == ProjectProvider.Instance.Current.SD_ID && r.SD_FILTER_ID == id);
                        _procLogService.Insert(new PDP_PROC_LOG
                        {
                            SD_ID = ProjectProvider.Instance.Current.SD_ID,
                            PROC_CAT_CODE = _procLogService.InGroupCatCode(),
                            PROC_CONTENT_ID = model.primaryKey,
                            START_TIME = model.startTime == DateTime.MinValue ? (DateTime?)null : model.startTime,//Convert.ToDateTime("1753 年 1 月 1 日")
                            END_TIME = model.endTime == DateTime.MinValue ? (DateTime?)null : model.endTime,
                            PATIENT_ID = model.patientId,
                            PROC_URL = model.url
                        });
                        return Success(testResult.Item2, testResult.Item3);//, testResult.Item3

                    }
                    else if (!testResult.Item2.Contains("api接口调用失败"))//api接口调用失败的日志已在调用的时候写入日志
                    {
                        _errorLogService.Insert(new PDP_ERROR_LOG
                        {
                            SD_ID = ProjectProvider.Instance.Current.SD_ID,
                            ERROR_CAT_CODE = "1",
                            SD_CPAT_NO = model.patientId,
                            PROC_TYPE = ConvertExeFlag("test"),
                            PROC_URL = model.url,
                            ERR_CONTENT = testResult.Item2,
                        });
                        return Error(testResult.Item2);
                    }
                    else
                        return Error(testResult.Item2);
                }
                catch (Exception ex)
                {
                    return Error(ex.Message);
                }
            }
            else//取消
            {
                //_ingroupService.CancelTasks(model.clientKey);
                return Error("该功能暂时无法使用");
            }
        }
        #endregion

        #region 入组执行
        /// <summary>
        /// 入组执行,使用多线程 rabbitmq
        /// </summary>
        /// <param name="model"></param>
        /// <param name="state">true:执行，false:取消</param>
        /// <returns></returns>
        [HttpPost]//, AsyncTimeout(60000)
        public ActionResult Execute(TestParamViewModel model, bool state)
        {
            try
            {
                if (state)
                {
                    int id = int.Parse(model.primaryKey);
                    _ingroupService.GroupRuleExecute(model);
                    //执行成功，插入项目日志
                    _procLogService.Insert(new PDP_PROC_LOG
                    {
                        SD_ID = ProjectProvider.Instance.Current.SD_ID,
                        PROC_CAT_CODE = _procLogService.InGroupCatCode(),
                        PROC_CONTENT_ID = model.primaryKey,
                        START_TIME = model.startTime == DateTime.MinValue ? (DateTime?)null : model.startTime,//prolog.START_TIME,
                        END_TIME = model.endTime == DateTime.MinValue ? (DateTime?)null : model.endTime,//prolog.END_TIME,
                        PATIENT_ID = model.patientId == null ? null : model.patientId.Trim(),//prolog.PATIENT_ID,
                        PROC_URL = model.url,//url
                        PROC_STAT_CODE = 2,
                    });
                    return Success("正在执行入组！");//入组执行中，可以关闭该窗口，若查看进度，可再次打开该窗口
                }
                else
                {
                    _ingroupService.CancelTasks(model.clientKey);
                    return null;
                }
            }
            catch (Exception ex)
            {
                _errorLogService.Insert(new PDP_ERROR_LOG
                {
                    SD_ID = ProjectProvider.Instance.Current.SD_ID,
                    ERROR_CAT_CODE = "1",
                    SD_CPAT_NO = model.patientId,
                    PROC_TYPE = ConvertExeFlag("execute"),
                    PROC_URL = model.url,
                    ERR_CONTENT = ex.ToString(),
                });
                return Error(ex.InnerException.Message);
            }
        }
        #endregion

        public ActionResult AfterExecute(string primaryKey)
        {
            //int id = int.Parse(primaryKey);
            //_inGroupStateService.InGroupDoExecute(ProjectProvider.Instance.Current.SD_ID, primaryKey);
            var proc = _inGroupStateService.Get(r => r.PROC_CONTENT_ID == primaryKey && r.SD_ID == ProjectProvider.Instance.Current.SD_ID && r.PROC_CAT_CODE == "1");
            return Success("执行成功！", new { UserName = _userService.GetUserNameById(proc.UPD_USER_ID), UpdateDate = proc.UPD_DATE });
        }

        #region 导出
        /// <summary>
        /// 导出
        /// </summary>
        /// <param name="primaryKey"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult Export(string primaryKey)//, TestParamViewModel model
        {
            var data = new Dictionary<string, List<InGroupResultViewModel>>();
            int filterId = int.Parse(primaryKey);
            try
            {
                var pageData = _inGroupResultService.GetList(null, primaryKey);//.GetResultList(primaryKey);//model.patientId,model.startTime,model.endTime,
                var title = new InGroupResultViewModel()
                {
                    SD_CPAT_NO = "入组样本ID",
                    SD_CPAT_DATE = "入组样本时间",
                    PATIENT_ID = "患者ID",
                    PATIENT_NO = "住院号",
                    IN_FLAG = "住院门/急诊标识",
                    BASE_FLAG = "是否入组主记录",
                    IN_DEPT_DATE = "转入科室时间",
                    OUT_DEPT_DATE = "转出科室时间",
                    IN_DATE = "入院时间",
                    OUT_DATE = "出院时间",
                };
                data.Add("title", new List<InGroupResultViewModel> { title });
                data.Add("data", pageData);

                #region 使用后端代码实现导出excel
                //int filterId = int.Parse(primaryKey);
                //var filtername = _ingroupService.Get(e => e.SD_FILTER_ID == filterId).SD_FILTER_NAME;
                //var filename = string.Format("{0}_{1}_{2}.xlsx", ProjectProvider.Instance.Current.SD_NAME, filtername, DateTime.Now.ToString("yyyyMMddhhmmss"));
                //var ok = ExportExcelHelper.DataSetToExcel<InGroupResultViewModel>(pageData, false, filename);
                //if (ok.Item1)
                //{
                //    return Success(ok.Item2);
                //}
                //return Error(ok.Item2);
                #endregion
            }
            catch (Exception ex)
            {
                _errorLogService.Insert(new PDP_ERROR_LOG
                {
                    SD_ID = ProjectProvider.Instance.Current.SD_ID,
                    ERROR_CAT_CODE = "1",
                    PROC_TYPE = ConvertExeFlag("export"),
                    ERR_CONTENT = ex.ToString(),
                });
                data.Add("data", null);
            }
            return Content(data.ToJson());
        }
        #endregion

        #region 锁定
        /// <summary>
        /// 锁定
        /// </summary>
        /// <param name="primaryKey"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult Locking(string primaryKey)
        {
            var id = int.Parse(primaryKey);
            var result = _inGroupResultService.Locking(id);
            if (result.Item1)
            {
                try
                {
                    _inGroupStateService.InGroupLockExecute(ProjectProvider.Instance.Current.SD_ID, primaryKey);
                    var filterInfo = _ingroupService.Get(r => r.SD_ID == ProjectProvider.Instance.Current.SD_ID && r.SD_FILTER_ID == id);
                    if (filterInfo.VALID_FLAG != 1)
                    {
                        filterInfo.VALID_FLAG = 1;
                        _ingroupService.Update(filterInfo);
                    }
                    //执行成功，插入项目日志
                    _procLogService.Insert(new PDP_PROC_LOG
                    {
                        SD_ID = ProjectProvider.Instance.Current.SD_ID,
                        PROC_CAT_CODE = _procLogService.InGroupCatCode(),
                        PROC_CONTENT_ID = primaryKey,
                        START_TIME = null,
                        END_TIME = null,
                        PATIENT_ID = null,
                        PROC_URL = null,
                        PROC_STAT_CODE = 4,
                    });
                    return Success(result.Item2);
                }
                catch (Exception ex)
                {
                    return Error(ex.Message);
                }
            }
            else
            {
                return Error(result.Item2);
            }
        }
        #endregion

        #region 清库
        /// <summary>
        /// 清库
        /// </summary>
        /// <param name="primaryKey"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult Clear(string primaryKey)
        {
            try
            {
                var id = int.Parse(primaryKey);
                var result = _inGroupResultService.ClearUniteData(id, null);
                _procLogService.Insert(new PDP_PROC_LOG
                {
                    SD_ID = ProjectProvider.Instance.Current.SD_ID,
                    PROC_CAT_CODE = _procLogService.InGroupCatCode(),
                    PROC_CONTENT_ID = primaryKey,
                    START_TIME = null,
                    END_TIME = null,
                    PATIENT_ID = null,
                    PROC_URL = null,
                    PROC_STAT_CODE = 3,
                });
                return result.Item1 ? Success(result.Item2, result.Item2.Contains("清库成功") ? new { UserName = OperatorProvider.Instance.Current.RealName, UpdateDate = DateTime.Now } : null) : Error(result.Item2);
            }
            catch (Exception ex)
            {
                return Error(ex.Message);
            }
        }
        #endregion

        #region HttpGet方式请求url
        //private object getPatientInfo(string url, string pNO, string constr)
        //{
        //    try
        //    {
        //        var result = HttpGet(url, string.Format("patientNo={0&connectionString={1}", pNO, constr));
        //        var tuple = JsonConvert.DeserializeObject<object>(result);
        //        return tuple;
        //    }
        //    catch (Exception ex)
        //    {
        //        throw ex;
        //    }
        //}
        //private string HttpGet(string Url, string postDataStr)
        //{
        //    HttpWebResponse response = null;
        //    Stream myResponseStream = null;
        //    StreamReader myStreamReader = null;
        //    try
        //    {
        //        GC.Collect();//垃圾回收站
        //        HttpWebRequest request = (HttpWebRequest)WebRequest.Create(Url + (postDataStr == "" ? "" : "?") + postDataStr);
        //        request.Method = "POST";//GET
        //        request.ServicePoint.ConnectionLimit = 1024;//默认为2个
        //        request.KeepAlive = false;
        //        request.Timeout = 1000000;//100秒
        //        request.ContentType = "text/html;charset=UTF-8";
        //        response = (HttpWebResponse)request.GetResponse();
        //        myResponseStream = response.GetResponseStream();
        //        myStreamReader = new StreamReader(myResponseStream, Encoding.GetEncoding("utf-8"));
        //        string retString = myStreamReader.ReadToEnd();
        //        //myStreamReader.Close();
        //        //myResponseStream.Close();
        //        return retString;
        //    }
        //    catch (Exception ex)
        //    {
        //        //LogHelper.Error("post信息时出错！信息：" + ex.Message);
        //        //return null;
        //        throw ex;
        //    }
        //    finally
        //    {
        //        if (myStreamReader != null)
        //        {
        //            myStreamReader.Close();
        //            myStreamReader.Dispose();
        //        }
        //        if (myResponseStream != null)
        //        {
        //            myResponseStream.Close();
        //            myResponseStream.Dispose();
        //        }
        //        if (response != null)
        //        {
        //            response.Close();
        //            response.Dispose();
        //        }
        //    }
        //}
        #endregion

        #region 获取入组规则信息分页加载到界面 1.0版本
        /// <summary>
        /// 获取入组规则信息分页加载到界面
        /// </summary>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="resultdata"></param>
        /// <returns></returns>
        public ActionResult GetInGroupResultList(int pageIndex, int pageSize, List<InGroupResultViewModel> resultdata)// GetInGroupResultList,List<InGroupResultViewModel> data
        {
            var pageData = _inGroupResultService.GetList(pageIndex, pageSize, "", "", resultdata ?? new List<InGroupResultViewModel>());
            var result = new LayPadding<InGroupResultViewModel>()
            {
                result = true,
                msg = "success",
                list = pageData.Items,
                count = pageData.TotalItems
            };
            return Content(result.ToJson());
        }
        #endregion

        #region 获取入组规则信息分页加载到界面 2.0版本
        /// <summary>
        /// 获取入组规则信息分页加载到界面
        /// </summary>
        /// <param name="page"></param>
        /// <param name="limit"></param>
        /// <param name="field"></param>
        /// <param name="type"></param>
        /// <param name="resultdata"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult GetInGroupList(int page, int limit, string field, string type, List<InGroupResultViewModel> resultdata)// List<InGroupResultViewModel> GetInGroupResultList,List<InGroupResultViewModel> data
        {
            //var resultData= JsonConvert.DeserializeObject<List<InGroupResultViewModel>>(resultdata); 
            var pageData = _inGroupResultService.GetList(page, limit, field, type, resultdata == null || resultdata[0] == null ? new List<InGroupResultViewModel>() : resultdata);
            var result = new Dictionary<string, object>()
            {
                { "code", 0},
                { "msg","success"},
                { "data", pageData.Items},
                { "count", pageData.TotalItems}
            };
            return Content(result.ToJson());
        }
        #endregion

        public ActionResult Progress()
        {
            return View();
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

        public ActionResult SynchData(string startTime, string endTime)
        {
            var start = string.IsNullOrWhiteSpace(startTime) ? DateTime.MinValue : DateTime.Parse(startTime);
            var end = string.IsNullOrWhiteSpace(endTime) ? DateTime.MinValue : DateTime.Parse(endTime);
            var result = _ingroupService.SynchData(start, end);
            return result > 0 ? Success("同步成功！") : Error("同步失败！");
        }
    }
}