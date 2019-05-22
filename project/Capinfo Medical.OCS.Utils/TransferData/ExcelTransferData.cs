using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NPOI.SS.UserModel;

namespace TestingCenterSystem.Utils.TransferData
{
    /// <summary>
    /// Excel 转换 数据
    /// </summary>
    public class ExcelTransferData: ITransferData
    {
        protected IWorkbook WorkBook;

        public virtual Stream GetStream(DataTable table)
        {
            var sheet = WorkBook.CreateSheet();
            if (table != null)
            {
                var rowCount = table.Rows.Count;
                for (int i = 0; i < table.Rows.Count; i++)
                {
                    var row = sheet.CreateRow(i);
                    for (int j = 0; j < table.Columns.Count; j++)
                    {
                        var cell = row.CreateCell(j);
                        if (table.Rows[i][j] != null)
                        {
                            cell.SetCellValue(table.Rows[i][j].ToString());
                        }
                    }
                }
            }
            MemoryStream ms = new MemoryStream();
            WorkBook.Write(ms);
            return ms;
        }

        public virtual DataTable GetData(Stream stream)
        {
            using (stream)
            {
                var sheet = WorkBook.GetSheetAt(0);
                if (sheet != null)
                {
                    var headerRow = sheet.GetRow(0);
                    var dt = new DataTable();
                    int columnCount = headerRow.Cells.Count;

                    for (int i = headerRow.FirstCellNum; i < columnCount; i++)
                    {
                        var cell = headerRow.GetCell(i);
                        var cellValue = cell?.StringCellValue;
                        if (cellValue != null)
                        {
                            var column = new DataColumn(cellValue);
                            dt.Columns.Add(column);
                        }
                    }
                    var row = sheet.GetRowEnumerator();

                    while (row.MoveNext())
                    {
                        var dtRow = dt.NewRow();
                        var excelRow = row.Current as IRow;
                        for (var i = 0; i < columnCount; i++)
                        {
                            var cell = excelRow?.GetCell(i);

                            if (cell != null)
                            {
                                dtRow[i] = GetValue(cell);
                            }
                        }
                        dt.Rows.Add(dtRow);
                    }
                    dt.Rows.RemoveAt(0);
                    return dt;
                }
            }

            return null;
        }


        private object GetValue(ICell cell)
        {
            object value = null;
            switch (cell.CellType)
            {
                case CellType.Blank:
                    break;
                case CellType.Boolean:
                    value = cell.BooleanCellValue ? "1" : "0";
                    break;
                case CellType.Error:
                    value = cell.ErrorCellValue;
                    break;
                case CellType.Formula:
                    value = "=" + cell.CellFormula;
                    break;
                case CellType.Numeric:
                    value = cell.NumericCellValue.ToString();
                    break;
                case CellType.String:
                    value = cell.StringCellValue;
                    break;
                case CellType.Unknown:
                    break;
            }
            return value;
        }
    }
}
