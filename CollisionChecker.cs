using System.Collections.Generic;
using System.Drawing;

namespace FlyMeToTheMoon
{
    public class CollisionChecker
    {
        public void CheckRocketCollisions(ref Player rocket, ref List<Asteroid> asteroids, int healthDecRate, int asteroidsAmount)
        {
            for (var i = 0; i < asteroidsAmount; i++)
            {
                if (asteroids[i].GetDrawingStatus())
                {
                    var rocketRect = new Rectangle(rocket.GetX(), rocket.GetY(), rocket.GetWidth(),
                        rocket.GetHeight());
                    var asteroidRect = new Rectangle(asteroids[i].GetX(), asteroids[i].GetY(),
                        asteroids[i].GetWidth(), asteroids[i].GetHeight());

                    if (rocketRect.IntersectsWith(asteroidRect))
                    {
                        asteroids[i].SetExplosionStatus(true);
                        asteroids[i].SetDrawingStatus(false);
                        asteroids[i].SetExplosionTimer(0);
                        rocket.DecHealth(healthDecRate);
                    }
                }
            }
        }
        
        public void CheckBulletCollisions(ref Player rocket, ref List<Asteroid> asteroids, ref List<Bullet> bullets, int bulletsAmount, int asteroidsAmount)
        {
            for (var i = 0; i < bulletsAmount; i++)
            {
                if (bullets[i].GetDrawingStatus())
                {
                    for (var j = 0; j < asteroidsAmount; j++)
                    {
                        if (asteroids[j].GetDrawingStatus())
                        {
                            var bulletRect = new Rectangle(bullets[i].GetX(), bullets[i].GetY(), bullets[i].GetWidth(), bullets[i].GetHeight());
                            var asteroidRect = new Rectangle(asteroids[j].GetX(), asteroids[j].GetY(), asteroids[j].GetWidth(), asteroids[j].GetHeight());
                    
                            if (bulletRect.IntersectsWith(asteroidRect))
                            {
                                asteroids[j].SetExplosionStatus(true);
                                asteroids[j].SetDrawingStatus(false);
                                asteroids[j].SetExplosionTimer(0);
                                
                                bullets[i].SetDrawingStatus(false);
                                rocket.IncHighScore(50);
                                rocket.IncHits(1);
                            }      
                        }   
                    }
                }
            }
        }
        
        public int CheckBonusCollision(ref Player rocket, ref List<Bonus> bonusList, int bonusesAmount)
        {
            for (var i = 0; i < bonusesAmount; i++)
            {
                if (bonusList[i].GetDrawingStatus())
                {
                    var rocketRect = new Rectangle(rocket.GetX(), rocket.GetY(), rocket.GetWidth(), rocket.GetHeight());
                    var bonusRect = new Rectangle(bonusList[i].GetX(), bonusList[i].GetY(),
                        bonusList[i].GetWidth(), bonusList[i].GetHeight());

                    if (rocketRect.IntersectsWith(bonusRect))
                    {
                        bonusList[i].SetDrawingStatus(false);
                        return bonusList[i].GetBonusType();
                    }
                }
            }
            return -1;
        }
    }
}