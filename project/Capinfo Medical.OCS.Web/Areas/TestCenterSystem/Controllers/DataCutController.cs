using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using EasyNetQ.SystemMessages;
using Elight.Infrastructure;
using HJ.Common.Funcs;
using TestingCenterSystem.Utils.TransferData;
using TestingCenterSystem.ViewModels;
using TestingCenterSystem.ViewModels.ContrastTool;
using TestingCenterSystem.ViewModels.DataCut;

namespace Elight.Web.Areas.TestCenterSystem.Controllers
{
    public class DataCutController : Controller
    {
        private const string ConstSessionKey = "ContrastToolModel";
        // GET: TestCenterSystem/DataCut
        public ActionResult Index()
        {
            Session[ConstSessionKey] = new ContrastToolModel();
            return View();
        }
        //获取文件
        [HttpPost]
        public ActionResult UploadFile(int id)
        {
            var file = Request.Files[0];
            if (file == null)
                return Json(new { code = -1, msg = "上传失败" });
            file.InputStream.Position = 0;
            var column = string.Empty;

            var util = TransferDataFactory.GetUtil(file.FileName);
            var dt = util.GetData(file.InputStream);

            foreach (var col in dt.Columns)
                column += string.IsNullOrEmpty(column) ? col : $"|{col}";

            var cToolM = (ContrastToolModel)Session[ConstSessionKey];
            if (cToolM != null)
            {
                if (cToolM.UploadFileInfo.Keys.Contains(id))
                    cToolM.UploadFileInfo[id] = new KeyValuePair<string, DataTable>(file.FileName, dt);
                else
                    cToolM.UploadFileInfo.Add(id, new KeyValuePair<string, DataTable>(file.FileName, dt));
            }
            else
                column = string.Empty;

            var json = Json(new
            {
                code = string.IsNullOrEmpty(column) ? -1 : 0,
                filename = file.FileName,
                cols = column,
                msg = "上传成功。"
            }
                );
            return json;
        }
        //获取上传的文件数据
        public ActionResult GetFileData(int id, int page, int limit)
        {
            var cToolM = (ContrastToolModel)Session[ConstSessionKey];
            if (cToolM != null)
            {
                var dt = cToolM.UploadFileInfo[id].Value;
                var dicList = new List<Dictionary<string, string>>();
                foreach (DataRow row in dt.Rows)
                {
                    var dict = new Dictionary<string, string>();
                    for (var index = 0; index < dt.Columns.Count; index++)
                    {
                        dict.Add(dt.Columns[index].ColumnName, row.ItemArray[index].ToString());
                    }
                    dicList.Add(dict);
                }
                var pageDict = GetPage(dicList, page, limit);
                var result = new Dictionary<string, object>()
                {
                    {"code", 0},
                    {"msg", "success"},
                    {"data", pageDict.Items},
                    {"count", pageDict.TotalItems}
                };
                return Content(result.ToJson());
            }
            return Content("");
        }
        //分页展示结果
        public Page<Dictionary<string, string>> GetPage(List<Dictionary<string, string>> dictList, int pageIndex, int pageSize)
        {
            var totalCount = dictList.Count;
            var resultList = dictList.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToList();
            var page = new Page<Dictionary<string, string>>()
            {
                CurrentPage = pageIndex,
                ItemsPerPage = pageSize,
                TotalItems = totalCount,
                Items = resultList
            };
            return page;
        }
        //导出数据
        public ActionResult ExportCutData(int id)
        {
            var cToolM = (ContrastToolModel)Session[ConstSessionKey];
            var data = new Dictionary<string, List<Dictionary<string, string>>>();
            if (cToolM != null)
            {
                var dt = cToolM.UploadFileInfo[id].Value;
                var dicList = new List<Dictionary<string, string>>();
                var title = new Dictionary<string, string>();
                foreach (DataRow row in dt.Rows)
                {
                    var dict = new Dictionary<string, string>();
                    for (var index = 0; index < dt.Columns.Count; index++)
                    {
                        if (!title.ContainsKey(dt.Columns[index].ColumnName))
                            title.Add(dt.Columns[index].ColumnName, dt.Columns[index].ColumnName);
                        dict.Add(dt.Columns[index].ColumnName, row.ItemArray[index].ToString());
                    }
                    dicList.Add(dict);
                }
                data.Add("title", new List<Dictionary<string, string>>() { title });
                data.Add("data", dicList);
            }
            return Content(data.ToJson());
        }
        //清除当前截取字段数据
        public ActionResult ClearCutData(int id, string columnname, int page, int limit)
        {
            var cToolM = (ContrastToolModel)Session[ConstSessionKey];
            var newColumn = columnname + "_Cut";
            if (cToolM != null)
            {
                var dt = cToolM.UploadFileInfo[id].Value;
                var fileName = cToolM.UploadFileInfo[id].Key;
                var dicList = new List<Dictionary<string, string>>();
                if (dt.Columns.Contains(newColumn))
                    dt.Columns.Remove(newColumn);
                foreach (DataRow row in dt.Rows)
                {
                    var dict = new Dictionary<string, string>();
                    for (var index = 0; index < dt.Columns.Count; index++)
                    {
                        if (Regex.IsMatch(dt.Columns[index].ColumnName, newColumn + @"+\d+"))
                            dt.Columns.Remove(dt.Columns[index].ColumnName);
                        else
                            dict.Add(dt.Columns[index].ColumnName, row.ItemArray[index].ToString());
                    }
                    dicList.Add(dict);
                }
                cToolM.UploadFileInfo[id] = new KeyValuePair<string, DataTable>(fileName, dt);
                var pageDict = GetPage(dicList, page, limit);
                var result = new Dictionary<string, object>()
                {
                    {"code", 0},
                    {"msg", "success"},
                    {"data", pageDict.Items},
                    {"count", pageDict.TotalItems}
                };
                return Content(result.ToJson());
            }
            return Content("");
        }
        public ActionResult CutData(DATACUT_MODEL model, int page, int limit)
        {
            var cToolM = (ContrastToolModel)Session[ConstSessionKey];
            if (string.IsNullOrWhiteSpace(model.CoreWord) || (string.IsNullOrWhiteSpace(model.BeforeWord) && string.IsNullOrWhiteSpace(model.AfterWord)))
                return Content("");
            var newColumn = model.ResultName;
            if (cToolM != null)
            {
                var dt = cToolM.UploadFileInfo[model.Id].Value;
                var fileName = cToolM.UploadFileInfo[model.Id].Key;
                var dicList = new List<Dictionary<string, string>>();
                try
                {
                    if (!dt.Columns.Contains(newColumn) && model.NumType.Equals("single"))
                        dt.Columns.Add(newColumn);
                    foreach (DataRow row in dt.Rows)
                    {
                        var dict = new Dictionary<string, string>();
                        for (var index = 0; index < dt.Columns.Count; index++)
                        {
                            if (!dict.ContainsKey(dt.Columns[index].ColumnName))
                                dict.Add(dt.Columns[index].ColumnName, row.ItemArray[index].ToString());
                            if (model.ColumnName.Equals(dt.Columns[index].ColumnName))
                            {
                                var content = row.ItemArray[index].ToString();
                                if (model.NumType.Equals("single"))
                                {
                                    var reContent = CutWord(content, model.CoreWord, model.BeforeWord, model.BeforeType, model.AfterWord,
                                        model.AfterType);
                                    if (!dict.ContainsKey(newColumn))
                                        dict.Add(newColumn, reContent);
                                    else
                                        dict[newColumn] = reContent;
                                    row[newColumn] = reContent;
                                }
                                else
                                {
                                    var resContentList = CutWordToList(content, model.CoreWord, model.BeforeWord, model.BeforeType, model.AfterWord,
                                        model.AfterType);
                                    var colDict = GetRowData(resContentList.ToArray(), newColumn, ref dt);
                                    foreach (var col in colDict)
                                    {
                                        if (!dict.ContainsKey(col.Key))
                                            dict.Add(col.Key, col.Value);
                                        else
                                            dict[col.Key] = col.Value;
                                        row[col.Key] = col.Value;
                                    }
                                }
                            }
                        }
                        dicList.Add(dict);
                    }
                }
                catch (Exception ex)
                {

                }
                cToolM.UploadFileInfo[model.Id] = new KeyValuePair<string, DataTable>(fileName, dt);
                var pageDict = GetPage(dicList, page, limit);
                var result = new Dictionary<string, object>()
                {
                    {"code", 0},
                    {"msg", "success"},
                    {"data", pageDict.Items},
                    {"count", pageDict.TotalItems}
                };
                return Content(result.ToJson());
            }
            return Content("");
        }
        //TODO:规则判断未加
        //单条截取
        private string CutWord(string content, string coreword, string beforeword, string beforetype, string afterword, string aftertype)
        {
            var reContent = "";
            if (string.IsNullOrWhiteSpace(beforeword))
            {
                //截取后置词
                if (string.IsNullOrWhiteSpace(afterword)) return reContent;
                if (aftertype.Equals("string"))
                    reContent = content.CoreWord(coreword).after.keyWord(afterword).withoutCore.withoutKey.endStr;
                else if (aftertype.Equals("length"))
                    reContent = CutLength(content, afterword, coreword, false);
            }
            else
            {
                if (string.IsNullOrWhiteSpace(afterword))
                {
                    //截取前置词
                    if (beforetype.Equals("string"))
                        reContent = content.CoreWord(coreword).before.keyWord(beforeword).withoutCore.withoutKey.endStr;
                    else if (beforetype.Equals("length"))
                        reContent = CutLength(content, beforeword, coreword, true);
                }
                else
                {
                    //前后截取
                    if (beforetype.Equals("string") && aftertype.Equals("string"))
                        reContent = content.CoreWord(coreword).both.keyWord(beforeword, afterword).endStr;
                    else if (beforetype.Equals("length") && aftertype.Equals("length"))
                        reContent = content.CoreWord(coreword).both.length(int.Parse(beforeword), int.Parse(afterword)).endStr;
                    else if (beforetype.Equals("string") && aftertype.Equals("length"))
                        reContent = content.CoreWord(coreword).both.mix(beforeword, int.Parse(afterword)).endStr;
                    else if (beforetype.Equals("length") && aftertype.Equals("string"))
                        reContent = content.CoreWord(coreword).both.mix(int.Parse(beforeword), afterword).endStr;
                }
            }
            return reContent;
        }
        //多条截取
        private List<string> CutWordToList(string content, string coreword, string beforeword, string beforetype, string afterword, string aftertype)
        {
            var reContent = new List<string>();
            if (string.IsNullOrWhiteSpace(beforeword))
            {
                //截取后置词
                if (string.IsNullOrWhiteSpace(afterword)) return reContent;
                if (aftertype.Equals("string"))
                    reContent = content.CoreWord(coreword).after.keyWord(afterword).end;
                else if (aftertype.Equals("length"))
                    reContent = CutLengthToList(content, afterword, coreword, false);
            }
            else
            {
                if (string.IsNullOrWhiteSpace(afterword))
                {
                    //截取前置词
                    if (beforetype.Equals("string"))
                        reContent = content.CoreWord(coreword).before.keyWord(beforeword).end;
                    else if (beforetype.Equals("length"))
                        reContent = CutLengthToList(content, beforeword, coreword, true);
                }
                else
                {
                    //前后截取
                    if (beforetype.Equals("string") && aftertype.Equals("string"))
                        reContent = content.CoreWord(coreword).both.keyWord(beforeword, afterword).end;
                    else if (beforetype.Equals("length") && aftertype.Equals("length"))
                        reContent = content.CoreWord(coreword).both.length(int.Parse(beforeword), int.Parse(afterword)).end;
                    else if (beforetype.Equals("string") && aftertype.Equals("length"))
                        reContent = content.CoreWord(coreword).both.mix(beforeword, int.Parse(afterword)).end;
                    else if (beforetype.Equals("length") && aftertype.Equals("string"))
                        reContent = content.CoreWord(coreword).both.mix(int.Parse(beforeword), afterword).end;
                }
            }
            return reContent;
        }
        //单条长度截取
        private string CutLength(string content, string keyword, string coreword, bool isBefore)
        {
            var res = "";
            var array = keyword.Split(',');
            if (array.Length == 2 && !string.IsNullOrWhiteSpace(array[1]))
            {
                res = isBefore ? content.CoreWord(coreword).before.length(int.Parse(array[0]), int.Parse(array[1])).endStr :
                    content.CoreWord(coreword).after.length(int.Parse(array[0]), int.Parse(array[1])).endStr;
            }
            else
            {
                res = isBefore ? content.CoreWord(coreword).before.length(int.Parse(keyword)).endStr :
                    content.CoreWord(coreword).after.length(int.Parse(keyword)).endStr;
            }
            return res;
        }
        //多条长度截取
        private List<string> CutLengthToList(string content, string keyword, string coreword, bool isBefore)
        {
            List<string> res = new List<string>();
            var array = keyword.Split(',');
            if (array.Length == 2 && !string.IsNullOrWhiteSpace(array[1]))
            {
                res = isBefore ? content.CoreWord(coreword).before.length(int.Parse(array[0]), int.Parse(array[1])).end :
                    content.CoreWord(coreword).after.length(int.Parse(array[0]), int.Parse(array[1])).end;
            }
            else
            {
                res = isBefore ? content.CoreWord(coreword).before.length(int.Parse(keyword)).end :
                    content.CoreWord(coreword).after.length(int.Parse(keyword)).end;
            }
            return res;
        }
        //动态添加列
        private Dictionary<string, string> GetRowData(string[] list, string columnName, ref DataTable dt)
        {
            var index = 0;
            var dict = new Dictionary<string, string>();
            for (var i = 0; i < list.Length; i++)
            {
                if (!string.IsNullOrWhiteSpace(list[i]))
                {
                    var name = columnName + (list.Length > 1 ? (index + 1).ToString() : "");
                    if (!dict.ContainsKey(name))
                        dict.Add(name, list[i]);
                    if (!dt.Columns.Contains(name))
                        dt.Columns.Add(name);
                    index++;
                }
            }
            return dict;
        }
    }
}