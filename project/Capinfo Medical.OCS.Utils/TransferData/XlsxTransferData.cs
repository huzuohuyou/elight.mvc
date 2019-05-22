using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NPOI.XSSF.UserModel;

namespace TestingCenterSystem.Utils.TransferData
{
   public  class XlsxTransferData:ExcelTransferData
    {
        public override Stream GetStream(DataTable table)
        {
            WorkBook = new XSSFWorkbook();
            return base.GetStream(table);
        }

        public override DataTable GetData(Stream stream)
        {
            WorkBook = new XSSFWorkbook(stream);
            return base.GetData(stream);
        }
    }
}
