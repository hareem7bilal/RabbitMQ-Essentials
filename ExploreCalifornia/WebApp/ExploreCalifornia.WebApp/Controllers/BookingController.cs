using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RabbitMQ.Client;

namespace ExploreCalifornia.WebApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BookingController : ControllerBase
    {
        [HttpPost]
        [Route("Book")]
        public IActionResult Book()
        {
            var tourname = Request.Form["tourname"];
            var name = Request.Form["name"];
            var email= Request.Form["email"];
            var needsTransport = Request.Form["transport"] == "on";

            // Send messages here...
            var message = $"{tourname};{name};{email}";
            
            var headers = new Dictionary<string, object> {
                {"subject", "tour" },
                {"action", "booked" }
            };

            //SendMessage("tour.booked", message);
            SendMessageHeaders(headers, message);

            if (needsTransport)
            {
                var needsTransportHeaders = new Dictionary<string, object> {
                {"subject", "transport" },
                {"action", "booked" }
            };
                SendMessageHeaders(needsTransportHeaders, message);
            }

            return Redirect($"/BookingConfirmed?tourname={tourname}&name={name}&email={email}");
        }

        [HttpPost]
        [Route("Cancel")]
        public IActionResult Cancel()
        {
            var tourname = Request.Form["tourname"];
            var name = Request.Form["name"];
            var email = Request.Form["email"];
            var cancelReason = Request.Form["reason"];

            // Send cancel message here
            var message = $"{tourname};{name};{email};{cancelReason}";

            var headers = new Dictionary<string, object> {
                {"subject", "tour" },
                {"action", "canceled" }
            };

            //SendMessage("tour.canceled", message);
            SendMessageHeaders(headers, message);

            return Redirect($"/BookingCanceled?tourname={tourname}&name={name}");
        }

        private void SendMessage(string routingKey, string message)
        {
            var factory = new ConnectionFactory();
            factory.Uri = new Uri("amqp://guest:guest@localhost:5672");
            var connection = factory.CreateConnection();
            var channel = connection.CreateModel();

            var bytes = System.Text.Encoding.UTF8.GetBytes(message);
            channel.BasicPublish("webappExchange", routingKey, null, bytes);
            channel.Close();
            connection.Close();
        }

        private void SendMessageHeaders(IDictionary<string,object> headers, string message)
        {
            var factory = new ConnectionFactory();
            factory.Uri = new Uri("amqp://webapp:Lumpaxix777@localhost:5672");
            var connection = factory.CreateConnection();
            var channel = connection.CreateModel();

            var bytes = System.Text.Encoding.UTF8.GetBytes(message);
            var props = channel.CreateBasicProperties();
            props.Headers = headers;
            props.UserId = "webapp";

            channel.BasicPublish("webappExchange", "", props, bytes);
            channel.Close();
            connection.Close();
        }
    }
}