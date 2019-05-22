using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestingCenterSystem.Utils.TransferData
{
   public  class TransferDataFactory
    {
        public static ITransferData GetUtil(string fileName)
        {
            var array = fileName.Split('.');
            var dataType = (DataFileType)Enum.Parse(typeof(DataFileType), array[array.Length - 1], true);
            return GetUtil(dataType);
        }

        public static ITransferData GetUtil(DataFileType dataType)
        {
            switch (dataType)
            {
                case DataFileType.Csv: return new CsvTransferData();
                case DataFileType.Xls: return new XlsTransferData();
                case DataFileType.Xlsx: return new XlsxTransferData();
                default: return new CsvTransferData();
            }
        }
        public enum DataFileType
        {
            Csv,
            Xls,
            Xlsx
        }
    }
}
