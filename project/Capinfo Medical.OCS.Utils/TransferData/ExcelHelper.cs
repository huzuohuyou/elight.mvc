using System;
using System.Data;
using System.IO;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;

namespace TestingCenterSystem.Utils.TransferData
{
    public class ExcelHelper : IDisposable
    {
        private readonly string _fileName; //文件名
        private IWorkbook _workbook;
        private FileStream _fs;
        private bool _disposed;

        public ExcelHelper(string fileName)
        {
            _fileName = fileName;
            _disposed = false;
        }

        /// <summary>
        /// 将DataTable数据导入到excel中
        /// </summary>
        /// <param name="data">要导入的数据</param>
        /// <param name="isColumnWritten">DataTable的列名是否要导入</param>
        /// <param name="sheetName">要导入的excel的sheet的名称</param>
        /// <returns>导入数据行数(包含列名那一行)</returns>
        public int DataTableToExcel(DataTable data, string sheetName, bool isColumnWritten)
        {
            int i = 0;
            int j = 0;
            int count = 0;
            ISheet sheet = null;

            _fs = new FileStream(_fileName, FileMode.OpenOrCreate, FileAccess.ReadWrite);
            if (_fileName.IndexOf(".xlsx") > 0) // 2007版本
                _workbook = new XSSFWorkbook();
            else if (_fileName.IndexOf(".xls") > 0) // 2003版本
                _workbook = new HSSFWorkbook();

            try
            {
                if (_workbook != null)
                {
                    sheet = _workbook.CreateSheet(sheetName);
                }
                else
                {
                    return -1;
                }

                if (isColumnWritten == true) //写入DataTable的列名
                {
                    IRow row = sheet.CreateRow(0);
                    for (j = 0; j < data.Columns.Count; ++j)
                    {
                        row.CreateCell(j).SetCellValue(data.Columns[j].ColumnName);
                    }
                    count = 1;
                }
                else
                {
                    count = 0;
                }

                for (i = 0; i < data.Rows.Count; ++i)
                {
                    IRow row = sheet.CreateRow(count);
                    for (j = 0; j < data.Columns.Count; ++j)
                    {
                        row.CreateCell(j).SetCellValue(data.Rows[i][j].ToString());
                    }
                    ++count;
                }
                _workbook.Write(_fs); //写入到excel
                return count;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception: " + ex.Message);
                return -1;
            }
        }

        /// <summary>
        /// 将excel中的数据导入到DataTable中
        /// </summary>
        /// <param name="sheetName">excel工作薄sheet的名称</param>
        /// <param name="isFirstRowColumn">第一行是否是DataTable的列名</param>
        /// <returns>返回的DataTable</returns>
        public DataTable ExcelToDataTable(string sheetName, bool isFirstRowColumn)
        {
            _fs = new FileStream(_fileName, FileMode.Open, FileAccess.Read);
            return ExcelToDataTable(_fs, sheetName, isFirstRowColumn);
        }

        /// <summary>
        /// 将excel中的数据导入到DataTable中
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="sheetName">excel工作薄sheet的名称</param>
        /// <param name="isFirstRowColumn">第一行是否是DataTable的列名</param>
        /// <returns>返回的DataTable</returns>
        public DataTable ExcelToDataTable(Stream stream, string sheetName, bool isFirstRowColumn)
        {
            var data = new DataTable();
            try
            {
                if (_fileName.IndexOf(".xlsx") > 0) // 2007版本
                    _workbook = new XSSFWorkbook(stream);
                else if (_fileName.IndexOf(".xls") > 0) // 2003版本
                    _workbook = new HSSFWorkbook(stream);

                ISheet sheet;
                if (sheetName != null)
                {
                    sheet = _workbook.GetSheet(sheetName) ?? _workbook.GetSheetAt(0);
                }
                else
                {
                    sheet = _workbook.GetSheetAt(0);
                }

                if (sheet == null) return data;

                var firstRow = sheet.GetRow(0);
                int cellCount = firstRow.LastCellNum; //一行最后一个cell的编号 即总的列数

                int startRow;
                if (isFirstRowColumn)
                {
                    for (int i = firstRow.FirstCellNum; i < cellCount; ++i)
                    {
                        var cell = firstRow.GetCell(i);
                        var cellValue = cell?.StringCellValue;
                        if (cellValue != null)
                        {
                            var column = new DataColumn(cellValue);
                            data.Columns.Add(column);
                        }
                    }
                    startRow = sheet.FirstRowNum + 1;
                }
                else
                {
                    startRow = sheet.FirstRowNum;
                }

                //最后一列的标号
                var rowCount = sheet.LastRowNum;
                for (var i = startRow; i <= rowCount; ++i)
                {
                    var row = sheet.GetRow(i);
                    if (row == null) continue; //没有数据的行默认是null　　　　　　　

                    var dataRow = data.NewRow();
                    for (int j = row.FirstCellNum; j < cellCount; ++j)
                    {
                        if (row.GetCell(j) != null) //同理，没有数据的单元格都默认是null
                            dataRow[j] = row.GetCell(j).ToString();
                    }
                    data.Rows.Add(dataRow);
                }

                return data;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception: " + ex.Message);
                return null;
            }
        }

        /// <summary>
        /// 获取列名
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="sheetName"></param>
        /// <returns></returns>
        public string GetExcelColumn(Stream stream, string sheetName)
        {
            try
            {

                if (_fileName.IndexOf(".xlsx") > 0) // 2007版本
                    _workbook = new XSSFWorkbook(stream);
                else if (_fileName.IndexOf(".xls") > 0) // 2003版本
                    _workbook = new HSSFWorkbook(stream);

                ISheet sheet;
                //列名
                var columnName = string.Empty;

                if (sheetName != null)
                {
                    sheet = _workbook.GetSheet(sheetName) ?? _workbook.GetSheetAt(0);
                }
                else
                {
                    sheet = _workbook.GetSheetAt(0);
                }
                if (sheet == null) return string.Empty;
                var firstRow = sheet.GetRow(0);
                int cellCount = firstRow.LastCellNum; //一行最后一个cell的编号 即总的列数
                for (int i = firstRow.FirstCellNum; i < cellCount; ++i)
                {
                    var cell = firstRow.GetCell(i);
                    var cellValue = cell?.StringCellValue;
                    if (cellValue != null)
                    {
                        columnName += string.IsNullOrEmpty(columnName) ? cellValue : $"|{cellValue}";
                    }
                }
                return columnName;
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception: " + e.Message);
                return string.Empty;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (this._disposed) return;
            if (disposing)
            {
                _fs?.Close();
            }

            _fs = null;
            _disposed = true;
        }
    }
}
