using System.Collections.Generic;
using System.Drawing;

namespace FlyMeToTheMoon
{
    public class Message : DrawObject
    {
        protected string MessageText;
        protected int LastShown;
        protected DrawObject Author;
        protected Brushes Color;

        public void SetColor(Brushes value)
        {
            Color = value;
        }

        public Brushes GetColor()
        {
            return Color;
        }

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