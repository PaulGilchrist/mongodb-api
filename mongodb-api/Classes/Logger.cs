using System;
using System.Text;
using MongoDbApi.Models;
using RabbitMQ.Client;

namespace MongoDbApi.Classes {
    public class Logger {
        private readonly ApplicationSettings _applicationSettings;
        private ConnectionFactory _factory = null;

        public Logger(ApplicationSettings applicationSettings) {
            _applicationSettings = applicationSettings;
            Init();
        }

        private bool Init() {
            var success = (_factory != null);
            if(!success) {
                try {
                    _factory = new ConnectionFactory();
                    //_factory.UserName = _applicationSettings.QueueUser;
                    //_factory.Password = _applicationSettings.QueuePassword;
                    //_factory.VirtualHost = _applicationSettings.QueueVHost;
                    _factory.HostName = _applicationSettings.QueueHostName;
                    //_factory.Port = _applicationSettings.QueuePort;
                    success = true;
                } catch(Exception ex) {
                    Console.WriteLine(ex);
                }
            }
            return success;
        }

        public void Send(string message) {
            if(Init()) {
                try {
                    using(var connection = _factory.CreateConnection()) {
                        using(var channel = connection.CreateModel()) {
                            channel.QueueDeclare(queue: "api",
                                                 durable: false,
                                                 exclusive: false,
                                                 autoDelete: false,
                                                 arguments: null);
                            var body = Encoding.UTF8.GetBytes(message);
                            channel.BasicPublish(exchange: "",
                                                 routingKey: "api",
                                                 basicProperties: null,
                                                 body: body);
                       }
                    }
                } catch (Exception ex) {
                    Console.WriteLine(ex);
                }
                Console.WriteLine(" [x] Sent {0}",message);
            }
        }
    }
}
