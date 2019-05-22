using System;
using System.Configuration;
using EasyNetQ;
using Elight.Infrastructure;

namespace TestingCenterSystem.Utils.RabbitMQ
{
    /// <summary>
    /// ��Ϣ������������
    /// </summary>
    public class BusBuilder
    {

        public static IBus CreateMessageBus()
        {
            string MQHost = Configs.GetValue("MQHost");
            string MQPort = Configs.GetValue("MQPort");
            string MQVirtualHost = Configs.GetValue("MQVirtualHost");
            string MQUserName = Configs.GetValue("MQUserName");
            string MQPassword = Configs.GetValue("MQPassword");
            // ��Ϣ�����������ַ���
            string connString = $"host={MQHost}:{MQPort};virtualHost={MQVirtualHost};username={MQUserName};password={MQPassword}";

            if (connString == null || connString == string.Empty)
            {
                throw new Exception("messageserver connection string is missing or empty");
            }

            return RabbitHutch.CreateBus(connString);
        }


    }

}