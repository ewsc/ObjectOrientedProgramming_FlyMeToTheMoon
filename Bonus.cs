namespace FlyMeToTheMoon
{
    public class Bonus : DrawObject
    {
        protected int BonusType;

        public int GetBonusType()
        {
            return BonusType;
        }

        public void SetBonusType(int value)
        {
            BonusType = value;
        }
    }
}