using Elight.Entity;
using Elight.Infrastructure;
using Elight.Web.Controllers;
using Elight.Web.Filters;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using TestingCenterSystem.Models.PDP;
using TestingCenterSystem.Service.DataItem;
using TestingCenterSystem.Service.DataItem.Interface;
using TestingCenterSystem.Service.InGroup;
using TestingCenterSystem.Service.InGroup.Interface;
using TestingCenterSystem.Service.KPI;
using TestingCenterSystem.Service.KPI.Interface;
using TestingCenterSystem.Service.Project;
using TestingCenterSystem.Service.Project.Interface;
using TestingCenterSystem.Service.Sys;
using TestingCenterSystem.Service.Sys.Interface;
using TestingCenterSystem.ViewModels.SD;

namespace Elight.Web.Areas.TestCenterSystem.Controllers.Analysis
{
    [LoginChecked]
    public class ProgressAndAnalysisController : BaseController
    {
        private static readonly IProjectService _projectService = new ProjectService();
        private static readonly IInGroupService _ingroupService = new InGroupService();
        private static readonly IDataItemService _dataitemService = new DataItemService();
        private static readonly IKPIService _kpiService = new KPIService();
        private static readonly IUserService _userService = new UserService();
        private static readonly ISDStateService _sdStateService = new SDStateService();

        public ActionResult Index()
        {
            return View();
        }

