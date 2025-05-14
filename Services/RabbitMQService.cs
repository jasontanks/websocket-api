using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace WebSocketRabbitMQ.Services
{
    public class RabbitMQService
    {
        private IConnection? _connection;
        private IChannel? _channel;
        private readonly string _queueName = "testc#";
        private static List<Func<string, Task>> _subscribers = new();

        public async Task InitializeAsync()
        {
            var factory = new ConnectionFactory
            {
                Uri = new Uri("amqps://czrthnkb:iW-ql_NhjUsKJmW06d-9x17WDa6YpThH@kebnekaise.lmq.cloudamqp.com/czrthnkbf"),
                UserName = "czrthnkb",
                Password = "iW-ql_NhjUsKJmW06d-9x17WDa6YpThH",
                VirtualHost = "czrthnkb",
            };

            _connection = await factory.CreateConnectionAsync();
            _channel = await _connection.CreateChannelAsync();
            await _channel.QueueDeclareAsync(_queueName, durable: true, exclusive: false, autoDelete: false, arguments: null);
        }

        public void Subscribe(Func<string, Task> callback)
        {
            _subscribers.Add(callback);
        }

        public async Task ConsumeMessagesAsync()
        {
            var consumer = new AsyncEventingBasicConsumer(_channel!);
            consumer.ReceivedAsync += async (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                Console.WriteLine($"📩 Received from RabbitMQ: {message}");

                foreach (var subscriber in _subscribers)
                {
                    await subscriber(message);
                }

                await _channel!.BasicAckAsync(ea.DeliveryTag, false);
            };

            await _channel!.BasicConsumeAsync(queue: _queueName, autoAck: false, consumer: consumer);
            Console.WriteLine("🔄 Waiting for RabbitMQ messages...");
        }
    }
}