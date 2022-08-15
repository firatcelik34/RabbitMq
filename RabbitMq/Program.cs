using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Text;

namespace RabbitMq
{
    class Program
    {
        static void Main(string[] args)
        {
            var connectionFactory = new ConnectionFactory()
            {
                UserName = "guest",
                Password = "guest",
                Port = 5672,
                VirtualHost = "/",
                HostName = "localhost"
            };
            var connection = connectionFactory.CreateConnection();
            var model = connection.CreateModel();
            var props = model.CreateBasicProperties();
            props.ContentType = "application/json";
            object message = new { Name = "Fırat", Surname = "Çelik" };
            var jsonMessage = JsonConvert.SerializeObject(message);
            model.BasicPublish("sms_queue", "sms.tomcat.xmlsms.info.preprocessor", props, Encoding.UTF8.GetBytes(jsonMessage));
            //Consume example
            var consumeModel = connection.CreateModel();
            consumeModel.BasicQos(0, 1, false);
            var consumer = new EventingBasicConsumer(consumeModel);
            consumer.Received += async (ch, ea) =>
            {
                try
                {
                    dynamic dynamic = JsonConvert.DeserializeObject(Encoding.UTF8.GetString(ea.Body.ToArray()));
                    Console.WriteLine(dynamic.Name);
                    object @object2 = JsonConvert.DeserializeObject(Encoding.UTF8.GetString(ea.Body.ToArray()));
                    Console.WriteLine(@object2);
                    //var ahmet = JsonConvert.DeserializeObject<Ahmet>(Encoding.UTF8.GetString(ea.Body.ToArray()));
                    //consumeModel.BasicAck(ea.DeliveryTag, false);
                    //Console.WriteLine(ahmet.Surname);
                }
                catch (Exception)
                {
                    consumeModel.BasicNack(ea.DeliveryTag, false, true);
                }
            };
            consumeModel.BasicConsume("tomcat_sms_preprocessor_queue", false, consumer);
        }
    }
}
