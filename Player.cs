namespace FlyMeToTheMoon
{
    public class Player : DrawObject
    {
        protected int HighScore;
        protected int SmHighScore;
        protected int Health;
        protected int UsedBullets;
        protected int Hits;
        public bool IsMovingLeft;
        public bool IsMovingRight;
        protected int Difficulty;

        public int GetDifficulty()
        {
            return Difficulty;
        }

        public void SetDifficulty(int value)
        {
            Difficulty = value;
        }
        
        public void SetUsedBullets(int value)
        {
            UsedBullets = value;
        }
        
        public void IncUsedBullets(int value)
        {
            UsedBullets += value;
        }

        public int GetUsedBullets()
        {
            return UsedBullets;
        }
        
        public void SetHits(int value)
        {
            Hits = value;
        }
        
        public void IncHits(int value)
        {
            Hits += value;
        }

        public int GetHits()
        {
            return Hits;
        }

        public void SetHighScore(int value)
        {
            HighScore = value;
        }

        public void SetHeath(int value)
        {
            Health = value;
        }

        public void DecHealth(int value)
        {
            Health -= value;
        }

        public int GetHealth()
        {
            return Health;
        }
        
        public int GetHighScore()
        {
            return HighScore;
        }

        public void IncHighScore(int incValue)
        {
            SmHighScore += incValue;
            if (SmHighScore > 50) {
                HighScore += 10;
                SmHighScore = 0;
            }
        }
    }
}