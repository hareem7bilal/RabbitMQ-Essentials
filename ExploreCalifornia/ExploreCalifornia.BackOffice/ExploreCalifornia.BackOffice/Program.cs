using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

var factory = new ConnectionFactory();
factory.Uri = new Uri("amqp://backoffice:Lumpaxix777@localhost:5672");
var connection = factory.CreateConnection();
var channel = connection.CreateModel();

var arguments = new Dictionary<string, object>
            {
                {"x-dead-letter-exchange", "DLX"},
            };

// declare resources here, handle consumed events, etc
channel.QueueDeclare("backOfficeQueue", true, false, false, arguments);

var headers = new Dictionary<string, object>
            {
                {"subject", "tour"},
                {"action", "booked"},
                {"x-match", "any"}
            };

//channel.QueueBind("backOfficeQueue", "webappExchange", "tour.*");
channel.QueueBind("backOfficeQueue", "webappExchange", "", headers);
var consumer = new EventingBasicConsumer(channel);

consumer.Received += (sender, eventArgs) =>
{
    var body = eventArgs.Body.ToArray(); // Convert ReadOnlyMemory<byte> to byte[]
    var message = Encoding.UTF8.GetString(body);
    var subject = Encoding.UTF8.GetString(eventArgs.BasicProperties.Headers["subject"] as byte[] ?? Array.Empty<byte>());
    var action = Encoding.UTF8.GetString(eventArgs.BasicProperties.Headers["action"] as byte[] ?? Array.Empty<byte>());
    //Console.WriteLine($"{eventArgs.RoutingKey} : {message}");
    Console.WriteLine($"{eventArgs.BasicProperties.UserId} -> {subject} {action} : {message}");
    //channel.BasicReject(eventArgs.DeliveryTag, false);
};

channel.BasicConsume("backOfficeQueue", true, consumer);
//channel.BasicConsume("backOfficeQueue", false, consumer);
Console.ReadLine();

channel.Close();
connection.Close();



