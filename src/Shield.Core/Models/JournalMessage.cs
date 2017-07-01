namespace Shield.Core.Models
{
    public class JournalMessage : Journal
    {
        public string Message { get; set; }

        public JournalMessage() { }

        public JournalMessage(string message)
        {
            Message = message;
        }
    }
}
