using SecureMessenger.Core.Models;

namespace SecureMessenger.UI
{
    public class ChatMessageDisplay
    {
        //contains all the important data like the message ID ... and encrypted content
        //when a user right-clicks a message, we know its ID to delete it
        public SecureMessenger.Core.Models.Message OriginalMessage { get; }
        //ex: "[6/28/2025 10:30 AM] You: Hello world"
        public string DisplayText { get; }
        //Constructor
        public ChatMessageDisplay(SecureMessenger.Core.Models.Message message, string displayText)
        {
            OriginalMessage = message;
            DisplayText = displayText;
        }
        //overriding the default behavior
        //Every object in C# has a default 'ToString()' method. The ListBox automatically calls this method
        public override string ToString() => DisplayText;
    }
}