using System;
using System.Collections.Generic;
using System.Drawing;

namespace FlyMeToTheMoon
{
    public class Spawner
    {
        public void SpawnNewAsteroids(ref List<Asteroid> asteroids, int asteroidsAmount, int asteroidMoveSpeedMin, int asteroidMoveSpeedMax, int maxAsteroidRow, int width)
        {
            var rand = new Random();
            var rowSize = rand.Next(1, maxAsteroidRow);
            List<Point> asteroidSpawnPoints = new List<Point>();
            for (var i = 0; i < rowSize; i++)
            {
                var position = new Point()
                {
                    X = rand.Next(10, width - 60),
                    Y = rand.Next(20, 100),
                };
                asteroidSpawnPoints.Add(position);
            }
            var count = 0;
            var index = 0;
            do
            {
                if (!asteroids[index].GetDrawingStatus() && !asteroids[index].GetExplosionStatus())
                {
                    asteroids[index].SetDrawingStatus(true);
                    asteroids[index].SetMoveSpeed(rand.Next(asteroidMoveSpeedMin, asteroidMoveSpeedMax));
                    asteroids[index].SetPosition(asteroidSpawnPoints[count].X, asteroidSpawnPoints[count].Y);
                    count++;
                }
                index++;
            } while (count < rowSize && index < asteroidsAmount);
        }
        
        public void FireRocket(ref Player rocket, ref List<Bullet> bullets, int bulletsAmount)
        {
            for (var i = 0; i < bulletsAmount; i++)
            {
                if (bullets[i].GetDrawingStatus()) continue;
                bullets[i].SetDrawingStatus(true);
                bullets[i].SetPosition(rocket.GetX() + rocket.GetWidth() / 2 - bullets[i].GetWidth() / 2, rocket.GetY() - 23);
                break;
            }    
        }
    }
}