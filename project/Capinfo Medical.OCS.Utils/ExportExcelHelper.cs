using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace TestingCenterSystem.Utils
{
    public static class ExportExcelHelper
    {
        /// <summary>
        /// Convert a List{T} to a DataTable.
        /// </summary>
        private static DataTable ToDataTable<T>(List<T> items)
        {
            var tb = new DataTable(typeof(T).Name);

            PropertyInfo[] props = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (PropertyInfo prop in props)
            {
                Type t = GetCoreType(prop.PropertyType);
                tb.Columns.Add(prop.Name, t);
            }

            foreach (T item in items)
            {
                var values = new object[props.Length];

                for (int i = 0; i < props.Length; i++)
                {
                    values[i] = props[i].GetValue(item, null);
                }

                tb.Rows.Add(values);
            }

            return tb;
        }

        /// <summary>
        /// Determine of specified type is nullable
        /// </summary>
        public static bool IsNullable(Type t)
        {
            return !t.IsValueType || (t.IsGenericType && t.GetGenericTypeDefinition() == typeof(Nullable<>));
        }

        /// <summary>
        /// Return underlying type if type is Nullable otherwise return the type
        /// </summary>
        public static Type GetCoreType(Type t)
        {
            if (t != null && IsNullable(t))
            {
                if (!t.IsValueType)
                {
                    return t;
                }
                else
                {
                    return Nullable.GetUnderlyingType(t);
                }
            }
            else
            {
                return t;
            }
        }

        public static Tuple<bool, string> DataSetToExcel<T>(List<T> items, bool isShowExcle = false, string filename = "KPI指标.xlsx")
        {
            DataSet ds = new DataSet();
            ds.Tables.Add(ToDataTable(items));
            return DataSetToExcel(ds, isShowExcle, filename);
        }

        /// <summary>
        /// 将数据集中的数据导出到EXCEL文件
        /// </summary>
        /// <param name="dataSet">输入数据集</param>
        /// <param name="isShowExcle">是否显示该EXCEL文件</param>
        /// <returns></returns>
        public static Tuple<bool, string> DataSetToExcel(DataSet dataSet, bool isShowExcle = false, string filename = "KPI指标.xlsx")
        {
            //建立Excel对象 
            Microsoft.Office.Interop.Excel.Application excel = new Microsoft.Office.Interop.Excel.Application();
            //excel.Application.Workbooks.Add(true);
            Microsoft.Office.Interop.Excel.Workbook workbook = excel.Workbooks.Add(Microsoft.Office.Interop.Excel.XlWBATemplate.xlWBATWorksheet);
            Microsoft.Office.Interop.Excel.Worksheet worksheet = (Microsoft.Office.Interop.Excel.Worksheet)workbook.Worksheets[1];
            excel.Visible = isShowExcle;
            try
            {
                DataTable dataTable = dataSet.Tables[0];
                int rowNumber = dataTable.Rows.Count;//不包括字段名
                int columnNumber = dataTable.Columns.Count;
                int colIndex = 0;

                if (rowNumber == 0)
                {
                    return new Tuple<bool, string>(false, "无可导出数据！！！");
                }


                //Microsoft.Office.Interop.Excel.Worksheet worksheet = (Microsoft.Office.Interop.Excel.Worksheet)excel.Worksheets[1];
                Microsoft.Office.Interop.Excel.Range range;

                //生成字段名称 
                foreach (DataColumn col in dataTable.Columns)
                {
                    colIndex++;
                    excel.Cells[1, colIndex] = col.ColumnName;
                }

                object[,] objData = new object[rowNumber, columnNumber];

                for (int r = 0; r < rowNumber; r++)
                {
                    for (int c = 0; c < columnNumber; c++)
                    {
                        objData[r, c] = dataTable.Rows[r][c];
                    }
                    //Application.DoEvents();
                }

                // 写入Excel 
                range = worksheet.Range[excel.Cells[2, 1], excel.Cells[rowNumber + 1, columnNumber]];
                //range.NumberFormat = "@";//设置单元格为文本格式
                //range.Value2 = objData;
                range.Value = objData;
                //worksheet.Range[excel.Cells[2, 1], excel.Cells[rowNumber + 1, columnNumber]].NumberFormat = "yyyy-m-d h:mm";//1
                workbook.Saved = true;
                var filepath = HttpRuntime.AppDomainAppPath.ToString() + "Excel\\" + filename;
                workbook.SaveCopyAs(filepath);
                return new Tuple<bool, string>(true, string.Format("..\\..\\..\\Excel\\{0}",filename));
            }
            catch (Exception e)
            {
                return new Tuple<bool, string>(false, e.Message);
            }
            finally
            {
                workbook.Close(true, Type.Missing, Type.Missing);
                workbook = null;
                excel.Quit();
                excel = null;
            }

        }
    }
}
