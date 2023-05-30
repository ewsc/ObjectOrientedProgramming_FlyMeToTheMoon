using System.Collections.Generic;
using System.Drawing;

namespace FlyMeToTheMoon
{
    public class Drawer
    {
        public Image DrawAsteroids(Image back, ref List<Asteroid> asteroids, int asteroidsAmount, string resources, int maxExplosionTime, int explosionTimer)
        {
            for (var i = 0; i < asteroidsAmount; i++)
            {
                if (asteroids[i].GetDrawingStatus() && !asteroids[i].GetExplosionStatus()) 
                {
                    var g = Graphics.FromImage(back);
                    var asteroid = Image.FromFile(resources + "asteroid64.png");
                    var point = new Point(asteroids[i].GetX(), asteroids[i].GetY());
                    g.DrawImage(asteroid, point);
                }

                if (!asteroids[i].GetExplosionStatus()) continue;
                {
                    if (asteroids[i].GetExplosionTimer() >= maxExplosionTime)
                    {
                        asteroids[i].SetExplosionStatus(false);
                        asteroids[i].SetDrawingStatus(false);
                    }
                    var g = Graphics.FromImage(back);
                    var explosion = Image.FromFile(resources + "explosion.png");
                    var point = new Point(asteroids[i].GetX(), asteroids[i].GetY());
                    g.DrawImage(explosion, point);
                    asteroids[i].IncExplosionTimer(explosionTimer);
                }
            }
            return back;
        }
        
        public Image DrawBullets(Image back, ref List<Bullet> bullets, int bulletsAmount, string resources)
        {
            for (var i = 0; i < bulletsAmount; i++)
            {
                if (!bullets[i].GetDrawingStatus()) continue;
                var g = Graphics.FromImage(back);
                var bullet = Image.FromFile(resources + "bullet.png");
                var point = new Point(bullets[i].GetX(), bullets[i].GetY());
                g.DrawImage(bullet, point);
            }
            return back;
        }
    }
}