using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Jupiter.Api
{
    public class ConsumerEmulator : BackgroundService
    {
        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var factory = new ConnectionFactory
                {UserName = "jupiter", Password = "jupiter-pwd", HostName = "localhost"};
            var connection = factory.CreateConnection();
            var channel = connection.CreateModel();
            channel.QueueDeclare(queue: "queue", durable: false, exclusive: false, autoDelete: false, arguments: null);
            stoppingToken.ThrowIfCancellationRequested();
            var consumer = new EventingBasicConsumer(channel);
            consumer.Received += (ch, ea) =>
            {
                Debug.WriteLine($"***************************{Encoding.UTF8.GetString(ea.Body.ToArray())}");
                channel.BasicAck(ea.DeliveryTag, false);
            };

            channel.BasicConsume("queue", false, consumer);

            return Task.CompletedTask;
        }
    }
}