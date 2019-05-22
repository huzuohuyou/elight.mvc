using Elight.Entity;
using Elight.Infrastructure;
using Elight.Web.Controllers;
using Elight.Web.Filters;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Web.Mvc;
using TestingCenterSystem.Models.PDP;
using TestingCenterSystem.Service.comm.Interface;
using TestingCenterSystem.Service.InGroup;
using TestingCenterSystem.Service.InGroup.Interface;
using TestingCenterSystem.ViewModels.DataItem;
using TestingCenterSystem.ViewModels.InGroup;
using TestingCenterSystem.ViewModels.Project;
using System.IO;
using System.Net.Security;
using System.Net;
using System.Text;
using TestingCenterSystem.Models.Unit;
using TestingCenterSystem.Utils;
using System.Threading.Tasks;
using System.Threading;
using TestingCenterSystem.Service.Comm.Interface;
using TestingCenterSystem.Service.Comm;
using System.Linq;
using System.Dynamic;
using System.Web;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using TestingCenterSystem.Utils.RabbitMQ;

namespace Elight.Web.Areas.TestCenterSystem.Controllers
{
    public class FilterController : BaseController
    {
        // GET: TestCenterSystem/InGroup
        private readonly IInGroupService _ingroupService = new InGroupService();
        private readonly IProcStateService _inGroupStateService = new ProcStateService();
        private readonly IPatientInfoService _patientInfoService = new PatientInfoService();
        private readonly IInGroupResultService _inGroupResultService = new InGroupResultService();
        private readonly IErrorLogService _iErrorLogService = new ErrorLogService();
        //private readonly IProcLogService _procLogService = new ProcLogService();
        private static int count = 0;
        public static List<InGroupResultViewModel> list = new List<InGroupResultViewModel>();

        //用来存储 进度条的值
        //public static int Num = 0;
        public static Dictionary<string, int> NumList = new Dictionary<string, int>();

        public ActionResult Index()
        {
            return View();
        }
        [HttpPost]//, AuthorizeChecked
        public ActionResult Index(int pageIndex, int pageSize, string keyWord)
        {
            var pageData = _ingroupService.GetList(pageIndex, pageSize, keyWord);
            count = (int)pageData.TotalItems;
            var result = new LayPadding<InGroupEntity>()
            {
                result = true,
                msg = "success",
                list = pageData.Items,
                count = pageData.TotalItems
            };
            return Content(result.ToJson());
        }
        [HttpGet, AuthorizeChecked]
        public ActionResult Form(string primaryKey)
        {
            if (string.IsNullOrEmpty(primaryKey))
            {
                ViewBag.count = count + 1;
            }
            else
            {
                ViewBag.count = count;
            }
            return View();
        }
        [HttpGet, AuthorizeChecked]
        public ActionResult Detail(string primaryKey)
        {
            return View();
        }

