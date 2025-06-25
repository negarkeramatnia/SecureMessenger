using SecureMessenger.Core.Models;

namespace SecureMessenger.UI
{
    public class ChatMessageDisplay
    {
        // We specify the full name here to avoid confusion
        public SecureMessenger.Core.Models.Message OriginalMessage { get; }

        public string DisplayText { get; }

        // And we specify the full name here as well
        public ChatMessageDisplay(SecureMessenger.Core.Models.Message message, string displayText)
        {
            OriginalMessage = message;
            DisplayText = displayText;
        }

        public override string ToString() => DisplayText;
    }
}