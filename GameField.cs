using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace FlyMeToTheMoon
{
    public partial class GameField : Form
    {
        [DllImport("user32.dll")]
        private static extern short GetAsyncKeyState(int key);
        private const string Resources = "../../resources/";
        private const int MoveSize = 5;
        private const int BulletMoveSize = 6;
        private const int AsteroidMoveSize = 2;
        private const int BulletsAmount = 15;
        private const int AsteroidsAmount = 25;
        private const int FireRate = 6; // MIN=4
        private const int SpawnRate = 30;
        private const int MaxAsteroidRow = 3;
        private const int HealthDecRate = 20;

        private readonly List<Bullet> _bullets = new List<Bullet>();
        private readonly List<Asteroid> _asteroids = new List<Asteroid>();
        private readonly Player _rocket = new Player();
        private int _lastFired;
        private int _lastSpawned;
        public GameField()
        {
            InitializeComponent();
            _rocket.SetWidthHeight(26, 40);
            _rocket.SetPosition(Width / 2 + _rocket.GetWidth(), 300);
            _rocket.SetHighScore(0);
            _rocket.SetHeath(100);
            label2.Text = "HEALTH: " + _rocket.GetHealth();
            InitBullets();
            InitAsteroids();
        }
        
        private void InitAsteroids()
        {
            for (var i = 0; i < AsteroidsAmount; i++)
            {
                var tempAsteroid = new Asteroid();
                tempAsteroid.SetDrawingStatus(false);
                tempAsteroid.SetWidthHeight(32, 38);
                _asteroids.Add(tempAsteroid);
            }    
        }

        private void InitBullets()
        {
            for (var i = 0; i < BulletsAmount; i++)
            {
                var tempBullet = new Bullet();
                tempBullet.SetDrawingStatus(false);
                tempBullet.SetWidthHeight(16, 16);
                _bullets.Add(tempBullet);
            }    
        }

        private void SetHigh()
        {
            label1.Text = "SCORE: " + _rocket.GetHighScore();
        }

        private Image DrawBackground()
        {
            var back = Image.FromFile(Resources + "back.png");
            var actor = Image.FromFile(Resources + "rocket.png");
            var g = Graphics.FromImage(back);
            
            var p = new Point(_rocket.GetX(), _rocket.GetY()); 
            g.DrawImage(actor, p);
            var actorRect = new Rectangle(_rocket.GetX(), _rocket.GetY(), _rocket.GetWidth(), _rocket.GetHeight());
            var actorPen = new Pen(Brushes.DeepSkyBlue);
            actorPen.Width = 1.0F;
            g.DrawRectangle(actorPen, actorRect);

            for (var i = 0; i < BulletsAmount; i++)
            {
                if (_bullets[i].GetDrawingStatus()) 
                {
                    g = Graphics.FromImage(back);
                    var bullet = Image.FromFile(Resources + "bullet.png");
                    var point = new Point(_bullets[i].GetX(), _bullets[i].GetY());
                    g.DrawImage(bullet, point);
                }
            }
            
            for (var i = 0; i < AsteroidsAmount; i++)
            {
                if (_asteroids[i].GetDrawingStatus()) 
                {
                    g = Graphics.FromImage(back);
                    var asteroid = Image.FromFile(Resources + "asteroid32.png");
                    var point = new Point(_asteroids[i].GetX(), _asteroids[i].GetY());
                    g.DrawImage(asteroid, point);
                    
                    var asteroidRect = new Rectangle(_asteroids[i].GetX(), _asteroids[i].GetY(), _asteroids[i].GetWidth(), _asteroids[i].GetHeight());
                    var asteroidPed = new Pen(Brushes.Red);
                    asteroidPed.Width = 1.0F;
                    g.DrawRectangle(asteroidPed, asteroidRect);
                }
            }
            
            return back;
        }

        private void FireRocket()
        {
            for (var i = 0; i < BulletsAmount; i++)
            {
                if (!_bullets[i].GetDrawingStatus())
                {
                    _bullets[i].SetDrawingStatus(true);
                    _bullets[i].SetPosition(_rocket.GetX() + 11, _rocket.GetY() - 23);
                    break;
                }
            }    
        }

        private void GetMoves()
        {
            var rightKeyIsPressed = GetAsyncKeyState(Convert.ToInt32(Keys.D)) != 0;
            var leftKeyIsPressed = GetAsyncKeyState(Convert.ToInt32(Keys.A)) != 0;
            var spaceKeyIsPressed = GetAsyncKeyState(Convert.ToInt32(Keys.Space)) != 0;
            
            if (rightKeyIsPressed)
            {
                if (_rocket.CheckWorldBorders(Width + 152, MoveSize, true)) { 
                    _rocket.IncX(MoveSize);
                }         
            }
            else if (leftKeyIsPressed)
            {
                if (_rocket.CheckWorldBorders(Size.Width, MoveSize, false))
                {
                    _rocket.DecX(MoveSize);
                }    
            }
            if (spaceKeyIsPressed && _lastFired > FireRate)
            {
                _lastFired = 0;
                FireRocket();
            }
        }

        private void MoveBullets()
        {
            for (var i = 0; i < BulletsAmount; i++)
            {
                if (_bullets[i].GetY() < 10)
                {
                    _bullets[i].SetDrawingStatus(false);
                }
                if (_bullets[i].GetDrawingStatus())
                {
                    _bullets[i].DecY(BulletMoveSize);
                }
            } 
        }
        
        private void MoveAsteroids()
        {
            for (var i = 0; i < AsteroidsAmount; i++)
            {
                if (_asteroids[i].GetY() > 360)
                {
                    _asteroids[i].SetDrawingStatus(false);
                }
                if (_asteroids[i].GetDrawingStatus())
                {
                    _asteroids[i].MoveAsteroid(AsteroidMoveSize);
                }
            } 
        }

        private void SpawnNewAsteroids()
        {
            var rand = new Random();
            var rowSize = rand.Next(1, MaxAsteroidRow);
            List<int> asteroidSpawnPoints = new List<int>();
            for (var i = 0; i < rowSize; i++)
            {
                var random = new Random();
                var position = random.Next(10, Width + 150);
                asteroidSpawnPoints.Add(position);
            }
            var count = 0;
            var index = 0;
            do
            {
                if (!_asteroids[index].GetDrawingStatus())
                {
                    _asteroids[index].SetDrawingStatus(true);
                    _asteroids[index].SetPosition(asteroidSpawnPoints[count], 30);
                    count++;
                }
                index++;
            } while (count < rowSize && index < AsteroidsAmount);
        }
        
        public bool CheckCollision(Point l1, Point r1, Point l2, Point r2)
        {
            if (l1.X == r1.X || l1.Y == r1.Y || r2.X == l2.X || l2.Y == r2.Y)
            {
                return false;
            }
       
            if (l1.X > r2.X || l2.X > r1.X)
            {
                return false;
            }
 
            if (r1.Y > l2.Y || r2.Y > l1.Y)
            {
                return false;
            }
            
            return true;
        }
        
        private void CheckBulletCollisions()
        {
            for (var i = 0; i < BulletsAmount; i++)
            {
                if (_bullets[i].GetDrawingStatus())
                {
                    for (var j = 0; j < AsteroidsAmount; j++)
                    {
                        if (_asteroids[j].GetDrawingStatus())
                        {
                            var bulletRect = new Rectangle(_bullets[i].GetX(), _bullets[i].GetY(), _bullets[i].GetWidth(), _bullets[i].GetHeight());
                            var asteroidRect = new Rectangle(_asteroids[j].GetX(), _asteroids[j].GetY(), _asteroids[j].GetWidth(), _asteroids[j].GetHeight());
                    
                            if (bulletRect.IntersectsWith(asteroidRect))
                            {
                                _asteroids[j].SetDrawingStatus(false);
                                _bullets[i].SetDrawingStatus(false);
                            }      
                        }   
                    }
                }
            }
        }

        private void CheckRocketCollisions()
        {
            for (var i = 0; i < AsteroidsAmount; i++)
            {
                if (_asteroids[i].GetDrawingStatus())
                {
                    var rocketRect = new Rectangle(_rocket.GetX(), _rocket.GetY(), _rocket.GetWidth(), _rocket.GetHeight());
                    var asteroidRect = new Rectangle(_asteroids[i].GetX(), _asteroids[i].GetY(), _asteroids[i].GetWidth(), _asteroids[i].GetHeight());
                    
                    if (rocketRect.IntersectsWith(asteroidRect))
                    {
                        _asteroids[i].SetDrawingStatus(false);
                        _rocket.DecHealth(HealthDecRate);
                        label2.Text = "HEALTH: " + _rocket.GetHealth();
                    }    
                }
            }
        }
        
        private void Timer_Tick(object sender, EventArgs e)
        {
            if (_lastFired < FireRate + 1)
            {
                _lastFired++;
            }
            GetMoves();
            _rocket.IncHighScore(2);
            if (_lastSpawned > SpawnRate + 1)
            {
                SpawnNewAsteroids();
                _lastSpawned = 0;
            }
            _lastSpawned++;
            CheckRocketCollisions();
            CheckBulletCollisions();
            MoveBullets();
            MoveAsteroids();
            BackgroundImage = DrawBackground();
            SetHigh();
        }
    }
}