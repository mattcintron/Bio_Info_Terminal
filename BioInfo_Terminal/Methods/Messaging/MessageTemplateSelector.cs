using System.Windows;
using System.Windows.Controls;

namespace BioInfo_Terminal.Methods.Messaging
{
    public class MessageTemplateSelector : DataTemplateSelector
    {
        //message structure
        // Template A: Computer under user
        // Template B: User under Computer
        // Template C: User under User
        // Template D: Computer under Computer

        public DataTemplate A { get; set; }
        public DataTemplate B { get; set; }
        public DataTemplate C { get; set; }
        public DataTemplate D { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            var message = item as Message;

            if (message != null && message.Side == MessageSide.UserSide)
                return message.PrevSide == MessageSide.BioInfoSide ? A : D;
            return message != null && message.PrevSide == MessageSide.BioInfoSide ? C : B;
        }
    }
}