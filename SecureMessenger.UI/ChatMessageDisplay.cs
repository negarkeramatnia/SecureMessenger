using SecureMessenger.Core.Models;

namespace SecureMessenger.UI
{
    public class ChatMessageDisplay
    {
        public SecureMessenger.Core.Models.Message OriginalMessage { get; }

        public string DisplayText { get; }

        public ChatMessageDisplay(SecureMessenger.Core.Models.Message message, string displayText)
        {
            OriginalMessage = message;
            DisplayText = displayText;
        }

        public override string ToString() => DisplayText;
    }
}