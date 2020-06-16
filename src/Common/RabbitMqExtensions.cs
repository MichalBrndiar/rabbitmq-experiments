namespace Common
{
    #region Using directives

    using System.Collections.Generic;

    using RabbitMQ.Client;

    #endregion

    /// <summary>
    ///     Contains extension methods for RabbitMq.Client API.
    /// </summary>
    public static class RabbitMqExtensions
    {
        /// <summary>
        ///     Creates RabbitMq queue according to <see cref="Constants" />.
        /// </summary>
        public static QueueDeclareOk QueueDeclareCustom(this IModel channel, string queueName = null)
        {
            queueName ??= Constants.QueueName;

            // Create exchange for dead letter queue
            channel.ExchangeDeclare(Constants.DeadLetterExchange, "direct");

            channel.QueueDeclare(
                Constants.DeadLetterQueue,
                exclusive: false,
                autoDelete: false,
                durable: true);

            channel.QueueBind(Constants.DeadLetterQueue, Constants.DeadLetterExchange, string.Empty);

            var arguments = new Dictionary<string, object>
            {
                ["x-message-ttl"] = 1000 * 60 * 10, // TTL - TimeToLive, message remaining in queue longer then TTL (in ms) are discarded
                ["x-max-priority"]         = 255,   // Priority queue, priority on message can be set from 0 to 255
                ["x-dead-letter-exchange"] = Constants.DeadLetterQueue
            };

            // When declaring Queue without explicit Exchange and Binding, this Queue is bound to 'Default Exchange' - "" (name is empty string)
            return channel.QueueDeclare(
                queueName,
                exclusive: false,
                autoDelete: false,
                durable: true,
                arguments: arguments); // Queue is durable, so messages are persisted on retrieval to broker
        }
    }
}