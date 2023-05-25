namespace FlyMeToTheMoon
{
    public class MenuItem : DrawObject
    {
        protected string ItemName;
        protected string ItemAttr;

        public void SetItemName(string value)
        {
            ItemName = value;
        }

        public string GetItemName()
        {
            return ItemName;
        }
        
        public void SetItemAttr(string value)
        {
            ItemAttr = value;
        }

        public string GetItemAttr()
        {
            return ItemAttr;
        }
    }
}