using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestingCenterSystem.Utils.RabbitMQ
{
    public interface ICanCheckMessage
    {
        void ChechMessage(BaseMessage msg);
    }
}
