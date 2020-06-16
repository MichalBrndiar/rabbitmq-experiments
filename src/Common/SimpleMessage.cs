namespace Common
{
    /// <summary>
    ///     Represents simple message to be put to RabbitMq queues.
    /// </summary>
    public class SimpleMessage
    {
        /// <summary>
        ///     Content of message.
        /// </summary>
        public string Content { get; set; }
    }
}