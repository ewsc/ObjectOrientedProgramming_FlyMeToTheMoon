using System;
using System.Collections.Generic;
using System.IO;

namespace FlyMeToTheMoon
{
    public class FileWriter
    {
        
        private const string SaveGameFile = "../../saves/savefile.fmtm";
        
        public void SaveGame(Player rocket, List<Asteroid> asteroids, List<Bullet> bullets)
        {
            try
            {
                if (File.Exists(SaveGameFile))    
                {    
                    File.Delete(SaveGameFile);    
                }
                using (var writer = File.CreateText(SaveGameFile))
                {
                    writer.WriteLine("+USER");
                    writer.WriteLine("score=" + rocket.GetHighScore());      
                    writer.WriteLine("bullets=" + rocket.GetUsedBullets());    
                    writer.WriteLine("hits=" + rocket.GetHits());    
                    writer.WriteLine("health=" + rocket.GetHealth());
                    writer.WriteLine("difficulty=" + rocket.GetDifficulty());
                    writer.WriteLine("posX=" + rocket.GetX());
                    writer.WriteLine("posY=" + rocket.GetY());
                    writer.WriteLine("-USER");
                    
                    writer.WriteLine("+ASTEROIDS");
                    foreach (var asteroid in asteroids)
                    {
                        if (!asteroid.GetDrawingStatus()) continue;
                        writer.WriteLine("posX=" + asteroid.GetX());
                        writer.WriteLine("posY=" + asteroid.GetY());
                        writer.WriteLine("speed=" + asteroid.GetMoveSpeed());
                        writer.WriteLine("expTimer=" + asteroid.GetExplosionTimer());
                        writer.WriteLine("exp=" + asteroid.GetExplosionStatus());
                        writer.WriteLine("[");
                    }
                    writer.WriteLine("-ASTEROIDS");
                    
                    writer.WriteLine("+BULLETS");
                    foreach (var bullet in bullets)
                    {
                        if (!bullet.GetDrawingStatus()) continue;
                        writer.WriteLine("posX=" + bullet.GetX());
                        writer.WriteLine("posY=" + bullet.GetY());
                        writer.WriteLine(">");
                    }
                    writer.WriteLine("-BULLETS");
                } 
            }
            catch (Exception)
            {
                // ignored
            }
        }
    }
}