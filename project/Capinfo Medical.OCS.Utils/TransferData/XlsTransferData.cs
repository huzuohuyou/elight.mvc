using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NPOI.HSSF.UserModel;

namespace TestingCenterSystem.Utils.TransferData
{
    public class XlsTransferData : ExcelTransferData
    {
        public override Stream GetStream(DataTable table)
        {
            WorkBook = new HSSFWorkbook();
            return base.GetStream(table);
        }

        public override DataTable GetData(Stream stream)
        {
            WorkBook = new HSSFWorkbook(stream);
            return base.GetData(stream);
        }
    }
}
