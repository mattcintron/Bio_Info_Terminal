using System;

namespace BioInfo_Terminal.Methods.Messaging
{
    public enum MessageSide
    {
        UserSide,
        BioInfoSide
    }

    public class Message
    {
        public Message()
        {
            Timestamp = DateTime.Now;
        }

        public string Text { get; set; }

        public DateTime Timestamp { get; set; }

        public MessageSide Side { get; set; }
        public MessageSide PrevSide { get; set; }
    }
}