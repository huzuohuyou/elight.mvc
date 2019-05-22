using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using EasyNetQ;

namespace TestingCenterSystem.Utils.RabbitMQ
{
    public class MQHelper
    {
        /// <summary>
        /// 发送消息
        /// </summary>
        public static void Publish(BaseMessage msg)
        {
            //// 创建消息bus
            IBus bus = BusBuilder.CreateMessageBus();

            try
            {
                bus.Publish(msg, x => x.WithTopic(msg.MessageRouter));
            }
            catch (EasyNetQException ex)
            {
                throw ex;
            }

            bus.Dispose();//与数据库connection类似，使用后记得销毁bus对象
        }

        /// <summary>
        /// 接收消息
        /// </summary>
        /// <param name="msg"></param>
        public static void Subscribe(BaseMessage msg, IProcessMessage ipro)
        {
            //// 创建消息bus
            IBus bus = BusBuilder.CreateMessageBus();

            try
            {
                bus.Subscribe<BaseMessage>(msg.MessageRouter, message => ipro.ProcessMsg(message), x => x.WithTopic(msg.MessageRouter));
            }
            catch (EasyNetQException ex)
            {
                throw ex;
            }
        }
     
    }
}
