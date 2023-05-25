namespace FlyMeToTheMoon
{
    public class MenuItem : DrawObject
    {
        protected string ItemName;

        public void SetItemName(string value)
        {
            ItemName = value;
        }

        private static void ExecuteExit()
        {
            System.Windows.Forms.Application.ExitThread();    
        }

        public void ItemExecution()
        {
            if (ItemName == "Exit")
            {
                ExecuteExit();
            }
        }

        public string GetItemName()
        {
            return ItemName;
        }
    }
}