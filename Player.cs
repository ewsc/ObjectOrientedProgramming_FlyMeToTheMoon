namespace FlyMeToTheMoon
{
    public class Player : DrawObject
    {
        protected int HighScore;
        protected int Health;

        public void SetHighScore(int value)
        {
            HighScore = value;
        }
        
        public int GetHighScore()
        {
            return HighScore;
        }

        public void IncHighScore(int incValue)
        {
            HighScore += incValue;
        }
    }
}