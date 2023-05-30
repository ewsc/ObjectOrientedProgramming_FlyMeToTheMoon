using System.Collections.Generic;

namespace FlyMeToTheMoon
{
    public class Message : DrawObject
    {
        protected string MessageText;
        protected int LastShown;
        protected DrawObject Author;

        public void SetMessage(string value)
        {
            MessageText = value;
        }

        public string GetMessage()
        {
            return MessageText;
        }
        
        public void AddMessage(string text, int duration, ref List <Message> gameNotifications)
        {
            SetWidthHeight(text.Length * 50, 100);
            SetMessage(text);
            SetDuration(duration);
            SetDrawingStatus(true);
            gameNotifications.Add(this);
        }

        public void SetDuration(int value)
        {
            LastShown = value;
        }

        public int GetDuration()
        {
            return LastShown;
        }

        public void DecDuration(int value)
        {
            LastShown -= value;
        }
    }
}