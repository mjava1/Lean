using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.MessagePatterns;
using System.Threading;
using QuantConnect.Logging;

namespace QuantConnect.Brokerages.Bitmex
{
    public class BitemexTradesSubscribe
    {
        private IModel _channel;
        private IConnection _connection;
        private readonly IDictionary<string, Type> _messageHandlers;
        private string _queue;
        private Action _connectionErrorCallback;
        private string _exchange;

        public BitemexTradesSubscribe()
        {
            _exchange = "bitmex.trades";
            _queue = "trades_xbt";
        }

        public bool ConnectionIsOpen => _connection.IsOpen;

        public void SetupConnection(Action connectionErrorCallback = null)
        {
            _connectionErrorCallback = connectionErrorCallback;
            SetupConnection(_queue, RabbitMqHelper.GetRobustConnection());
        }

        public void SetupConnection(string queue, IConnection connection)
        {
            _queue = queue;
            //_log.Info("Connecting to RabbitMQ...");

            _connection = connection;
            
            _connection.ConnectionShutdown += ConnectionShutdown;
            _channel = _connection.CreateModel();

            SetupChannel(_channel);
            SetupQueue(queue);
            SetupBindings(queue);

            //_log.Info($"Connected to {_connection.Endpoint.HostName}:{_connection.Endpoint.Port}.");
        }

        private void ConnectionShutdown(object sender, ShutdownEventArgs e)
        {
            //_log.Error($"RabbitMQ ConnectionShutdown {_connection.Endpoint.HostName}:{_connection.Endpoint.Port}.");
            if (_connectionErrorCallback != null)
                _connectionErrorCallback.Invoke();
        }

        protected void SetupChannel(IModel channel)
        {
            channel.ExchangeDeclare(exchange: _exchange,
                durable: false,
                type: "fanout",
                autoDelete: true,
                arguments: null);
        }

        public void Subscribe(Action<Trade> handleTrade)
        {
            SetupConnection();

            var subscription = new Subscription(_channel, _queue, false);

            //cancellationToken.Register(() => HandleCancellationToken(subscription));

            foreach (BasicDeliverEventArgs e in subscription)
            {
                var messageJson = Encoding.UTF8.GetString(e.Body);
                try
                {
                    // Deserialise message
                    var trade = DeserializeMessage(messageJson, e);
                    handleTrade(trade);

                    // And finally acknowledge it
                    subscription.Ack(e);
                }
                catch (OperationCanceledException oe)
                {
                    Log.Error(oe);
                }
                catch (Exception ex)
                {
                    Log.Error(ex);
                }
            }
        }

        public bool ProcessMessage(object handler, object message, CancellationToken cancellationToken)
        {
            bool result;
            int attempts = 0;

            var method = handler.GetType().GetMethod("HandleMessage");
            do
            {
                cancellationToken.ThrowIfCancellationRequested();
                result = (bool)method.Invoke(handler, new[] { message });
                if (!result)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    Thread.Sleep(TimeSpan.FromSeconds(5));
                    attempts++;
                    //_log.Info($"Failed to process message, retrying. Count is {attempts}");
                }
            } while (!result && attempts < 3);

            return result;
        }

        private Trade DeserializeMessage(string messageJson, BasicDeliverEventArgs e)
        {        
            return JsonConvert.DeserializeObject<Trade>(messageJson);
        }

        private void SetupQueue(string queue)
        {
            _channel.QueueDeclare(queue, durable: true, exclusive: false, autoDelete: false, arguments: null);
        }

        private void SetupBindings(string queue)
        {
            _channel.QueueBind(queue: queue, exchange: _exchange, arguments: null, routingKey:"abc");

            //foreach (var messageHandler in _messageHandlers)
            //{
            //    var routingKey = $"{messageHandler.Key}";
            //    _channel.QueueBind(queue: queue, exchange: _exchange, routingKey: routingKey, arguments: null);
            //}
        }

        private void HandleCancellationToken(Subscription subscription)
        {
            //_log.Info("ContentBusSubscribe cancelling...");
            try
            {
                subscription.Close();
                _channel.Close();
                _connection.Close();
            }
            catch (Exception e)
            {
                //_log.Info("ContentBusSubscribe error...");
                //_log.Error(e);
            }

            //_log.Info("ContentBusSubscribe cancelled");
        }
    }
}