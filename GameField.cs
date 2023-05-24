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
        
        private const int MoveSize = 9;
        private const int BulletMoveSize = 9;
        private const int AsteroidMoveSize = 3;
        private const int BulletsAmount = 20;
        private const int AsteroidsAmount = 25;
        private const int FireRate = 8; // MIN=4
        private const int SpawnRate = 20;
        private const int MaxAsteroidRow = 3;
        private const int HealthDecRate = 20;
        private const bool DrawHitboxes = true;
        private const int ExplosionTimer = 2;
        private const int MaxExplosionTime = 100;

        private readonly List<Bullet> _bullets = new List<Bullet>();
        private readonly List<Asteroid> _asteroids = new List<Asteroid>();
        private readonly Player _rocket = new Player();
        private int _lastFired;
        private int _lastSpawned;
        private static System.Timers.Timer _aTimer;
        private bool _canMove;
        
        public GameField()
        {
            FormBorderStyle = FormBorderStyle.None;
            WindowState = FormWindowState.Maximized;
            InitializeComponent();
            
            Width = Screen.PrimaryScreen.Bounds.Width;
            Height = Screen.PrimaryScreen.Bounds.Height;
            
            PlaceLabels();

            _aTimer = new System.Timers.Timer(30);
            _aTimer.Elapsed += OnTimedEvent;
            _aTimer.AutoReset = true;
            
            StartGame();
            
            label1.Text = @"SCORE: " + _rocket.GetHighScore();
            label2.Text = @"HEALTH: " + _rocket.GetHealth();
            label3.Text = @"BULLETS: " + _rocket.GetUsedBullets();
            label4.Text = @"HITS: " + _rocket.GetHits();
            var currentDateTime = DateTime.Now;
            label5.Text = @"TIME: " + currentDateTime;
            
        }

        private void StartGame()
        {
            _canMove = true;
            _rocket.SetWidthHeight(56, 128);
            _rocket.SetPosition((Width / 2) - _rocket.GetWidth(), 2 * (Height / 3));
            _rocket.SetHighScore(0);
            _rocket.SetHeath(100);
            _rocket.SetUsedBullets(0);
            _rocket.SetHits(0);
            _rocket.IsMovingLeft = false;
            _rocket.IsMovingRight = false;
            InitBullets();
            InitAsteroids();
            _aTimer.Enabled = true;
        }

        private void PlaceLabels()
        {
            var size = Width / 5;
            
            label1.Left = size * 0;
            label2.Left = size * 1;
            label3.Left = size * 2;
            label4.Left = size * 3;
            label5.Left = size * 4;
        }

        private void InitAsteroids()
        {
            _asteroids.Clear();
            for (var i = 0; i < AsteroidsAmount; i++)
            {
                var tempAsteroid = new Asteroid();
                tempAsteroid.SetDrawingStatus(false);
                tempAsteroid.SetWidthHeight(64, 64);
                tempAsteroid.SetExplosionStatus(false);
                _asteroids.Add(tempAsteroid);
            }    
        }

        private void InitBullets()
        {
            _bullets.Clear();
            for (var i = 0; i < BulletsAmount; i++)
            {
                var tempBullet = new Bullet();
                tempBullet.SetDrawingStatus(false);
                tempBullet.SetWidthHeight(10, 32);
                _bullets.Add(tempBullet);
            }    
        }

        private Image DrawAsteroids(Image back)
        {
            for (var i = 0; i < AsteroidsAmount; i++)
            {
                if (_asteroids[i].GetDrawingStatus() && !_asteroids[i].GetExplosionStatus()) 
                {
                    var g = Graphics.FromImage(back);
                    var asteroid = Image.FromFile(Resources + "asteroid64.png");
                    var point = new Point(_asteroids[i].GetX(), _asteroids[i].GetY());
                    g.DrawImage(asteroid, point);

                    if (DrawHitboxes)
                    {
                        var asteroidRect = new Rectangle(_asteroids[i].GetX(), _asteroids[i].GetY(),
                            _asteroids[i].GetWidth(), _asteroids[i].GetHeight());
                        var asteroidPed = new Pen(Brushes.Red);
                        asteroidPed.Width = 1.0F;
                        g.DrawRectangle(asteroidPed, asteroidRect);
                    }
                }

                if (_asteroids[i].GetExplosionStatus())
                {
                    if (_asteroids[i].GetExplosionTimer() >= MaxExplosionTime)
                    {
                        _asteroids[i].SetExplosionStatus(false);
                        _asteroids[i].SetDrawingStatus(false);
                    }
                    var g = Graphics.FromImage(back);
                    var explosion = Image.FromFile(Resources + "explosion.png");
                    var point = new Point(_asteroids[i].GetX(), _asteroids[i].GetY());
                    g.DrawImage(explosion, point);
                    _asteroids[i].IncExplosionTimer(ExplosionTimer);
                }
            }
            return back;
        }

        private Image DrawBullets(Image back)
        {
            for (var i = 0; i < BulletsAmount; i++)
            {
                if (!_bullets[i].GetDrawingStatus()) continue;
                var g = Graphics.FromImage(back);
                var bullet = Image.FromFile(Resources + "bullet.png");
                var point = new Point(_bullets[i].GetX(), _bullets[i].GetY());
                g.DrawImage(bullet, point);
            }
            return back;
        }

        private Image DrawBackground()
        {
            var back = Image.FromFile(Resources + "back.jpg");
            Image actor;
            if (_rocket.IsMovingRight && !_rocket.IsMovingLeft)
            {
                actor = Image.FromFile(Resources + "rocket_right.png");    
            }
            else if (_rocket.IsMovingLeft && !_rocket.IsMovingRight)
            {
                actor = Image.FromFile(Resources + "rocket_left.png");        
            }
            else
            {
                actor = Image.FromFile(Resources + "rocket.png");      
            }
            
            var g = Graphics.FromImage(back);
            var p = new Point(_rocket.GetX(), _rocket.GetY()); 
            g.DrawImage(actor, p);

            if (DrawHitboxes)
            {
                var actorRect = new Rectangle(_rocket.GetX(), _rocket.GetY(), _rocket.GetWidth(), _rocket.GetHeight());
                var actorPen = new Pen(Brushes.DeepSkyBlue);
                actorPen.Width = 1.0F;
                g.DrawRectangle(actorPen, actorRect);
            }

            back = DrawAsteroids(back);
            back = DrawBullets(back);
            return back;
        }

        private void FireRocket()
        {
            for (var i = 0; i < BulletsAmount; i++)
            {
                if (!_bullets[i].GetDrawingStatus())
                {
                    _bullets[i].SetDrawingStatus(true);
                    _bullets[i].SetPosition(_rocket.GetX() + _rocket.GetWidth() / 2 - _bullets[i].GetWidth() / 2, _rocket.GetY() - 23);
                    break;
                }
            }    
        }

        private void GetMoves()
        {
            var rightKeyIsPressed = GetAsyncKeyState(Convert.ToInt32(Keys.D)) != 0;
            var leftKeyIsPressed = GetAsyncKeyState(Convert.ToInt32(Keys.A)) != 0;
            var upKeyIsPressed = GetAsyncKeyState(Convert.ToInt32(Keys.W)) != 0;
            var downIsPressed = GetAsyncKeyState(Convert.ToInt32(Keys.S)) != 0;
            var spaceKeyIsPressed = GetAsyncKeyState(Convert.ToInt32(Keys.Space)) != 0;

            if (upKeyIsPressed)
            {
                if (_rocket.CheckWorldHeights(Size.Height, MoveSize, false))
                {
                    _rocket.DecY(MoveSize);
                }
            }
            else if (downIsPressed)
            {
                if (_rocket.CheckWorldHeights(Size.Height - _rocket.GetHeight(), MoveSize, true))
                {
                    _rocket.IncY(MoveSize);
                }
            }
            if (rightKeyIsPressed)
            {
                if (_rocket.CheckWorldBorders(Size.Width - 65, MoveSize, true))
                {
                    _rocket.IsMovingRight = true;
                    _rocket.IsMovingLeft = false;
                    _rocket.IncX(MoveSize);
                }         
            }
            else if (leftKeyIsPressed)
            {
                if (_rocket.CheckWorldBorders(Size.Width, MoveSize, false))
                {
                    _rocket.IsMovingRight = false;
                    _rocket.IsMovingLeft = true;
                    _rocket.DecX(MoveSize);
                }    
            }
            else
            {
                _rocket.IsMovingRight = false;
                _rocket.IsMovingLeft = false;
            }
            if (spaceKeyIsPressed && _lastFired > FireRate)
            {
                _lastFired = 0;
                _rocket.IncUsedBullets(1);
                label3.Text = @"BULLETS: " + _rocket.GetUsedBullets();
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
                if (_asteroids[i].GetY() > Height - _asteroids[i].GetHeight() - 10)
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
                var position = rand.Next(10, Width - 60);
                asteroidSpawnPoints.Add(position);
            }
            var count = 0;
            var index = 0;
            do
            {
                if (!_asteroids[index].GetDrawingStatus() && !_asteroids[index].GetExplosionStatus())
                {
                    _asteroids[index].SetDrawingStatus(true);
                    _asteroids[index].SetPosition(asteroidSpawnPoints[count], 30);
                    count++;
                }
                index++;
            } while (count < rowSize && index < AsteroidsAmount);
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
                                _asteroids[j].SetExplosionStatus(true);
                                _asteroids[j].SetDrawingStatus(false);
                                _asteroids[j].SetExplosionTimer(0);
                                
                                _bullets[i].SetDrawingStatus(false);
                                _rocket.IncHighScore(50);
                                _rocket.IncHits(1);
                                label4.Text = @"HITS: " + _rocket.GetHits();
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
                        _asteroids[i].SetExplosionStatus(true);
                        _asteroids[i].SetDrawingStatus(false);
                        _asteroids[i].SetExplosionTimer(0);
                        _rocket.DecHealth(HealthDecRate);
                    }    
                }
            }
        }

        private void UpdateLabels()
        {
            label1.Text = @"SCORE: " + _rocket.GetHighScore();
            label2.Text = @"HEALTH: " + _rocket.GetHealth();
            label2.ForeColor = _rocket.GetHealth() <= 20 ? Color.Red : Color.WhiteSmoke;
            label3.Text = @"BULLETS: " + _rocket.GetUsedBullets();
            label4.Text = @"HITS: " + _rocket.GetHits();
            var currentDateTime = DateTime.Now;
            var formattedDateTime = currentDateTime.ToString("HH:mm:ss");
            label5.Text = @"TIME: " + formattedDateTime;
        }

        private static Image DrawFinBackground()
        {
            var back = Image.FromFile(Resources + "back_fin.jpg");
            return back;
        }

        private void MainLoopExecute()
        {
            if (_lastFired < FireRate + 1)
            {
                _lastFired++;
            }

            if (_canMove)
            {
                GetMoves();
                _rocket.IncHighScore(2);
                if (_lastSpawned > SpawnRate + 1)
                {
                    SpawnNewAsteroids();
                    _lastSpawned = 0;
                }
                _lastSpawned++;
                
                UpdateLabels();
                CheckRocketCollisions();
                CheckBulletCollisions();
                MoveBullets();
                MoveAsteroids();
                BackgroundImage = DrawBackground();
            }
            else
            {
                BackgroundImage = DrawFinBackground();
            }
            
            if (_rocket.GetHealth() <= 0)
            {
                _canMove = false;
                var rKeyIsPressed = GetAsyncKeyState(Convert.ToInt32(Keys.R)) != 0;
                if (rKeyIsPressed)
                {
                    StartGame();
                }
            }
        }

        private void OnTimedEvent(Object source, System.Timers.ElapsedEventArgs e)
        {
            MainLoopExecute();
        }
    }
}