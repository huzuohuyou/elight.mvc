using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TestingCenterSystem.Models.Unit;
using TestingCenterSystem.ViewModels.DataItem;

namespace TestingCenterSystem.Utils.RabbitMQ
{
    public class DataItemMessage : BaseMessage
    {
        public string CurrentUser { get; set; }
        public ITEM_MSG_BODY_VIEWMODEL MessageBody { get; set; }
    }
}
