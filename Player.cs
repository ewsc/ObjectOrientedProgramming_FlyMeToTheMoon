﻿namespace FlyMeToTheMoon
{
    public class Player : DrawObject
    {
        protected int HighScore;
        protected int SmHighScore;
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
            SmHighScore += incValue;
            if (SmHighScore > 50) {
                HighScore += 10;
                SmHighScore = 0;
            }
        }
    }
}