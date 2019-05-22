using System.Data;
using System.IO;

namespace TestingCenterSystem.Utils.TransferData
{
    /// <summary>
    /// 数据转换接口
    /// </summary>
    public interface ITransferData
    {
        Stream GetStream(DataTable table);
        DataTable GetData(Stream stream);
    }
}
