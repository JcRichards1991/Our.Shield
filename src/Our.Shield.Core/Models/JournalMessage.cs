namespace Our.Shield.Core.Models
{
    /// <summary>
    /// Out-Of-The-Box Journal class to use
    /// </summary>
    public class JournalMessage : Journal
    {
        /// <summary>
        /// The message for the Journal
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Default constructure
        /// </summary>
        public JournalMessage() { }

        /// <summary>
        /// Initialises a JournalMessage Object with the desired message
        /// </summary>
        /// <param name="message">
        /// The message for the journal
        /// </param>
        public JournalMessage(string message)
        {
            Message = message;
        }
    }
}
