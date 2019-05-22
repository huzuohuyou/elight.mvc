using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Elight.Infrastructure;
using Elight.Web.Controllers;
using TestingCenterSystem.Utils.TransferData;
using TestingCenterSystem.ViewModels.ContrastTool;

namespace Elight.Web.Areas.TestCenterSystem.Controllers.ContrastTool
{
    public class ContrastToolController : BaseController
    {
        private const string ConstSessionKey = "ContrastToolModel";


        // GET: TestCenterSystem/ContrastTool
        public ActionResult Index()
        {
            ViewBag.Title = "对比工具";
            Session[ConstSessionKey] = new ContrastToolModel();
            return View();
        }

        /// <summary>
        /// 获取文件
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult GetXlsx(int id)
        {
            

            var file = Request.Files[0];
            if (file == null)
                return Json(new
                {
                    code = -1,
                    msg = "上传失败"
                });

            file.InputStream.Position = 0;
            var column = string.Empty;

            var util = TransferDataFactory.GetUtil(file.FileName);
            var dt = util.GetData(file.InputStream);

            foreach (var col in dt.Columns)
                column += string.IsNullOrEmpty(column) ? col : $"|{col}";

            var cToolM = (ContrastToolModel) Session[ConstSessionKey];
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

        /// <summary>
        /// 获取需要对比的字段名称
        /// </summary>
        /// <param name="cols">比较列</param>
        /// <param name="lsCol">左连接字段</param>
        /// <param name="rsCol">右连接字段</param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult ComparisonColumn(string[] cols,string lsCol,string rsCol)
        {
            var cToolM = (ContrastToolModel)Session[ConstSessionKey];

            if (cToolM.UploadFileInfo.Count != 2 || cols == null)
                return Json(new
                {
                    code = -1,
                    msg = "请先上传对比文件，或未关联字段映射。"
                });

            var isGuanLian = !string.IsNullOrEmpty(lsCol) && !string.IsNullOrEmpty(rsCol);

            var dt = new DataTable();
            dt.Columns.Add("col1");//对比列
            dt.Columns.Add("col2");//A独有的
            dt.Columns.Add("col3");//完全相同
            dt.Columns.Add("col4");//B独有
            dt.Columns.Add("col5");//值同
            dt.Columns.Add("col6");//值差异

            foreach (var col in cols)
            {
                var dr = dt.NewRow();
                dr["col1"] = col;
                var leftDt = cToolM.UploadFileInfo[1].Value;
                var rightDt = cToolM.UploadFileInfo[2].Value;

                var c = col.Split(':');
                //比较列
                var lc = c[0].Trim();
                var rc = c[1].Trim();

                var leftCol = lc;
                var rightCol = rc;

                //无关联比较用
                string[]  leftChaJi = null;
                string[] rightChaJi = null;
                string[] jiaoJi = null;

                //如果有关联条件，则差集和交集  比较关联列   如果没有，则差集、交集比较 对比列
                string[] identical = null;//相同
                string[] ldifference = null;//左差异
                string[] rdifference = null;//左差异

                if (isGuanLian)
                {
                    leftCol = string.IsNullOrEmpty(lsCol) ? c[0].Trim() : lsCol;
                    rightCol = string.IsNullOrEmpty(rsCol) ? c[1].Trim() : rsCol;

                    var lvalue =
                        leftDt.AsEnumerable()
                            .Select(r => string.Join("|", r.Field<string>(leftCol), r.Field<string>(lc)))
                            .ToList();
                    var rvalue =
                        rightDt.AsEnumerable()
                            .Select(r => string.Join("|", r.Field<string>(rightCol), r.Field<string>(rc)))
                            .ToArray();
                    identical = lvalue.Intersect(rvalue).ToArray();
                    ldifference = lvalue.Except(identical).ToArray();
                    rdifference = rvalue.Except(identical).ToArray();
                    dr["col5"] = identical.Length;
                    dr["col6"] = 0;
                }
                else
                {
                   var  leftList = leftDt.AsEnumerable().Select(r => r.Field<string>(leftCol)).ToList();
                  var  rightList = rightDt.AsEnumerable().Select(r => r.Field<string>(rightCol)).ToList();
                    //左列独有
                     leftChaJi = leftList.Except(rightList).ToArray();
                    dr["col2"] = leftChaJi.Length;
                    //交集
                     jiaoJi = leftList.Intersect(rightList).ToArray();
                    dr["col3"] = jiaoJi.Length;
                    //右列独有
                     rightChaJi = rightList.Except(leftList).ToArray();
                    dr["col4"] = rightChaJi.Length;
                    dr["col5"] = 0;
                    dr["col6"] = 0;
                }

                //异步组装JSON
                var pTask = Task.Factory.StartNew(() =>
                {
                    var isAddValueCol = isGuanLian;

                    #region CreateDataTable

                    var col1 = $"左列[{leftCol}]";
                    var col2 = "左列(值)";
                    var col3 = $"右列[{rightCol}]";
                    var col4 = "右列值)";
                    var col5 = "差异类型";
                    var col6 = "交集";
                    var col7 = "交集(值)";

                    var resultDt = new DataTable(col);
                    resultDt.Columns.Add(col1, typeof(string));
                    if (isAddValueCol)
                        resultDt.Columns.Add(col2, typeof(string));
                    resultDt.Columns.Add(col3, typeof(string));
                    if (isAddValueCol)
                        resultDt.Columns.Add(col4, typeof(string));
                    if (isAddValueCol)
                        resultDt.Columns.Add(col5, typeof(string));
                    resultDt.Columns.Add(col6, typeof(string));
                    if (isAddValueCol)
                        resultDt.Columns.Add(col7, typeof(string));
                    foreach (DataColumn column in resultDt.Columns)
                    {
                        column.DefaultValue = string.Empty;
                        column.DataType = typeof(string);
                    }

                    #endregion

                    //var maxLeng = isAddValueCol ? new[] {l.Length + r.Length + j.Length - (jval?.Length ?? 0), jval?.Length ?? 0}.Max() : new[] {l.Length + r.Length, j.Length}.Max();

                    if (isAddValueCol)
                    {
                        var jval = identical?.ToArray() ?? new string[] { }; //相同键值
                        var lval = ldifference?.ToArray() ?? new string[] { }; //左不同键值
                        var rval = rdifference?.ToArray() ?? new string[] { }; //右不同键值

                        #region 有关联关系比较

                        //var zcyRow = new List<DataRow>(); //值差异
                        //var zdyRow = new List<DataRow>(); //左独有
                        //var ydyRow = new List<DataRow>(); //右独有
                        //01 值差异    02  左列独有    03 右列独有
                        var resultRows = new List<DataRow>();

                        foreach (var s in lval)
                        {
                            var kv = s.Split('|');
                            var k = kv[0];
                            var v = kv[1];

                            var row = resultDt.NewRow();

                            row[col1] = k;
                            row[col2] = v;
                            row[col5] = "02";
                            resultRows.Add(row);
                        }

                        foreach (var s in rval)
                        {
                            var kv = s.Split('|');
                            var k = kv[0];
                            var v = kv[1];

                            var zdyR =
                                resultRows.FirstOrDefault(n => n[col1].ToString() == k && n[col5].ToString() == "02");
                            if (zdyR != null)
                            {
                                zdyR[col3] = k;
                                zdyR[col4] = v;
                                zdyR[col5] = "01";
                                continue;
                            }

                            var row = resultDt.NewRow();
                            row[col3] = k;
                            row[col4] = v;
                            row[col5] = "03";
                            resultRows.Add(row);
                        }
                        //左 独有
                        dr["col2"] = resultRows.Count(n => n[col5].ToString() == "02");
                        //值差异
                        var zcyCount = resultRows.Count(n => n[col5].ToString() == "01");
                        //交集
                        dr["col3"] = zcyCount + jval.Length;
                        //右列独有
                        dr["col4"] = resultRows.Count(n => n[col5].ToString() == "03");
                        dr["col5"] = jval.Length;
                        dr["col6"] = zcyCount;
                        
                        var jjIndex = 0;
                        foreach (var row in resultRows.OrderBy(n => n[col5]))
                        {
                            if (jjIndex < jval.Length)
                            {
                                var jkv = jval[jjIndex].Split('|');
                                row[col6] = jkv[0];
                                row[col7] = jkv[1];
                                jjIndex++;
                            }
                            
                            row[col5] = row[col5].ToString() == "01"
                                ? "值差异"
                                : (row[col5].ToString() == "02" ? "左列独有" : "右列独有");

                            resultDt.Rows.Add(row);
                        }

                        if (jval.Length > jjIndex)
                        {
                            for (; jjIndex < jval.Length; jjIndex++)
                            {
                                var row = resultDt.NewRow();
                                var jkv = jval[jjIndex].Split('|');

                                row[col6] = jkv[0];
                                row[col7] = jkv[1];

                                resultDt.Rows.Add(row);
                            }
                        }

                        #endregion
                    }
                    else
                    {
                        var l = leftChaJi?.ToArray() ?? new string[] { };
                        var j = jiaoJi?.ToArray() ?? new string[] { };
                        var r = rightChaJi?.ToArray() ?? new string[] { };

                        //单纯比较
                        var jjIndex = 0;
                        foreach (var k in l)
                        {
                            var row = resultDt.NewRow();
                            row[col1] = k;
                            if (j.Length > jjIndex)
                                row[col6] = j[jjIndex];
                            resultDt.Rows.Add(row);
                            jjIndex++;
                        }
                        foreach (var k in r)
                        {
                            var row = resultDt.NewRow();
                            row[col3] = k;
                            if (j.Length > jjIndex)
                                row[col6] = j[jjIndex];
                            resultDt.Rows.Add(row);
                            jjIndex++;
                        }
                        if (j.Length > jjIndex)
                        {
                            for (; jjIndex < j.Length; jjIndex++)
                            {
                                var row = resultDt.NewRow();
                                row[col6] = j[jjIndex];
                                resultDt.Rows.Add(row);
                            }
                        }
                    }

                    if (cToolM.ExportResult.Keys.Contains(resultDt.TableName))
                        cToolM.ExportResult[resultDt.TableName] = resultDt.ToJson();
                    else
                        cToolM.ExportResult.Add(col, resultDt.ToJson());
                });
               
                //如果是值比较，则等待Task的结束
                if (isGuanLian)
                    pTask.Wait();

                dt.Rows.Add(dr);
            }


            var json = Json(new
            {
                code = 0,
                cols,
                data = dt.ToJson()
            });
            return json;
        }

        /// <summary>
        /// 结果详情
        /// </summary>
        /// <returns></returns>
        public ActionResult ResultDetail(string cols)
        {
            var cToolM = (ContrastToolModel)Session[ConstSessionKey];
            var result = cToolM.ExportResult?[cols];
            return Content(result);
        }

        public ActionResult ValueDetail(string cols)
        {
            try
            {
                ViewBag.Name = cols;
                ViewBag.route =new MvcHtmlString("/TestCenterSystem/ContrastTool/ResultDetail?cols="+ cols) ;
                return View("ValueDetail");
            }
            catch (Exception ex)
            {
                LogHelper.Error("ExportDetail:" + ex);
                throw;
            }
        }
    }
}