        [HttpPost, AuthorizeChecked, ValidateAntiForgeryToken]
        public ActionResult Form(SD_FILTER_INFO model)
        {
            if (model.SD_FILTER_ID == 0)
            {
                var primaryKey = _ingroupService.Insert(model);
                return primaryKey != null ? Success() : Error();
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
        [HttpPost, AuthorizeChecked]
        public ActionResult Delete(string primaryKey)
        {
            var stat = _inGroupStateService.Get(ex => ex.SD_ID == ProjectProvider.Instance.Current.SD_ID && ex.PROC_CONTENT_ID == primaryKey && ex.PROC_CAT_CODE == "1").PROC_STAT_CODE;
            if (stat == "1" || stat == "3")
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
        //public ActionResult ExportSQL()
        //{
        //    //return Success("由项目统一导出SQL");
        //    throw new Exception("由项目统一导出SQL");
        //}

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
                ViewBag.UserName = state.UPD_USER_ID;
                ViewBag.UpdateDate = state.UPD_DATE;
            }
            ViewBag.state = state.PROC_STAT_CODE;
            return View();
        }
        /// <summary>
        /// 入组测试
        /// </summary>
        /// <param name="primaryKey"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult Test(string primaryKey, TestParamViewModel model)
        {
            list = new List<InGroupResultViewModel>();
            var result = GroupRuleTest(model, primaryKey);
            if (result.Item1)
            {
                _procLogService.Insert(new PDP_PROC_LOG
                {
                    SD_ID = ProjectProvider.Instance.Current.SD_ID,
                    PROC_CAT_CODE = _procLogService.InGroupCatCode(),
                    PROC_CONTENT_ID = primaryKey,
                    START_TIME = model.startTime,
                    END_TIME = model.endTime,
                    PATIENT_ID = model.patientId,
                    PROC_URL = model.url
                });
                return Success(result.Item2);
            }
            else
            {
                _iErrorLogService.Insert(new PDP_ERROR_LOG
                {
                    SD_ID = ProjectProvider.Instance.Current.SD_ID,
                    PROC_TYPE = ConvertExeFlag("test"),
                    //PROC_URL =
                    //DLL_NAME =
                    //CLASS_NAME =
                    //FUNC_NAME =
                    ERR_CONTENT = result.Item2,
                });
                list = new List<InGroupResultViewModel>();
                return Error(result.Item2);
            }
        }

        /// <summary>
        /// 入组执行
        /// </summary>
        /// <param name="url"></param>
        /// <param name="primaryKey"></param>
        /// <returns></returns>
        [HttpPost]//, AsyncTimeout(60000)
        //public ActionResult Execute(string url, string primaryKey)//, string ProGuid
        //{
        //    int id = int.Parse(primaryKey);
        //    var result = GroupRuleExecute(url.Trim(), id);//, ProGuid
        //    if (result.Item1)
        //    {
        //        _inGroupStateService.InGroupDoExecute(ProjectProvider.Instance.Current.SD_ID, primaryKey);
        //        var prolog = _procLogService.GetManay(log => log.SD_ID == ProjectProvider.Instance.Current.SD_ID && log.PROC_CAT_CODE == "1" && log.PROC_CONTENT_ID == primaryKey).OrderByDescending(r => r.UPD_DATE).FirstOrDefault() ?? new PDP_PROC_LOG();
        //        _procLogService.Insert(new PDP_PROC_LOG
        //        {
        //            SD_ID = ProjectProvider.Instance.Current.SD_ID,
        //            PROC_CAT_CODE = _procLogService.InGroupCatCode(),
        //            PROC_CONTENT_ID = primaryKey,
        //            START_TIME = prolog.START_TIME,
        //            END_TIME = prolog.END_TIME,
        //            PATIENT_ID = prolog.PATIENT_ID,
        //            PROC_URL = url
        //        });
        //        return Success("入组执行成功", new { UserName = OperatorProvider.Instance.Current.RealName, UpdateDate = DateTime.Now });
        //    }
        //    else
        //    {
        //        _iErrorLogService.Insert(new PDP_ERROR_LOG
        //        {
        //            SD_ID = ProjectProvider.Instance.Current.SD_ID,
        //            PROC_TYPE = ConvertExeFlag("execute"),
        //            //PROC_URL =
        //            //DLL_NAME =
        //            //CLASS_NAME =
        //            //FUNC_NAME =
        //            ERR_CONTENT = result.Item2,
        //        });
        //        return Error(result.Item2);
        //    }
        //}

        private Tuple<bool, string> GroupRuleTest(TestParamViewModel model, string primaryKey)
        {
            try
            {
                DateTime minTime = model.startTime == DateTime.MinValue ? _ingroupService.GetMinAndMaxTime().Item1 : model.startTime;
                DateTime maxTime = model.endTime == DateTime.MinValue ? _ingroupService.GetMinAndMaxTime().Item2.AddDays(1) : Convert.ToDateTime(model.endTime.ToDateString() + " 23:59:59");
                for (DateTime dt = minTime; dt <= maxTime; dt = dt.AddDays(60))
                {
                    var starTime = dt;
                    var endTime = dt.AddDays(60) < maxTime ? dt.AddDays(60) : maxTime;
                    var tuple = ExecuteUrl(model.url.Trim(), starTime, endTime, model.patientId.IsNullOrEmpty() ? model.patientId : model.patientId.Trim());
                    if (tuple == null || tuple.Item1.Count == 0 || tuple.Item2.Count == 0 || tuple.Item3.Count == 0)
                    {
                        continue;
                    }
                    if (tuple?.Item2.Select(r => r.SD_CODE).FirstOrDefault() == ProjectProvider.Instance.Current.SD_CODE)
                    {
                        list.AddRange(_inGroupResultService.GetList(tuple.Item3, primaryKey));
                    }
                    else
                    {
                        list = new List<InGroupResultViewModel>();
                        return new Tuple<bool, string>(false, "调用的webapi与该病种不匹配");
                    }
                }
                return new Tuple<bool, string>(true, "测试成功");
            }
            catch (Exception ex)
            {
                return new Tuple<bool, string>(false, ex.ToString());
            }
        }
        //private Tuple<bool, string> Do(string url, int primaryKey, string ProGuid)
        //{
        //    try
        //    {
        //        Tuple<bool, string> result = null;
        //        DateTime minTime = _ingroupService.GetMinAndMaxTime().Item1;
        //        DateTime maxTime = _ingroupService.GetMinAndMaxTime().Item2.AddDays(1);
        //        //DateTime minTime = Convert.ToDateTime("2017-04-05");
        //        //double days = (maxTime - minTime).TotalDays - 1; //zlt
        //        //int i = 0;

        //        for (DateTime dt = minTime; dt <= maxTime; dt = dt.AddDays(60))
        //        {
        //            var starTime = dt;
        //            var endTime = dt.AddDays(60) < maxTime ? dt.AddDays(60) : maxTime;

        //            //SetProgressNum(i, endTime, maxTime, days, ProGuid);

        //            //string connectionString = @"Data Source='" + ProjectProvider.Instance.Current.CDR_DATA_SOURCE + "';Initial Catalog='" + ProjectProvider.Instance.Current.CDR_INITIAL_CATALOG + "';User ID='" + ProjectProvider.Instance.Current.CDR_USER_ID + "';Password='" + ProjectProvider.Instance.Current.CDR_PASSWORD + "'";
        //            var tuple = ExecuteUrl(url, starTime, endTime, "");
        //            if (tuple == null || tuple.Item1.Count == 0 || tuple.Item2.Count == 0 || tuple.Item3.Count == 0)
        //            {
        //                continue;
        //            }
        //            result = _patientInfoService.Insert(tuple.Item1, tuple.Item2, tuple.Item3, primaryKey);
        //            if (!result.Item1)
        //            {
        //                //finishProgress();
        //                return new Tuple<bool, string>(result.Item1, result.Item2);
        //            }

        //            //i++;
        //        }
        //        //return new Tuple<bool, string>(true, "OK");
        //        return new Tuple<bool, string>(result.Item1, result.Item2);
        //    }
        //    catch (Exception ex)
        //    {
        //        return new Tuple<bool, string>(false, ex.ToString());
        //    }
        //}

        private Tuple<bool, string> GroupRuleExecute(string url, int primaryKey)//, string ProGuid
        {
            try
            {
                Tuple<bool, string> result = null;
                //DateTime minTime = _ingroupService.GetMinAndMaxTime().Item1;
                DateTime maxTime = _ingroupService.GetMinAndMaxTime().Item2.AddDays(1);
                DateTime minTime = Convert.ToDateTime("2017-04-05");
                //double days = (maxTime - minTime).TotalDays - 1; //zlt
                //int i = 0;
                int num = 0;
                List<PatientInfoViewModel> patientInfos = new List<PatientInfoViewModel>();
                List<UN_SD_CPATS> cpats = new List<UN_SD_CPATS>();
                List<UN_SD_CPAT_DETAIL> details = new List<UN_SD_CPAT_DETAIL>();
                for (DateTime dt = minTime; dt <= maxTime; dt = dt.AddDays(60))
                {
                    var starTime = dt;
                    var endTime = dt.AddDays(60) < maxTime ? dt.AddDays(60) : maxTime;

                    //SetProgressNum(i, endTime, maxTime, days, ProGuid);

                    //string connectionString = @"Data Source='" + ProjectProvider.Instance.Current.CDR_DATA_SOURCE + "';Initial Catalog='" + ProjectProvider.Instance.Current.CDR_INITIAL_CATALOG + "';User ID='" + ProjectProvider.Instance.Current.CDR_USER_ID + "';Password='" + ProjectProvider.Instance.Current.CDR_PASSWORD + "'";
                    var tuple = ExecuteUrl(url, starTime, endTime, "");
                    //if (tuple == null || tuple.Item1.Count == 0 || tuple.Item2.Count == 0 || tuple.Item3.Count == 0)
                    //{
                    //    continue;
                    //}
                    patientInfos.AddRange(tuple?.Item1);
                    cpats.AddRange(tuple?.Item2);
                    details.AddRange(tuple?.Item3);
                    num++;

                    if (num == 5 || endTime == maxTime)
                    {
                        result = _patientInfoService.Insert(patientInfos, cpats, details, primaryKey);
                        patientInfos = new List<PatientInfoViewModel>();
                        cpats = new List<UN_SD_CPATS>();
                        details = new List<UN_SD_CPAT_DETAIL>();
                        num = 0;
                        if (!result.Item1)
                        {
                            //finishProgress();
                            return new Tuple<bool, string>(result.Item1, result.Item2);
                        }
                    }
                    //i++;
                }
                //return new Tuple<bool, string>(true, "OK");
                return new Tuple<bool, string>(result.Item1, result.Item2);
            }
            catch (Exception ex)
            {
                return new Tuple<bool, string>(false, ex.ToString());
            }
        }
        /// <summary>
        /// 导出
        /// </summary>
        /// <param name="primaryKey"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult Export(string primaryKey)//, TestParamViewModel model
        {
            try
            {
                var pageData = _inGroupResultService.GetList(null, primaryKey);//.GetResultList(primaryKey);//model.patientId,model.startTime,model.endTime,
                int filterId = int.Parse(primaryKey);
                var filtername = _ingroupService.Get(e => e.SD_FILTER_ID == filterId).SD_FILTER_NAME;
                var filename = string.Format("{0}_{1}_{2}.xlsx", ProjectProvider.Instance.Current.SD_NAME, filtername, DateTime.Now.ToString("yyyyMMddhhmmss"));
                var ok = ExportExcelHelper.DataSetToExcel<InGroupResultViewModel>(pageData, false, filename);
                if (ok.Item1)
                {
                    return Success(ok.Item2);
                }
                return Error(ok.Item2);
            }
            catch (Exception ex)
            {
                _iErrorLogService.Insert(new PDP_ERROR_LOG
                {
                    SD_ID = ProjectProvider.Instance.Current.SD_ID,
                    PROC_TYPE = ConvertExeFlag("export"),
                    //PROC_URL =
                    //DLL_NAME =
                    //CLASS_NAME =
                    //FUNC_NAME =
                    ERR_CONTENT = ex.ToString(),
                });
                return Error(ex.ToString());
            }
        }
        /// <summary>
        /// 锁定
        /// </summary>
        /// <param name="primaryKey"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult Locking(string primaryKey)
        {
            var id = int.Parse(primaryKey);
            //if (_ingroupService.Get(ex => ex.SD_ID == sd_id && ex.SD_FILTER_ID == id).VALID_FLAG == 0)
            //    return Error("该入组规则无效，不能进行锁定");
            var result = _inGroupResultService.Locking(id);
            if (result.Item1)
            {
                _inGroupStateService.InGroupLockExecute(ProjectProvider.Instance.Current.SD_ID, primaryKey);
                return Success(result.Item2);
            }
            else
            {
                return Error(result.Item2);
            }
            //return result.Item1 ? Success(result.Item2) : Error(result.Item2);
        }
        /// <summary>
        /// 清库
        /// </summary>
        /// <param name="primaryKey"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult Clear(string primaryKey)
        {
            var id = int.Parse(primaryKey);
            var result = _inGroupResultService.ClearUniteData(id);
            return result.Item1 ? Success(result.Item2, new { UserName = OperatorProvider.Instance.Current.RealName, UpdateDate = DateTime.Now }) : Error(result.Item2);
        }
        private Tuple<List<PatientInfoViewModel>, List<UN_SD_CPATS>, List<UN_SD_CPAT_DETAIL>> ExecuteUrl(string url, DateTime startTime, DateTime endTime, string patientNo)
        {
            try
            {
                string connectionString = "Data Source='" + ProjectProvider.Instance.Current.CDR_DATA_SOURCE + "';Initial Catalog='" + ProjectProvider.Instance.Current.CDR_INITIAL_CATALOG + "';User ID='" + ProjectProvider.Instance.Current.CDR_USER_ID + "';Password='" + ProjectProvider.Instance.Current.CDR_PASSWORD + "'";
                var result = HttpGet(url, string.Format("dateBegin={0}&dateEnd={1}&patientNo={2}", startTime, endTime, patientNo));//&connectionString={3}  , connectionString
                if (result == "null")
                {
                    return null;
                }
                result = result.Replace("m_Item1", "Item1");
                result = result.Replace("m_Item2", "Item2");
                result = result.Replace("m_Item3", "Item3");
                var tuple = JsonConvert.DeserializeObject<Tuple<List<PatientInfoViewModel>, List<UN_SD_CPATS>, List<UN_SD_CPAT_DETAIL>>>(result);
                return tuple;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
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
        private string HttpGet(string Url, string postDataStr)
        {
            HttpWebResponse response = null;
            Stream myResponseStream = null;
            StreamReader myStreamReader = null;
            try
            {
                GC.Collect();//垃圾回收站
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(Url + (postDataStr == "" ? "" : "?") + postDataStr);
                request.Method = "GET";
                request.ServicePoint.ConnectionLimit = 1024;//默认为2个
                request.KeepAlive = false;
                request.Timeout = 1000000;//100秒
                request.ContentType = "text/html;charset=UTF-8";
                response = (HttpWebResponse)request.GetResponse();
                myResponseStream = response.GetResponseStream();
                myStreamReader = new StreamReader(myResponseStream, Encoding.GetEncoding("utf-8"));
                string retString = myStreamReader.ReadToEnd();
                //myStreamReader.Close();
                //myResponseStream.Close();
                return retString;
            }
            catch (Exception ex)
            {
                //LogHelper.Error("post信息时出错！信息：" + ex.Message);
                //return null;
                throw ex;
            }
            finally
            {
                if (myStreamReader != null)
                {
                    myStreamReader.Close();
                    myStreamReader.Dispose();
                }
                if (myResponseStream != null)
                {
                    myResponseStream.Close();
                    myResponseStream.Dispose();
                }
                if (response != null)
                {
                    response.Close();
                    response.Dispose();
                }
            }
        }
        /// <summary>
        /// 获取入组规则信息分页加载到界面
        /// </summary>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public ActionResult GetInGroupResultList(int pageIndex, int pageSize)// GetInGroupResultList,List<InGroupResultViewModel> data
        {
            var pageData = _inGroupResultService.GetList(pageIndex, pageSize, list);
            var result = new LayPadding<InGroupResultViewModel>()
            {
                result = true,
                msg = "success",
                list = pageData.Items,
                count = pageData.TotalItems
            };
            return Content(result.ToJson());
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

        /// <summary>
        /// 获取进度条的值
        /// </summary>
        /// <param name="ProGuid"></param>
        /// <returns></returns>
        public ActionResult GetProgressNum(string ProGuid)
        {
            //return Json(Num);
            try
            {
                //循环遍历，检测是否字典里面是否有 key为 ProGuid的键值对
                var temp = false;
                foreach (var key in NumList.Keys)
                {
                    if (key == ProGuid)
                    {
                        temp = true;
                    }
                }
                if (temp)//找到就返回相应的值
                    return Json(NumList[ProGuid]);
                else
                    return Json(0);
            }
            catch
            {
                return Json(0);
            }
        }
        /// <summary>
        /// 存储 进度条的值
        /// </summary>
        /// <param name="i"></param>
        /// <param name="endTime"></param>
        /// <param name="maxTime"></param>
        /// <param name="days"></param>
        /// <param name="ProGuid"></param>
        private void SetProgressNum(int i, DateTime endTime, DateTime maxTime, double days, string ProGuid)
        {
            var temp = true;
            if (NumList.Count > 0)
            {
                foreach (var key in NumList.Keys)
                {
                    if (key == ProGuid)
                    {
                        temp = false;
                        //break;
                    }
                    else
                    {
                        NumList.Remove(key);
                    }
                }
            }
            if (temp)
                NumList.Add(ProGuid, endTime == maxTime ? 400 : Convert.ToInt32(i * 30 / days * 400));
            else
                NumList[ProGuid] = (endTime == maxTime ? 400 : Convert.ToInt32(i * 30 / days * 400));
            //Num = (endTime == maxTime ? 200 : Convert.ToInt32(i * 30 / days * 200));
        }

        //使用rabbitMQ进行执行 09-01
        public ActionResult Execute(string url, string primaryKey)//, string ProGuid
        {
            int id = int.Parse(primaryKey);
            try
            {
                //创建两个进程
                var tasks = new Task[2];
                tasks[0] = Task.Factory.StartNew(() =>
                 {
                     FilterSend(url, id);
                 });
                tasks[1] = Task.Factory.StartNew(() =>
                {
                    FilterReceive(id);
                });
                Task.WaitAll(tasks);
                //什么时候执行成功？？？
                _inGroupStateService.InGroupDoExecute(ProjectProvider.Instance.Current.SD_ID, primaryKey);
                var prolog = _procLogService.GetManay(log => log.SD_ID == ProjectProvider.Instance.Current.SD_ID && log.PROC_CAT_CODE == "1" && log.PROC_CONTENT_ID == primaryKey).OrderByDescending(r => r.UPD_DATE).FirstOrDefault() ?? new PDP_PROC_LOG();
                _procLogService.Insert(new PDP_PROC_LOG
                {
                    SD_ID = ProjectProvider.Instance.Current.SD_ID,
                    PROC_CAT_CODE = _procLogService.InGroupCatCode(),
                    PROC_CONTENT_ID = primaryKey,
                    START_TIME = prolog.START_TIME,
                    END_TIME = prolog.END_TIME,
                    PATIENT_ID = prolog.PATIENT_ID,
                    PROC_URL = url
                });
                return Success("入组执行成功", new { UserName = OperatorProvider.Instance.Current.RealName, UpdateDate = DateTime.Now });
            }
            catch (Exception ex)
            {
                _iErrorLogService.Insert(new PDP_ERROR_LOG
                {
                    SD_ID = ProjectProvider.Instance.Current.SD_ID,
                    PROC_TYPE = ConvertExeFlag("execute"),
                    //PROC_URL =
                    //DLL_NAME =
                    //CLASS_NAME =
                    //FUNC_NAME =
                    ERR_CONTENT = ex.ToString(),
                });
                return Error(ex.Message);
            }
        }
        //入组执行发送
        private void FilterSend(string url, int primaryKey)
        {
            //DateTime minTime = _ingroupService.GetMinAndMaxTime().Item1;
            DateTime maxTime = _ingroupService.GetMinAndMaxTime().Item2.AddDays(1);
            DateTime minTime = Convert.ToDateTime("2017-04-05");

            for (DateTime dt = minTime; dt <= maxTime; dt = dt.AddDays(60))
            {
                var starTime = dt;
                var endTime = dt.AddDays(60) < maxTime ? dt.AddDays(60) : maxTime;
                string message = string.Format("{0}?dateBegin={1}&dateEnd={2}&patientNo={3}", url, starTime, endTime);
                //var body = Encoding.UTF8.GetBytes(message);

                MyMessage msg = new MyMessage();
                msg.MessageID = "1";
                msg.MessageRouter = "Filter_Condition";
                msg.MessageBody = message;
                MQHelper.Publish(msg);
            }
        }
        private void FilterReceive(int primaryKey)
        {
            OrderProcessMessage order = new OrderProcessMessage();
            MyMessage msg = new MyMessage();
            msg.MessageID = "1";
            msg.MessageRouter = "Filter_Result";

            MQHelper.Subscribe(msg, order);
        }
        /// <summary>
        /// 入组执行发送
        /// </summary>
        /// <param name="url"></param>
        /// <param name="primaryKey"></param>
        //private void FilterSend(string url, int primaryKey)
        //{
        //    var factory = new ConnectionFactory() { HostName = "localhost" };//"192.168.2.53"
        //    using (var connection = factory.CreateConnection())
        //    {
        //        using (var channel = connection.CreateModel())
        //        {
        //            channel.QueueDeclare(queue: "Filter",
        //                         durable: true,
        //                         exclusive: false,
        //                         autoDelete: false,
        //                         arguments: null);
        //            //DateTime minTime = _ingroupService.GetMinAndMaxTime().Item1;
        //            DateTime maxTime = _ingroupService.GetMinAndMaxTime().Item2.AddDays(1);
        //            DateTime minTime = Convert.ToDateTime("2017-04-05");

        //            for (DateTime dt = minTime; dt <= maxTime; dt = dt.AddDays(60))
        //            {
        //                var starTime = dt;
        //                var endTime = dt.AddDays(60) < maxTime ? dt.AddDays(60) : maxTime;
        //                string message = string.Format("{0}?dateBegin={1}&dateEnd={2}&patientNo={3}", url, starTime, endTime);
        //                var body = Encoding.UTF8.GetBytes(message);
        //                var properties = channel.CreateBasicProperties();
        //                properties.Persistent = true;

        //                channel.BasicPublish(exchange: "",
        //                             routingKey: "Filter_Condition",
        //                             basicProperties: properties,
        //                             body: body);
        //                Console.WriteLine(" [x] Sent {0}", message);
        //            }
        //        }
        //        Console.WriteLine(" Press [enter] to exit.");
        //        Console.ReadLine();
        //    }
        //}
        /// <summary>
        /// 入组执行接收
        /// </summary>
        //private void FilterReceive(int primaryKey)
        //{
        //    var factory = new ConnectionFactory() { HostName = "localhost" };//"192.168.2.53"
        //    using (var connection = factory.CreateConnection())
        //    {
        //        using (IModel channel = connection.CreateModel())
        //        {
        //            List<PatientInfoViewModel> patientInfos = new List<PatientInfoViewModel>();
        //            List<UN_SD_CPATS> cpats = new List<UN_SD_CPATS>();
        //            List<UN_SD_CPAT_DETAIL> details = new List<UN_SD_CPAT_DETAIL>();

        //            channel.QueueDeclare(queue: "Filter_Result",
        //                                 durable: true,
        //                                 exclusive: false,
        //                                 autoDelete: false,
        //                                 arguments: null);

        //            channel.BasicQos(prefetchSize: 0, prefetchCount: 1, global: false);

        //            var consumer = new EventingBasicConsumer(channel);
        //            consumer.Received += (model, ea) =>
        //            {
        //                var body = ea.Body;
        //                var message = Encoding.UTF8.GetString(body);
        //                //处理接收的信息
        //                try
        //                {
        //                    if (message != "null")
        //                    {
        //                        message = message.Replace("m_Item1", "Item1");
        //                        message = message.Replace("m_Item2", "Item2");
        //                        message = message.Replace("m_Item3", "Item3");
        //                        var tuple = JsonConvert.DeserializeObject<Tuple<List<PatientInfoViewModel>, List<UN_SD_CPATS>, List<UN_SD_CPAT_DETAIL>>>(message);
        //                        var result = _patientInfoService.Insert(tuple.Item1, tuple.Item2, tuple.Item3, primaryKey);
        //                    }
        //                }
        //                catch (Exception ex)
        //                {
        //                    throw ex;
        //                }
        //                //Thread t = new Thread(new ParameterizedThreadStart(Listener));
        //                //t.Start(new par { channel = channel, ea = ea });
        //            };
        //            channel.BasicConsume(queue: "Filter_Result",
        //                                 noAck: false,
        //                                 consumer: consumer);
        //        }
        //    }
        //}
    }
}