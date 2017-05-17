using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using RabbitMQ.Client;
using RabbitMQ.Client.Content;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.MessagePatterns;
using RabbitMQ.Util;

using iSql.Commons;


namespace iSql.Util
{
    /// <summary>
    /// This is a wrapper class for general messages received from local RabbitMQ, and some simple processing may be applied. 
    /// 
    /// Utilizing message queue can help simply business logic flow and make app scalable, but it may involve certain complexity and pitfalls as well. In general, it may not be 
    /// very reliable to consume messages in web app, so please make sure errors are properly handled. 
    /// 
    /// In our case, the notification is not crutial flow steps, and we can play tricks on either the queue's TTL or the message property to make sure it won't stay there for ever 
    /// if not consumed.
    /// </summary>
    public class MessageQueueHub {

        public static bool BackgroundProcessingStarted;
        public static void KickOffMessageProcessing() { 
            
            //stick to tranditional thread model, to avoid too much impact to ThreadPool.
            ThreadStart threadStart = new ThreadStart(ReceiveDbaMessage);
            Thread thread = new Thread(threadStart);
            thread.Start();

            BackgroundProcessingStarted = true;
        }

        #region fields & properties
        protected static string _broadcastMessageToDba;
        
        public static string BroadcastMessageToDba
        {
            get { return _broadcastMessageToDba; }
            set
            {
                _broadcastMessageToDba = value;
                BroadcaseMessageUpdateTime = DateTime.Now;
            }
        }

        public static DateTime BroadcaseMessageUpdateTime
        {
            get;
            set;
        }

        #endregion 


        // infinite loop, make sure you handle this correctly in your code, use it either in interactive console or run at background.
        public static void ReceiveDbaMessage()
        {
            try
            {
                var factory = new ConnectionFactory() { HostName = Conf.MQ.HostName };
                using (var connection = factory.CreateConnection())
                {
                    using (var channel = connection.CreateModel())
                    {
                        channel.QueueDeclare( queue: Conf.MQ.DbaNotificationQueue ,durable: false, exclusive: false, autoDelete: false, arguments: null);

                        var consumer = new QueueingBasicConsumer(channel);
                        //NOTE: may consider ack in the future
                        channel.BasicConsume(queue: Conf.MQ.DbaNotificationQueue , noAck: true, consumer: consumer);

                        while (true)
                        {
                            var eventArgs = (BasicDeliverEventArgs)consumer.Queue.Dequeue();
                            var body = eventArgs.Body;
                            var message = Encoding.UTF8.GetString(body);
                            BroadcastMessageToDba = message;
                        }
                    }
                }
            } catch ( Exception ex ) { 
                //have to capture all exceptions since this will be executed in a background thread anyway
            }

        }


        public static void SendNewDbaValidationRequestMessage( string message) 
        {
            try
            {
                var factory = new ConnectionFactory() { HostName = Conf.MQ.HostName };
                using (var connection = factory.CreateConnection())
                {
                    using (var channel = connection.CreateModel()) {
                        channel.QueueDeclare( queue: Conf.MQ.DbaNewRequestQueue ,durable: false, exclusive: false, autoDelete: false, arguments: null);
                        var body = Encoding.UTF8.GetBytes(message);

                        // simple queuing, no exchange and routing key yet
                        channel.BasicPublish( exchange:"", routingKey:Conf.MQ.DbaNewRequestQueue, basicProperties: null, body:body);
                    }
                }
            } catch ( Exception ex ) { 
                //have to capture all exceptions since this may be executed in a background thread anyway
            }
        }


        public static void SendDbaDecisionMessage( string message) 
        {
            try
            {
                var factory = new ConnectionFactory() { HostName = Conf.MQ.HostName };
                using (var connection = factory.CreateConnection())
                {
                    using (var channel = connection.CreateModel()) {
                        channel.QueueDeclare( queue: Conf.MQ.DbaDecisionQueue ,durable: false, exclusive: false, autoDelete: false, arguments: null);
                        var body = Encoding.UTF8.GetBytes(message);

                        // simple queuing, no exchange and routing key yet
                        channel.BasicPublish( exchange:"", routingKey:Conf.MQ.DbaDecisionQueue, basicProperties: null, body:body);
                    }
                }
            } catch ( Exception ex ) { 
                //have to capture all exceptions since this may be executed in a background thread anyway
            }
        }

    }
}
