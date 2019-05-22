using Elight.Infrastructure;
using Elight.Web.Controllers;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Web.Mvc;

namespace Elight.Web.Areas.TestCenterSystem.Controllers
{
    public class ScreeningToolController : BaseController
    {
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult Form(string primaryKey)
        {
            return View();
        }
        public ActionResult DeleteForm(string primaryKey)
        {
            return View();
        }
        public ActionResult DeleteTableOrItem(string primaryKey, string IsTableOrItem)
        {
            var primaryKeyList = new List<string>(primaryKey.Split(';'));
            return Success();
        }
        public ActionResult JudgmentAuthority(string primaryKey)
        {
            //var result = new Dictionary<string, object>() {
            //    { "code", 0},
            //    { "msg","success"},
            //    { "data", hasExeItems},
            //    { "count", hasExeItems.Count}
            //};
            return Content(true.ToJson());
        }

        public ActionResult RegexTest()
        {
            return View();
        }
        /// <summary>
        /// 正则表达式测试匹配或替换
        /// </summary>
        /// <param name="matchedText">待匹配文本</param>
        /// <param name="expression">正则表达式</param>
        /// <param name="globalSearch">全局搜索</param>
        /// <param name="ignoreCase">忽略大小写</param>
        /// <param name="replaceText">替换文本</param>
        /// <param name="flag">标志：0:测试匹配；1:替换</param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult RegexTestOrReplace(string matchedText, string expression, string globalSearch, string ignoreCase, string replaceText, int flag)
        {
            try
            {
                string strMatch = null;
                var matchList = new List<string>();
                Regex regex = new Regex(expression);
                if (ignoreCase == "on")
                    regex = new Regex(expression, RegexOptions.IgnoreCase);
                if (flag == 0)
                {
                    if (globalSearch == "on" && regex.IsMatch(matchedText))
                    {
                        var matches = regex.Matches(matchedText);
                        matchList.Add("共找到" + matches.Count + "处匹配：");
                        foreach (Match item in matches)
                        {
                            matchList.Add(item.Value);
                        }
                        strMatch = string.Join("\n", matchList);
                    }
                    if (globalSearch != "on" && regex.IsMatch(matchedText))
                    {
                        matchList.Add("匹配位置：0");
                        matchList.Add("匹配结果：" + regex.Match(matchedText).Value);
                        strMatch = string.Join("\n", matchList);
                    }
                }
                else if (flag == 1)
                {
                    //var matches = regex.Matches(matchedText);
                    if (globalSearch != "on")
                        strMatch = regex.Replace(matchedText, replaceText,1);
                    else
                        strMatch = regex.Replace(matchedText, replaceText); //regex.Replace(replaceText, new MatchEvaluator(OutPutMatch));
                }
                var result = new Dictionary<string, object>() {
                { "msg","success"},
                { "data", strMatch},
                 };
                return Content(result.ToJson());
            }
            catch (Exception ex)
            {
                var result = new Dictionary<string, object>() {
                { "msg","error"},
                { "data", ex.Message},
                 };
                return Content(result.ToJson());
            }
        }
        public ActionResult LableForm(string primaryKey)
        {
            return View();
        }
    }
}