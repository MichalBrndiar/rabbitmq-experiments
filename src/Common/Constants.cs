namespace Common
{
    public static class Constants
    {
        /// <summary>
        ///     Name of RabbitMq queue.
        /// </summary>
        public static readonly string QueueName = "TestQueue";

        /// <summary>
        ///     Name of RabbitMq exchange for dead letter messages.
        /// </summary>
        public static readonly string DeadLetterExchange = "TestQueue-DeadLetterExchange";

        /// <summary>
        ///     The of RabbitMq queue for dead letter messages.
        /// </summary>
        public static readonly string DeadLetterQueue = "TestQueue-DeadLetterQueue";
    }
}