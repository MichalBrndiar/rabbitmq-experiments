namespace MessageReceiver
{
    #region Using directives

    using System;
    using System.Collections.Generic;
    using System.Security.Cryptography.X509Certificates;
    using System.Threading;
    using System.Threading.Tasks;

    using Common;

    using RabbitMQ.Client;
    using RabbitMQ.Client.Events;

    #endregion

    internal class Program
    {
        private static async Task Main(string[] args)
        {
            var factory = new ConnectionFactory { HostName = "localhost" };
            using var connection = factory.CreateConnection();
            using var channel = connection.CreateModel();

            // Do not deliver more than 1 message to consumer at once
            // Without this setting, all messages will be delivered and remain in 'Unacked' state until processed (and 'Acked').
            // This is unintentional, because other workers will not be able to process other messages (if joined later).
            channel.BasicQos(0, 1, false);

            // Declare queues
            channel.QueueDeclareCustom();

            var consumer = new EventingBasicConsumer(channel);

            // When message is received, this event is fired...
            consumer.Received += (model, ea) =>
                {
                    try
                    {
                        Console.WriteLine();
                        // Set optional timeout for processing of 1 message
                        var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));

                        var task = Task.Run(
                            () =>
                                {
                                    var body = ea.Body.ToArray();
                                    var message = body.FromJsonBytes<SimpleMessage>();
                                    Console.WriteLine($"[{DateTime.Now}] Received message (delivery tag '{ea.DeliveryTag}'): {message.Content}");

                                    // Simulate long message processing (1000ms)
                                    //Thread.Sleep(1000);

                                    // Reject messages containing 'a' in content and put then back to Queue
                                    // Following implementation is less than optimal (block current thread for 3s and put 'maligious' message back to queue)...
                                    // ...so it is here only for demonstration how to reject messages
                                    if (message.Content.StartsWith("rq-"))
                                    {
                                        Console.WriteLine("Rejecting and requeueing message...");

                                        // Check if headers of message contains custom x-count key
                                        int? count = 1;
                                        if (ea.BasicProperties.Headers != null && ea.BasicProperties.Headers.ContainsKey("x-count"))
                                        {
                                            // If it does, read it
                                            count = (int?)ea.BasicProperties?.Headers["x-count"];
                                        }

                                        // If count is more that 3, message was requeued 3 times, so it is probably unprocessable
                                        if (count > 3)
                                        {
                                            Console.WriteLine("Message was unsuccessfully processed 3 times, so rejecting it...");
                                            // Rejecting message
                                            channel.BasicReject(ea.DeliveryTag, false);
                                        }
                                        else
                                        {
                                            Thread.Sleep(2000);
                                            if (ea.BasicProperties.Headers == null)
                                            {
                                                ea.BasicProperties.Headers = new Dictionary<string, object>();
                                            }

                                            // Store attempt of processing to x-count custom header
                                            ea.BasicProperties.Headers["x-count"] = count + 1;

                                            Console.WriteLine($"Republishing message - {count}x attempt");

                                            // Republish message as new one, because of updated headers
                                            channel.BasicPublish(ea.Exchange, ea.RoutingKey, ea.BasicProperties, body);
                                            // Rejecting old (current) message
                                            channel.BasicReject(ea.DeliveryTag, false);
                                        }
                                    }
                                    else if (message.Content.StartsWith("rj-"))
                                    {
                                        Console.WriteLine("Rejecting message...");

                                        // Rejecting message WITHOUT requeing
                                        channel.BasicReject(ea.DeliveryTag, false);
                                    }
                                    else
                                    {
                                        // Acknowledge message (signal to RabbitMq, that message processing was completed successfully)
                                        channel.BasicAck(ea.DeliveryTag, false);
                                    }
                                },
                            cts.Token);
                        
                        // Wait for completion of task. It can timeout if CancellationTokenSource decides to (as specified when constructing).
                        task.Wait(cts.Token);
                    }
                    catch (Exception ex)
                    {
                        // There was error (or timeout) while processing message, so reject it without requeuing
                        Console.WriteLine("Rejecting message (probably after timeout)...");
                        channel.BasicReject(ea.DeliveryTag, false);
                    }
                };

            // Gets count of messages in queue
            var messageCount = GetMessageCountInQueue(channel);
            Console.WriteLine($"There are {messageCount} message(s) in queue");

            // Start consuming messages from Queue
            channel.BasicConsume(
                Common.Constants.QueueName,
                false,
                consumer);

            Console.WriteLine("Waiting for messages, press <ENTER> to exit...");
            Console.ReadLine();
        }

        private static int GetMessageCountInQueue(IModel channel, string queueName = null)
        {
            var result = channel.QueueDeclareCustom(queueName);

            return (int)result.MessageCount;
        }
    }
}