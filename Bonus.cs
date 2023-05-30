using System.Collections.Generic;

namespace FlyMeToTheMoon
{
    public class Bonus : DrawObject
    {
        protected int BonusType;
        protected string BonusName;

        public string GetBonusName(List<Bonus> bonuses, int BonusType)
        {
            return bonuses[BonusType].BonusName;
        }

        public void SetBonusType(int value)
        {
            BonusType = value;
        }

        public int GetBonusType()
        {
            return BonusType;
        }
    }
}