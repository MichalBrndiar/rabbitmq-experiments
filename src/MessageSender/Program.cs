namespace MessageSender
{
    #region Using directives

    using System;

    using Common;

    using RabbitMQ.Client;

    #endregion

    internal class Program
    {
        private static void Main(string[] args)
        {
            var factory = new ConnectionFactory { HostName = "localhost" };
            using var connection = factory.CreateConnection();
            using var channel = connection.CreateModel();
            channel.ConfirmSelect(); // Enables 'publisher confirms'

            // 'Publisher confirms' events
            channel.BasicAcks += (sender, eventArgs) =>
                Console.WriteLine($">>> Message accepted by broker: {eventArgs.ToJson()}");
            channel.BasicNacks += (sender, eventArgs) => Console.WriteLine(">>> Message was NOT accepted by broker");

            channel.QueueDeclareCustom();

            Console.Write("Enter how many times should be message sent to broker [5 - default]: ");
            var timesString = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(timesString))
            {
                timesString = "5";
            }

            var times = int.Parse(timesString);

            Console.Write("Enter message priority [0 - default]: ");
            var priorityString = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(priorityString))
            {
                priorityString = "0";
            }

            var priority = int.Parse(priorityString);

            Console.WriteLine("Enter message and press <ENTER> to send, empty message to exit.");
            Console.WriteLine("Prefix message with 'rq-' to reject it by consumer and requeue");
            Console.WriteLine("Prefix message with 'rj-' to reject it by consumer and throw away");

            while (true)
            {
                var content = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(content))
                {
                    break;
                }

                // Create message...
                var msg = new SimpleMessage { Content = content };

                var props = channel.CreateBasicProperties();
                props.Persistent = true; // Message is marked as persistable, so broker can persist it
                props.Priority   = (byte)priority; // Modify priority of message

                for (var i = 0; i < times; i++)
                {
                    // ...and send it to default Exchange with routing key equal to our Queue name.
                    // Our Queue is bound to default Exchange using this routing key.
                    channel.BasicPublish(
                        string.Empty,
                        Common.Constants.QueueName,
                        body: msg.ToJsonBytes(),
                        basicProperties: props);
                }

                Console.WriteLine($">>> Message(s) sent: '{content}'");
            }
        }
    }
}