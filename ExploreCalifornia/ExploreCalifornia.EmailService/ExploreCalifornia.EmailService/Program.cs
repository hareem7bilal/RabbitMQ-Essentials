using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

var factory = new ConnectionFactory();
factory.Uri = new Uri("amqp://emailservice:Lumpaxix777@localhost:5672");
var connection = factory.CreateConnection();
var channel = connection.CreateModel();

// declare resources here, handle consumed events, etc
channel.QueueDeclare("emailServiceQueue", true, false, false);

var headers = new Dictionary<string, object>
            {
                {"subject", "tour"},
                {"action", "booked"},
                {"x-match", "all"}
            };

//channel.QueueBind("emailServiceQueue", "webappExchange", "tour.booked");
channel.QueueBind("emailServiceQueue", "webappExchange", "", headers);

var consumer = new EventingBasicConsumer(channel);
consumer.Received += (sender, eventArgs) =>
{
    var body = eventArgs.Body.ToArray(); // Convert ReadOnlyMemory<byte> to byte[]
    var message = Encoding.UTF8.GetString(body);
    var subject = Encoding.UTF8.GetString(eventArgs.BasicProperties.Headers["subject"] as byte[] ?? Array.Empty<byte>());
    var action = Encoding.UTF8.GetString(eventArgs.BasicProperties.Headers["action"] as byte[] ?? Array.Empty<byte>());
    //Console.WriteLine($"{eventArgs.RoutingKey} : {message}");
    Console.WriteLine($"{eventArgs.BasicProperties.UserId} -> {subject} {action} : {message}");
};

channel.BasicConsume("emailServiceQueue", true, consumer);
Console.ReadLine();

channel.Close();
connection.Close();

