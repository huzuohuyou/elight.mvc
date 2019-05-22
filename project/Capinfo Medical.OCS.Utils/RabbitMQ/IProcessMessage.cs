using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TestingCenterSystem.Utils.RabbitMQ
{
    public interface IProcessMessage
    {
        void ProcessMsg(BaseMessage msg);
        
    }
}