        //项目进度分析统计
        public JsonResult GetProjectProgess(string projectId, string sdId)
        {
            try
            {
                var sdList = GetIdList(projectId, sdId);
                var types = new List<string>() { "1", "2", "3" };//1:入组；2:数据项；3:kpi
                var result = new Dictionary<string, object>();

                var stateList = new List<PDP_PROC_STAT>();
                foreach (var sd in sdList)
                {
                    stateList.AddRange(_sdService.GetProStat(sd, false));
                }
                var dataList = (from s in (from r in stateList select new { PROC_CAT_CODE = r.PROC_CAT_CODE, PROC_STAT_CODE = GetExcuteName(r.PROC_STAT_CODE) })//stateList
                                    //where s.PROC_STAT_CODE != "3"
                                group s by new { s.PROC_CAT_CODE, s.PROC_STAT_CODE } into g
                                select new { PROC_CAT_CODE = g.Key.PROC_CAT_CODE, PROC_STAT_CODE = g.Key.PROC_STAT_CODE, COUNT = g.Count() }).ToList();

                var states = dataList.Select(s => s.PROC_STAT_CODE).Distinct().ToList();
                foreach (var type in types)
                {
                    var stateDate = dataList.Where(r => r.PROC_CAT_CODE == type).Select(s =>
                    {
                        return new Dictionary<string, object>
                        {
                            {"name", s.PROC_STAT_CODE},
                            {"value", s.COUNT}
                        };
                    }).ToList();
                    var data = new Dictionary<string, object>()
                    {
                        {"states", states},
                        {"data", stateDate}
                    };
                    result.Add(type, data);
                }
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        public JsonResult GetProjectDataItemProgess(string projectId, string sdId)
        {
            try
            {
                var sdList = GetIdList(projectId, sdId);
                //var types = new List<string>() { "1", "2", "3" };//1:入组；2:数据项；3:kpi
                var type = "2";//2:数据项
                var states = new List<string>() { "未执行", "已执行", "已锁定", "未指定" };
                var result = new Dictionary<string, object>();

                var stateList = new List<PDP_PROC_STAT>();
                foreach (var sd in sdList)
                {
                    stateList.AddRange(_sdService.GetProStat(sd, true));//只统计结果数据项
                }
                var dataList = (from s in (from r in stateList select new { PROC_CAT_CODE = r.PROC_CAT_CODE, PROC_STAT_CODE = GetExcuteName(r.PROC_STAT_CODE) })//stateList
                                where s.PROC_CAT_CODE == type
                                group s by new { s.PROC_CAT_CODE, s.PROC_STAT_CODE } into g
                                select new { PROC_CAT_CODE = g.Key.PROC_CAT_CODE, PROC_STAT_CODE = g.Key.PROC_STAT_CODE, COUNT = g.Count() }).ToList();

                var stateDate = new List<Dictionary<string, object>>();
                states.ForEach(state =>
                {
                    stateDate.Add(new Dictionary<string, object>
                    {
                            {"name", state},
                            {"value",dataList.Where(r=>r.PROC_STAT_CODE==state).Select(s=>s.COUNT).FirstOrDefault()}
                    });
                });

                #region V1.3
                //var states = dataList.Select(s => s.PROC_STAT_CODE).Distinct().ToList();
                ////foreach (var type in types)
                ////{
                //var stateDate = dataList.Where(r => r.PROC_CAT_CODE == type).Select(s =>
                //{
                //    return new Dictionary<string, object>
                //    {
                //            {"name", s.PROC_STAT_CODE},
                //            {"value", s.COUNT}
                //    };
                //}).ToList(); 
                #endregion

                var data = new Dictionary<string, object>()
                    {
                        {"states", states},
                        {"data", stateDate}
                    };
                result.Add(type, data);
                //}
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        //获取用户总贡献量
        public JsonResult GetUsersContributions(string projectId, string sdId)
        {
            #region 调用userService
            try
            {
                var sdList = GetIdList(projectId, sdId);
                var returndData = GetUsersAndContribution(sdList);
                var users = returndData.Item1 ?? new Dictionary<string, string>();
                var dataDic = returndData.Item2 ?? new Dictionary<string, int>();
                var userList = _userService.GetManay(r => 1 == 1).ToList();
                var userDate = dataDic.Select(s => new Dictionary<string, object>
                        {
                            {"name", userList.Where(r=>r.Id==s.Key).Select(r=>r.RealName).FirstOrDefault()},//GetMemberNameByIDs(s.Key).FirstOrDefault()},
                            {"value", s.Value}
                        }).ToList();

                var data = new Dictionary<string, object>()
                    {
                        {"users", users.Values.OrderBy(r=>r).ToList()},
                        {"data", userDate}
                    };
                return Json(data, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                throw ex;
            }
            #endregion

            #region zlt
            //try
            //{
            //    var sdList = GetIdList(projectId, sdId);

            //    var list = new List<string>();
            //    list.AddRange(_ingroupService.GetManay(r => sdList.Contains(r.SD_ID)).Select(s => s.CREATE_USER_ID).ToList());
            //    list.AddRange(_dataitemService.GetManay(r => sdList.Contains((int)r.SD_ID)).Select(s => s.CREATE_USER_ID).ToList());
            //    list.AddRange(_kpiService.GetManay(r => sdList.Contains((int)r.SD_ID)).Select(s => s.CREATE_USER_ID).ToList());
            //    var dataList = (from s in list
            //                    group s by s into g
            //                    select new { RealName = GetMemberNameByIDs(g.Key).FirstOrDefault() ?? "", COUNT = g.Count() }).ToList();
            //    var users = dataList.Select(s => s.RealName).ToList();

            //    var userDate = dataList.Select(s => new Dictionary<string, object>
            //            {
            //                {"name", s.RealName},
            //                {"value", s.COUNT}
            //            }).ToList();

            //    var data = new Dictionary<string, object>()
            //        {
            //            {"users", users},
            //            {"data", userDate}
            //        };
            //    return Json(data, JsonRequestBehavior.AllowGet);
            //}
            //catch (Exception ex)
            //{
            //    throw ex;
            //} 
            #endregion
        }

        //获取用户总用时
        public JsonResult GetUsersTime(string projectId, string sdId)
        {
            try
            {
                var users = new List<string>();
                var datas = new List<string>();

                var sdList = GetIdList(projectId, sdId);
                var dicIdResult = GetTimes(sdList);
                var userService = _userService.GetManay(r => dicIdResult.Keys.Contains(r.Id)).ToList();
                dicIdResult.OrderByDescending(r => double.Parse(r.Value ?? "0")).ToList().ForEach(dic =>
                    {
                        users.Add(userService.Where(r => r.Id == dic.Key).Select(s => s.RealName).FirstOrDefault());
                        datas.Add(dic.Value);
                    });

                var result = new Dictionary<string, object>()
                    {
                        {"users", users},
                        {"datas", datas}
                    };

                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        //获取用户效率
        public JsonResult GetUsersEfficiency(string projectId, string sdId)
        {
            try
            {
                //效率=总数量/总时间
                var users = new List<string>();
                var datas = new List<string>();
                var userAndValue = new Dictionary<string, string>();
                var sdList = GetIdList(projectId, sdId);
                var contribution = GetUsersAndContribution(sdList).Item2 ?? new Dictionary<string, int>();
                var times = GetTimes(sdList);
                var userList = _userService.GetManay(r => 1 == 1).ToList();
                contribution.ToList().ForEach(con =>
                {
                    //users.Add(GetMemberNameByIDs(con.Key.ToString()).FirstOrDefault());
                    //users.Add(userList.Where(r => r.Id == con.Key).Select(s => s.RealName).FirstOrDefault());
                    var Denominator = times.Where(t => t.Key == con.Key).Select(s => s.Value).FirstOrDefault();
                    var efficiency = Denominator == null ? null : (con.Value / double.Parse(Denominator)).ToString("f2");//double.Parse($"{(con.Value / Denominator):F}");//Denominator == 0.00
                    //datas.Add(efficiency);
                    userAndValue.Add(userList.Where(r => r.Id == con.Key).Select(s => s.RealName).FirstOrDefault(), efficiency);
                });
                users = userAndValue.OrderByDescending(r => double.Parse(r.Value ?? "0")).Select(s => s.Key).ToList();
                datas = userAndValue.OrderByDescending(r => double.Parse(r.Value ?? "0")).Select(s => s.Value).ToList();

                var result = new Dictionary<string, object>()
                    {
                        {"users", users},
                        {"datas", datas}
                    };

                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private Tuple<Dictionary<string, string>, Dictionary<string, int>> GetUsersAndContribution(List<int> sdList)
        {
            try
            {
                var users = new Dictionary<string, string>();
                var dataDic = new Dictionary<string, int>();
                var infos = GetInfos(sdList);
                var userIds = infos.Select(s => s.CREATE_USER_ID).Distinct().ToList();
                var userList = _userService.GetManay(r => 1 == 1).ToList();
                for (int i = 0; i < userIds.Count(); i++)
                {
                    var contribution = infos.Sum(info =>
                    {
                        if (info.CREATE_USER_ID == userIds[i])
                            return _userService.GetUserContribute(info.SD_ID, info.CAT_CODE, info.CREATE_USER_ID);
                        else
                            return 0;
                    });
                    if (!users.Keys.Contains(userIds[i]))
                        users.Add(userIds[i], userList.Where(r => r.Id == userIds[i]).Select(r => r.RealName).FirstOrDefault());
                    if (dataDic.Keys.Contains(userIds[i]))
                        dataDic[userIds[i]] = contribution;
                    else
                        dataDic.Add(userIds[i], contribution);
                }
                return new Tuple<Dictionary<string, string>, Dictionary<string, int>>(users, dataDic);

                #region zlt
                //try
                //{
                //    var list = new List<string>();
                //    var users = new Dictionary<string, string>();
                //    var dataDic = new Dictionary<string, int>();
                //    list.AddRange(_ingroupService.GetManay(r => sdList.Contains(r.SD_ID)).Select(s => s.CREATE_USER_ID).ToList());
                //    list.AddRange(_dataitemService.GetManay(r => sdList.Contains((int)r.SD_ID)).Select(s => s.CREATE_USER_ID).ToList());
                //    list.AddRange(_kpiService.GetManay(r => sdList.Contains((int)r.SD_ID)).Select(s => s.CREATE_USER_ID).ToList());
                //    var dataList = (from s in list
                //                    group s by s into g
                //                    select new { ID = g.Key, RealName = GetMemberNameByIDs(g.Key).FirstOrDefault() ?? "", COUNT = g.Count() }).ToList();
                //    //var users = dataList.Select(s => s.RealName).ToList();
                //    dataList.ForEach(r =>
                //    {
                //        users.Add(r.ID, r.RealName);
                //        dataDic.Add(r.ID, r.COUNT);
                //    });
                //    return new Tuple<Dictionary<string, string>, Dictionary<string, int>>(users, dataDic);

                #endregion
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private Dictionary<string, string> GetTimes(List<int> sdList)
        {
            try
            {
                #region zlt
                //var dicIdResult = new Dictionary<string, int>();
                //var leftJoinList = GetUsersAndStates(sdList);
                //var userIds = leftJoinList.Select(s => s.CREATE_USER_ID).Distinct().ToList();
                //for (int i = 0; i < userIds.Count(); i++)
                //{
                //    //获取状态
                //    var state = true;
                //    var days = 0;
                //    DateTime endTime;
                //    var lists = leftJoinList.Where(r => r.CREATE_USER_ID == userIds[i]).ToList();
                //    DateTime stateTime = lists.Select(r => r.CREATE_DATE).Min();
                //    foreach (var list in lists.Where(r => r.ISRSULT == 1))
                //    {
                //        if (list.PROC_STAT_CODE != "4")
                //        {
                //            state = false;
                //            break;
                //        }
                //    }
                //    if (state)
                //    {
                //        endTime = lists.Select(r => r.UPD_DATE ?? DateTime.Now).Max();
                //    }
                //    else
                //        endTime = DateTime.Now;
                //    days = Convert.ToDateTime(endTime.ToShortDateString()).Subtract(Convert.ToDateTime(stateTime.ToShortDateString())).Days;
                //    dicIdResult.Add(userIds[i], days);
                //}
                //return dicIdResult; 
                #endregion

                #region 调用service
                var dicIdResult = new Dictionary<string, string>();
                //var leftJoinList = GetUsersAndStates(sdList);

                var infos = GetInfos(sdList);
                var userIds = infos.Select(s => s.CREATE_USER_ID).Distinct().ToList();
                for (int i = 0; i < userIds.Count(); i++)
                {
                    var days = string.Empty;// 0.00;zlt
                    var startTime = DateTime.MinValue;
                    var endTime = DateTime.MinValue;
                    infos.Where(r => r.CREATE_USER_ID == userIds[i]).ToList().ForEach(info =>
                       {
                           var time = _userService.GetUserTime(info.SD_ID, info.CAT_CODE, info.CREATE_USER_ID);
                           if (startTime == DateTime.MinValue)
                               startTime = time.Item1;
                           if (endTime == DateTime.MinValue)
                               endTime = time.Item2;
                           var comparTime = CompareTime(time, startTime, endTime);
                           if (comparTime.Item1 != DateTime.MinValue)
                               startTime = comparTime.Item1;
                           if (comparTime.Item2 != DateTime.MinValue)
                               endTime = comparTime.Item2;
                       });
                    //days = Convert.ToDateTime(endTime.ToShortDateString()).Subtract(Convert.ToDateTime(startTime.ToShortDateString())).Days;
                    //days = endTime < startTime ? 0 : double.Parse($"{(endTime - startTime).TotalDays:F}");
                    days = endTime < startTime ? null : (endTime - startTime).TotalDays.ToString("f2");
                    dicIdResult.Add(userIds[i], days);
                }
                return dicIdResult;
                #endregion
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private List<int> GetIdList(string projectId, string sdId)
        {
            var sdList = new List<int>();
            if (string.IsNullOrWhiteSpace(projectId))
                sdList = new List<int>();
            else if (string.IsNullOrWhiteSpace(sdId))
            {
                var projId = int.Parse(projectId);
                sdList = _sdService.GetManay(r => r.TC_PROJ_ID == projId && r.SD_TYPE_CODE == 0).Select(s => s.SD_ID).ToList();
            }
            else
                sdList.Add(int.Parse(sdId));
            return sdList;
        }

        private List<USERSANDSTATES_VIEWMODEL> GetInfos(List<int> sdList)
        {
            try
            {
                var result = _ingroupService.GetManay(r => sdList.Contains(r.SD_ID)).GroupBy(r => new { r.SD_ID, r.CREATE_USER_ID }).Select(s => new USERSANDSTATES_VIEWMODEL
                {
                    SD_ID = s.Key.SD_ID,
                    CAT_CODE = "1",
                    CREATE_USER_ID = s.Key.CREATE_USER_ID
                }).Concat(_dataitemService.GetManay(r => sdList.Contains((int)r.SD_ID)).GroupBy(r => new { r.SD_ID, r.CREATE_USER_ID }).Select(s => new USERSANDSTATES_VIEWMODEL
                {
                    SD_ID = (int)s.Key.SD_ID,
                    CAT_CODE = "2",
                    CREATE_USER_ID = s.Key.CREATE_USER_ID
                })).Concat(_kpiService.GetManay(r => sdList.Contains((int)r.SD_ID)).GroupBy(r => new { r.SD_ID, r.CREATE_USER_ID }).Select(s => new USERSANDSTATES_VIEWMODEL
                {
                    SD_ID = (int)s.Key.SD_ID,
                    CAT_CODE = "3",
                    CREATE_USER_ID = s.Key.CREATE_USER_ID
                })).ToList();

                //var result = (from ingroup in _ingroupService.GetManay(r => sdList.Contains(r.SD_ID))
                //         select string.Format("{0};{1};{2}", ingroup.SD_ID, "1", ingroup.CREATE_USER_ID)
                //         ).Concat(from dataItem in _dataitemService.GetManay(r => sdList.Contains((int)r.SD_ID))
                //                  select string.Format("{0};{1};{2}", dataItem.SD_ID, "2", dataItem.CREATE_USER_ID))
                //                  .Concat(from kpi in _kpiService.GetManay(r => sdList.Contains((int)r.SD_ID))
                //                          select string.Format("{0};{1};{2}", kpi.SD_ID, "3", kpi.CREATE_USER_ID)).Distinct().Select(info=>  new USERSANDSTATES_VIEWMODEL()
                //                              {
                //                                  SD_ID = int.Parse(info.Split(';')[0]),
                //                                  CAT_CODE = info.Split(';')[1],
                //                                  CREATE_USER_ID = info.Split(';')[2]
                //                              }
                //                          ).ToList();

                return result;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        //private List<USERSANDSTATES_VIEWMODEL> GetUsersAndStates(List<int> sdList)
        //{
        //    try
        //    {
        //        var inGroupList = _ingroupService.GetManay(r => sdList.Contains(r.SD_ID));
        //        var dataItemList = _dataitemService.GetManay(r => sdList.Contains((int)r.SD_ID));
        //        var kpiList = _kpiService.GetManay(r => sdList.Contains((int)r.SD_ID));
        //        var sdStateList = _sdStateService.GetManay(r => sdList.Contains((int)r.SD_ID));

        //        #region zlt
        //        var leftJoinList = (from ingroup in inGroupList
        //                            join state in sdStateList.Where(r => r.PROC_CAT_CODE == "1")
        //                            on ingroup.SD_FILTER_ID.ToString() equals state.PROC_CONTENT_ID into inGroupState
        //                            from state in inGroupState.DefaultIfEmpty()
        //                            select new USERSANDSTATES_VIEWMODEL()
        //                            {
        //                                INFO_ID = ingroup.SD_FILTER_ID,
        //                                SD_ID = ingroup.SD_ID,
        //                                CAT_CODE = "1",
        //                                CREATE_DATE = ingroup.CREATE_DATE,
        //                                CREATE_USER_ID = ingroup.CREATE_USER_ID,
        //                                PROC_STAT_CODE = state != null ? state.PROC_STAT_CODE : null,
        //                                UPD_DATE = state != null ? state.UPD_DATE : null,
        //                                ISRSULT = 1

        //                            }).Concat(from dataItem in dataItemList
        //                                      join state in sdStateList.Where(r => r.PROC_CAT_CODE == "2")
        //                                      on dataItem.SD_ITEM_ID.ToString() equals state.PROC_CONTENT_ID into inGroupState
        //                                      from state in inGroupState.DefaultIfEmpty()
        //                                      select new USERSANDSTATES_VIEWMODEL()
        //                                      {
        //                                          INFO_ID = dataItem.SD_ITEM_ID,
        //                                          SD_ID = (int)dataItem.SD_ID,
        //                                          CAT_CODE = "2",
        //                                          CREATE_DATE = dataItem.CREATE_DATE,
        //                                          CREATE_USER_ID = dataItem.CREATE_USER_ID,
        //                                          PROC_STAT_CODE = state != null ? state.PROC_STAT_CODE : null,
        //                                          UPD_DATE = state != null ? state.UPD_DATE : null,
        //                                          ISRSULT = dataItem.IS_RESULT
        //                                      }).Concat(from kpi in kpiList
        //                                                join state in sdStateList.Where(r => r.PROC_CAT_CODE == "3")
        //                                                on kpi.SD_EKPI_ID.ToString() equals state.PROC_CONTENT_ID into inGroupState
        //                                                from state in inGroupState.DefaultIfEmpty()
        //                                                select new USERSANDSTATES_VIEWMODEL()
        //                                                {
        //                                                    INFO_ID = kpi.SD_EKPI_ID,
        //                                                    SD_ID = (int)kpi.SD_ID,
        //                                                    CAT_CODE = "3",
        //                                                    CREATE_DATE = kpi.CREATE_DATE,
        //                                                    CREATE_USER_ID = kpi.CREATE_USER_ID,
        //                                                    PROC_STAT_CODE = state != null ? state.PROC_STAT_CODE : null,
        //                                                    UPD_DATE = state != null ? state.UPD_DATE : null,
        //                                                    ISRSULT = 1
        //                                                }).ToList();
        //        #endregion
        //        return leftJoinList;
        //    }
        //    catch (Exception ex)
        //    {
        //        throw ex;
        //    }
        //}

        private Tuple<DateTime, DateTime> CompareTime(Tuple<DateTime, DateTime> tuple, DateTime start, DateTime end)
        {
            var newStart = DateTime.MinValue;
            var newEnd = DateTime.MinValue;
            if (tuple.Item1 < start)
                newStart = tuple.Item1;
            if (tuple.Item2 > end)
                newEnd = tuple.Item2;
            return new Tuple<DateTime, DateTime>(newStart, newEnd);
        }


        //private List<string> GetMemberNameByIDs(string userIds)
        //{
        //    if (string.IsNullOrWhiteSpace(userIds))
        //        return null;
        //    var userList = userIds.Split(';');
        //    var users = _userService.GetManay(r => userList.Contains(r.Id) || userList.Contains(r.RealName)).Select(s => s.RealName).ToList();
        //    return users;
        //} 

        private string GetExcuteName(string ExcuteCode)
        {
            switch (ExcuteCode)
            {
                case "1":
                    return "未执行";
                case "2":
                    return "已执行";
                case "3":
                    return "未执行";
                case "4":
                    return "已锁定";
                default:
                    return "未指定";
            }
        }

    }
}