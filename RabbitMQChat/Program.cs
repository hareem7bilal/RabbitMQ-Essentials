using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Reflection.PortableExecutable;
using System.Text;

var factory = new ConnectionFactory();
factory.Uri = new Uri("amqp://guest:guest@localhost:5672");
var connection = factory.CreateConnection();
var channel = connection.CreateModel();

Console.WriteLine("Please specify a chat room:");
var chatRoom = Console.ReadLine();


var exchangeName = "chat2";
var queueName = Guid.NewGuid().ToString();

channel.ExchangeDeclare(exchangeName, ExchangeType.Direct);
channel.QueueDeclare(queueName, true, true, true);
channel.QueueBind(queueName, exchangeName, chatRoom);

var consumer = new EventingBasicConsumer(channel);
consumer.Received += (sender, eventArgs) =>
{
    var body = eventArgs.Body.ToArray(); // Convert ReadOnlyMemory<byte> to byte[]
    var message = Encoding.UTF8.GetString(body);
    Console.WriteLine(message);
};

channel.BasicConsume(queueName, true, consumer);

var input = Console.ReadLine();
while(input != "")
{
    var bytes = Encoding.UTF8.GetBytes(input!);
    channel.BasicPublish(exchangeName, chatRoom, null, bytes);
    input = Console.ReadLine();
}

channel.Close();
connection.Close();