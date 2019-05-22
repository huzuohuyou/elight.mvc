using System.Data;
using System.IO;
using System.Text;

namespace TestingCenterSystem.Utils.TransferData
{
   public class CsvTransferData:ITransferData
    {
        private readonly Encoding _encode;
        public CsvTransferData()
        {
            //_encode = Encoding.GetEncoding("utf-8");
            _encode = Encoding.Default;
        }

        public Stream GetStream(DataTable table)
        {
            var sb = new StringBuilder();
            if (table != null && table.Columns.Count > 0 && table.Rows.Count > 0)
            {
                foreach (DataRow item in table.Rows)
                {
                    for (int i = 0; i < table.Columns.Count; i++)
                    {
                        if (i > 0)
                        {
                            sb.Append(",");
                        }
                        if (item[i] != null)
                        {
                            sb.Append("\"").Append(item[i].ToString().Replace("\"", "\"\"")).Append("\"");
                        }
                    }
                    sb.Append("\n");
                }
            }
            var stream = new MemoryStream(_encode.GetBytes(sb.ToString()));
            return stream;
        }

        public DataTable GetData(Stream stream)
        {
            using (stream)
            {
                using (var input = new StreamReader(stream, _encode))
                {
                    var dt = new DataTable();
                    //记录每次读取的一行记录
                    var strLine = "";
                    //记录每行记录中的各字段内容
                    //标示列数
                    var columnCount = 0;
                    //标示是否是读取的第一行
                    var isFirst = true;
                    //逐行读取CSV中的数据
                    while ((strLine = input.ReadLine()) != null)
                    {
                        var aryLine = strLine.Split(',');
                        if (isFirst)
                        {
                            isFirst = false;
                            columnCount = aryLine.Length;
                            for (int i = 0; i < columnCount; i++)
                            {
                                var dc = new DataColumn(aryLine[i]);
                                dt.Columns.Add(dc);
                            }
                        }
                        else
                        {
                            var dr = dt.NewRow();
                            for (var j = 0; j < columnCount; j++)
                            {
                                dr[j] = aryLine[j];
                            }
                            dt.Rows.Add(dr);
                        }
                    }
                    return dt;
                    //using (CsvReader csv = new CsvReader(input, false))
                    //{

                    //    var dt = new DataTable();
                    //    int columnCount = csv.FieldCount;
                    //    for (int i = 0; i < columnCount; i++)
                    //    {
                    //        dt.Columns.Add("col" + i);
                    //    }

                    //    while (csv.ReadNextRecord())
                    //    {
                    //        var dr = dt.NewRow();
                    //        for (var i = 0; i < columnCount; i++)
                    //        {
                    //            if (!string.IsNullOrWhiteSpace(csv[i]))
                    //            {
                    //                dr[i] = csv[i];
                    //            }
                    //        }
                    //        dt.Rows.Add(dr);
                    //    }
                    //    return dt;
                    //}
                }
            }
        }
    }
}
