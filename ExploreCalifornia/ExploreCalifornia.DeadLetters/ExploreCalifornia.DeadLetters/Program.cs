using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

var factory = new ConnectionFactory();
factory.Uri = new Uri("amqp://guest:guest@localhost:5672");
var connection = factory.CreateConnection();
var channel = connection.CreateModel();

channel.ExchangeDeclare("DLX", ExchangeType.Direct, true);
channel.QueueDeclare("deadLetters", true, false, false);

channel.QueueBind("deadLetters", "DLX", "");

var consumer = new EventingBasicConsumer(channel);
consumer.Received += (sender, eventArgs) =>
{
    var body = eventArgs.Body.ToArray(); // Convert ReadOnlyMemory<byte> to byte[]
    var message = Encoding.UTF8.GetString(body);
    var deathReason = Encoding.UTF8.GetString(eventArgs.BasicProperties.Headers["x-first-death-reason"] as byte[] ?? Array.Empty<byte>());
    Console.WriteLine($"Dead Letter -> {message}, Reason -> {deathReason}");
};

channel.BasicConsume("deadLetters", true, consumer);
Console.ReadLine();

channel.Close();
connection.Close();


