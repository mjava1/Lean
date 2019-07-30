using RabbitMQ.Client;
using System;

namespace QuantConnect.Brokerages.Bitmex
{
    public static class RabbitMqHelper
    {
        private static string _hostName;
        private static string _userName;
        private static string _password;
        private static int _port;

        static RabbitMqHelper()
        {
            _hostName = "localhost";
            _port = 5672;           
            _userName = "guest";
            _password = "guest";
        }

        public static IConnection GetConnection()
        {
            var connectionFactory = GetConnectionFactory();
            return connectionFactory.CreateConnection();
        }

        private static ConnectionFactory GetConnectionFactory()
        {
            var connectionFactory = new ConnectionFactory
            {
                HostName = _hostName,
                UserName = _userName,
                Password = _password,
                Port = _port
            };

            return connectionFactory;
        }

        public static IConnection GetRobustConnection()
        {            
            var connectionFactory = GetConnectionFactory();           
            connectionFactory.AutomaticRecoveryEnabled = true;
            connectionFactory.NetworkRecoveryInterval = TimeSpan.FromSeconds(5);
            connectionFactory.RequestedHeartbeat = 5;
            connectionFactory.TopologyRecoveryEnabled = true;
            connectionFactory.UseBackgroundThreadsForIO = false;

            return connectionFactory.CreateConnection();
        }
    }
}
