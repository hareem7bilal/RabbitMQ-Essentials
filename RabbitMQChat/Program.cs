using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Reflection.PortableExecutable;
using System.Text;


Console.WriteLine("Please specify a username:");
var username = Console.ReadLine();
var password = username;

Console.WriteLine("Please specify a chat room:");
var chatRoom = Console.ReadLine();


var exchangeName = "chat2";

//create a unique queue name for this instance
var queueName = Guid.NewGuid().ToString();

//connect to RabbitMQ
var factory = new ConnectionFactory();
factory.Uri = new Uri($"amqp://{username}:{password}@localhost:5672");
var connection = factory.CreateConnection();
var channel = connection.CreateModel();

channel.ExchangeDeclare(exchangeName, ExchangeType.Direct);
channel.QueueDeclare(queueName, true, true, true);
channel.QueueBind(queueName, exchangeName, chatRoom);

var consumer = new EventingBasicConsumer(channel);
consumer.Received += (sender, eventArgs) =>
{
    var body = eventArgs.Body.ToArray(); // Convert ReadOnlyMemory<byte> to byte[]
    var message = Encoding.UTF8.GetString(body);
    var user = eventArgs.BasicProperties.UserId;
    Console.WriteLine(user+" : "+message);
};

channel.BasicConsume(queueName, true, consumer);

var input = Console.ReadLine();
while(input != "")
{
    var bytes = Encoding.UTF8.GetBytes(input!);
    var props = channel.CreateBasicProperties();
    props.UserId = username;
    channel.BasicPublish(exchangeName, chatRoom, props, bytes);
    input = Console.ReadLine();
}

channel.Close();
connection.Close();