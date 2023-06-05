using System;

namespace FlyMeToTheMoon
{
   public class Event
   {
      protected string EventName;
      public DateTime EventCalled;

      public void SetEventName(string value)
      {
         EventName = value;
      }

      public void SetTime()
      {
         EventCalled = DateTime.Now;
      }
   }
}