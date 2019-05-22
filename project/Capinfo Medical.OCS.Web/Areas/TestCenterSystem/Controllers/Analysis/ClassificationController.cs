using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Elight.Web.Filters;
using TestingCenterSystem.Service.comm.Interface;
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

namespace Elight.Web.Areas.TestCenterSystem.Controllers.Analysis
{
    [LoginChecked]
    public class ClassificationController : Controller
    {
        private readonly IUserService _userService = new UserService();
        private readonly IProjectService _projectService = new ProjectService();
        private readonly ISDService _sdService = new SDService();
        public JsonResult UserContribute(string projectId, string sdId)
        {
            List<int> proIdList = new List<int>();
            if (string.IsNullOrWhiteSpace(projectId))
                proIdList = _projectService.GetSearchList(pro => true).Select(pro => pro.TC_PROJ_ID).ToList();
            else
                proIdList.Add(int.Parse(projectId));
            var sdList = _sdService.GetSearchList(sd => proIdList.Contains(sd.TC_PROJ_ID) && sd.SD_TYPE_CODE == 0);
            if (string.IsNullOrWhiteSpace(sdId))
            {
                var sdIdList = sdList.Select(i => i.SD_ID).ToList();
                return Json(GetContributionInfo(sdIdList), JsonRequestBehavior.AllowGet); //return Json(GetContribute(sdIdList), JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(GetContributionInfo(new List<int>() { int.Parse(sdId) }), JsonRequestBehavior.AllowGet); //return Json(GetContribute(new List<int>() { int.Parse(sdId) }), JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult UserTimeByInGroup(string projectId, string sdId)
        {
            return UserTime(projectId, sdId, "1");
        }
        public JsonResult UserTimeByDataItem(string projectId, string sdId)
        {
            return UserTime(projectId, sdId, "2");
        }
        public JsonResult UserTimeByKpi(string projectId, string sdId)
        {
            return UserTime(projectId, sdId, "3");
        }
        public JsonResult UserEfficiencyByInGroup(string projectId, string sdId)
        {
            return UserEfficiency(projectId, sdId, "1");
        }
        public JsonResult UserEfficiencyByDataItem(string projectId, string sdId)
        {
            return UserEfficiency(projectId, sdId, "2");
        }
        public JsonResult UserEfficiencyByKpi(string projectId, string sdId)
        {
            return UserEfficiency(projectId, sdId, "3");
        }
        private JsonResult UserEfficiency(string projectId, string sdId, string type)
        {
            List<int> proIdList = new List<int>();
            if (string.IsNullOrWhiteSpace(projectId))
                proIdList = _projectService.GetSearchList(pro => true).Select(pro => pro.TC_PROJ_ID).ToList();
            else
                proIdList.Add(int.Parse(projectId));
            var sdList = _sdService.GetSearchList(sd => proIdList.Contains(sd.TC_PROJ_ID) && sd.SD_TYPE_CODE == 0);
            if (string.IsNullOrWhiteSpace(sdId))
            {
                var sdIdList = sdList.Select(i => i.SD_ID).ToList();
                return Json(GetEfficiency(sdIdList, type), JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(GetEfficiency(new List<int>() { int.Parse(sdId) }, type), JsonRequestBehavior.AllowGet);
            }
        }
        private JsonResult UserTime(string projectId, string sdId, string type)
        {
            List<int> proIdList = new List<int>();
            if (string.IsNullOrWhiteSpace(projectId))
                proIdList = _projectService.GetSearchList(pro => true).Select(pro => pro.TC_PROJ_ID).ToList();
            else
                proIdList.Add(int.Parse(projectId));
            var sdList = _sdService.GetSearchList(sd => proIdList.Contains(sd.TC_PROJ_ID) && sd.SD_TYPE_CODE == 0);
            if (string.IsNullOrWhiteSpace(sdId))
            {
                var sdIdList = sdList.Select(i => i.SD_ID).ToList();
                return Json(GetTime(sdIdList, type), JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(GetTime(new List<int>() { int.Parse(sdId) }, type), JsonRequestBehavior.AllowGet);
            }
        }
        //用时
        private object GetTime(List<int> sdIdList, string type)
        {
            var userList = new List<Tuple<string, string>>();
            foreach (var uu in sdIdList.Select(sdId => _sdService.GetSDMember(sdId).ToList())
                .SelectMany(u => u.Where(uu => !userList.Contains(uu))))
            {
                if (uu == null) continue;
                userList.Add(uu);
            }
            var users = new List<string>();
            var dic = GetTimeDic(userList, type, sdIdList, ref users);
            var days = users.Select(user => dic[user]).ToList();
            var data = new Dictionary<string, object>()
            {
                {"users", users},
                {"days", days}
            };
            return data;
        }
        //效率
        private object GetEfficiency(List<int> sdIdList, string type)
        {
            var userList = new List<Tuple<string, string>>();
            foreach (var uu in sdIdList.Select(sdId => _sdService.GetSDMember(sdId).ToList())
                .SelectMany(u => u.Where(uu => !userList.Contains(uu))))
            {
                if (uu == null) continue;
                userList.Add(uu);
            }
            var efficiencyList = new List<double>();
            var users = new List<string>();
            var contributeDic = GetContributDic(userList, type, sdIdList, ref users);
            var timeDic = GetTimeDic(userList, type, sdIdList, ref users);
            foreach (var user in userList.OrderBy(r => r.Item2))//zlt
            {
                var userName = user.Item2;
                if (!contributeDic.ContainsKey(userName)) continue;
                var efficiency = double.Parse($"{((!timeDic.ContainsKey(userName) || (timeDic[userName]) == 0) ? 0 : contributeDic[userName] / timeDic[userName]):F}");
                efficiencyList.Add(efficiency);
            }
            var data = new Dictionary<string, object>()
            {
                {"users", users},
                {"efficiency", efficiencyList}
            };
            return data;
        }

        //贡献量
        private object GetContribute(List<int> sdIdList)
        {
            var typeList = new List<string>() { "1", "2", "3" };
            var result = new Dictionary<string, object>();
            var users = new List<string>();
            foreach (var type in typeList)
            {
                var userList = new List<Tuple<string, string>>();
                foreach (var uu in sdIdList.Select(sdId => _sdService.GetSDMember(sdId)
                .ToList()).SelectMany(u => u.Where(uu => !userList.Contains(uu))))
                {
                    if (uu == null) continue;
                    userList.Add(uu);
                }
                var userDic = GetContributDic(userList, type, sdIdList, ref users);
                var showData = userDic.Select(dic => new Dictionary<string, object>()
                {
                    {"name", dic.Key}, {"value", dic.Value}
                }).ToList();
                var data = new Dictionary<string, object>()
                {
                    {"users", users.ToList()},
                    {"data", showData}
                };
                result.Add(type, data);
            }
            return result;
        }

        private Dictionary<string, double> GetContributDic(List<Tuple<string, string>> userList, string type, List<int> sdIdList, ref List<string> users)
        {
            var userDic = new Dictionary<string, double>();
            var list = users;
            foreach (var user in userList)
            {
                var userName = user.Item2;
                //var count = sdIdList.Sum(sdId => _userService.GetUserContribute(sdId, type, user.Item1));  //V1.3
                var count = sdIdList.Sum(sdId => _sdService.GetDateItemCount(sdId.ToString(), user.Item1));
                //if (count == 0) continue;
                if (!list.Contains(userName)) list.Add(userName);
                if (!userDic.ContainsKey(userName))
                    userDic.Add(userName, count);
                else
                    userDic[userName] += count;
            }
            users = list.OrderBy(i => i).ToList();
            var dic = userDic.OrderBy(i => i.Key).ToDictionary(i => i.Key, i => i.Value);
            return dic;
        }

        private Dictionary<string, double> GetTimeDic(List<Tuple<string, string>> userList, string type, List<int> sdIdList, ref List<string> users)
        {
            var userDic = new Dictionary<string, double>();
            var timeDic = new Dictionary<string, Tuple<DateTime, DateTime>>();
            foreach (var user in userList)
            {
                var userName = user.Item2;
                var startTime = DateTime.MinValue;
                var endTime = DateTime.MinValue;
                foreach (var time in sdIdList.Select(sdId => _userService.GetUserTime(sdId, type, user.Item1)))
                {
                    if (startTime == DateTime.MinValue || (startTime > time.Item1 && time.Item1 != DateTime.MinValue))
                        startTime = time.Item1;
                    if (endTime < time.Item2)
                        endTime = time.Item2;
                }
                var day = endTime < startTime ? 0 : double.Parse($"{(endTime - startTime).TotalDays:F}");//zlt
                if (day == 0 && endTime == DateTime.MinValue && startTime == DateTime.MinValue) continue;
                if (!users.Contains(userName))
                    users.Add(userName);
                if (!userDic.ContainsKey(userName))
                    userDic.Add(userName, day);
                else
                {
                    var tuple = timeDic[userName];
                    //记录起始时间
                    timeDic[userName] = CompareTime(tuple, startTime, endTime);
                    userDic[userName] = timeDic[userName].Item2 < timeDic[userName].Item1 ? 0 : double.Parse($"{(timeDic[userName].Item2 - timeDic[userName].Item1).TotalDays:F}");//zlt  
                }
                if (!timeDic.ContainsKey(userName))
                    timeDic.Add(userName, new Tuple<DateTime, DateTime>(startTime, endTime));
                else
                    timeDic[userName] = CompareTime(timeDic[userName], startTime, endTime);
            }
            users = users.OrderBy(r => r).ToList();//zlt
            return userDic;
        }

        private Tuple<DateTime, DateTime> CompareTime(Tuple<DateTime, DateTime> tuple, DateTime start, DateTime end)
        {
            var newStart = DateTime.MinValue;
            var newEnd = DateTime.MinValue;
            if (tuple.Item1 > start)
                newStart = start;
            if (tuple.Item2 < end)
                newEnd = end;
            return new Tuple<DateTime, DateTime>(newStart, newEnd);
        }

        //获取成员对比分析数据
        private object GetContributionInfo(List<int> sdIdList)
        {
            var typeList = new List<string>() { "1", "2", "3" };//1:完成量，2：资源消耗，3：效率
            var result = new Dictionary<string, object>();
            var users = new List<string>();

            var userList = new List<Tuple<string, string>>();
            foreach (var uu in sdIdList.Select(sdId => _sdService.GetSDDateItemMember(sdId)
            .ToList()).SelectMany(u => u.Where(uu => !userList.Contains(uu))))
            {
                if (uu == null) continue;
                userList.Add(uu);
            }
            var contributeDic = GetContributDic(userList, "2", sdIdList, ref users);
            var timeDic = GetDateItemTimeDic(userList, sdIdList, ref users);

            foreach (var type in typeList)
            {
                if (type == "1")
                    result.Add(type, GetDataItemContrOrTime(contributeDic, users));
                if (type == "2")
                    result.Add(type, GetDataItemContrOrTime(timeDic, users));
                if (type == "3")
                    result.Add(type, GetDataItemEfficiency(userList, contributeDic, timeDic, users));
            }
            return result;
        }

        private object GetDataItemContrOrTime(Dictionary<string, double> contrOrTimeDic, List<string> users)
        {
            try
            {
                var showData = contrOrTimeDic.Select(dic => new Dictionary<string, object>()
                    {
                        {"name", dic.Key}, {"value", dic.Value}
                    }).ToList();
                var data = new Dictionary<string, object>()
                    {
                        {"users", users.ToList()},
                        {"data", showData}
                    };
                return data;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        //获取成员数据项效率
        private object GetDataItemEfficiency(List<Tuple<string, string>> userList,Dictionary<string, double> contributionDic, Dictionary<string, double> timeDic, List<string> users)
        {
            try
            {
                var efficiencyList = new List<Dictionary<string, object>>();//new List<double>();
                foreach (var user in userList.OrderBy(r => r.Item2))//zlt
                {
                    var userName = user.Item2;
                    if (!contributionDic.ContainsKey(userName)) continue;
                    var efficiency = double.Parse($"{((!timeDic.ContainsKey(userName) || (timeDic[userName]) == 0) ? 0 : contributionDic[userName] / timeDic[userName]):F}");
                    efficiencyList.Add(new Dictionary<string, object>() {
                            {"name", userName}, {"value", efficiency}
                        });
                }

                var data = new Dictionary<string, object>()
                    {
                        {"users", users},
                        {"data", efficiencyList}
                     };
                return data;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        //获取成员数据项资源消耗（小时）
        private Dictionary<string, double> GetDateItemTimeDic(List<Tuple<string, string>> userList, List<int> sdIdList, ref List<string> users)
        {
            try
            {
                var userDic = new Dictionary<string, double>();
                foreach (var user in userList.Distinct())
                {
                    var hours = 0.00;
                    var userName = user.Item2;
                    sdIdList.ForEach(sdId =>
                    {
                        hours += _sdService.GetTimeCost(sdId.ToString(), user.Item1);
                    });
                    if (!userDic.ContainsKey(userName))
                        userDic.Add(userName, hours);
                    else
                        userDic[userName] = hours;

                    if (!users.Contains(userName))
                        users.Add(userName);
                }
                users = users.OrderBy(r => r).ToList();
                return userDic;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}