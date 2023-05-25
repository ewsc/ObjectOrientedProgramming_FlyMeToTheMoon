using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace FlyMeToTheMoon
{
    public partial class GameField : Form
    {
        [DllImport("user32.dll")]
        private static extern short GetAsyncKeyState(int key);
        private const string Resources = "../../resources/";
        
        
        private const int BulletsAmount = 20;
        private const int AsteroidsAmount = 25;
        private const bool DrawHitboxes = false;
        private const int ExplosionTimer = 2;
        private const int MaxExplosionTime = 100;
        private const int MenuOpenDelay = 30;
        
        //LEVEL VARIABLES
        private int _moveSize = 9;
        private int _bulletMoveSize = 9;
        private int _fireRate = 13; // MIN=4
        private int _spawnRate = 20;
        private int _maxAsteroidRow = 3;
        private int _healthDecRate = 20;
        private int _asteroidMoveSpeedMin = 2;
        private int _asteroidMoveSpeedMax = 6;
        private int _highScoreIncValue = 2;

        private readonly List<Bullet> _bullets = new List<Bullet>();
        private readonly List<Asteroid> _asteroids = new List<Asteroid>();
        private readonly List<MenuItem> _menu = new List<MenuItem>();
        private readonly Player _rocket = new Player();
        private int _lastFired;
        private int _lastSpawned;
        private static System.Timers.Timer _aTimer;
        private bool _canMove;
        private bool _menuOpened;
        private int _lastOpened;
        
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
            label3.Text = @"ACCURACY: " + _rocket.GetAccuracy();
            label4.Text = @"DIFFICULTY: " + GetDifficultyStrings(_rocket.GetDifficulty());
            var currentDateTime = DateTime.Now;
            label5.Text = @"TIME: " + currentDateTime;
        }

        private void ChangeDifficulty()
        {
            if (_rocket.GetDifficulty() == 0)
            {
                _moveSize = 11;
                _bulletMoveSize = 11;
                _fireRate = 10; // MIN=4
                _spawnRate = 15;
                _maxAsteroidRow = 3;
                _healthDecRate = 10;
                _asteroidMoveSpeedMin = 1;
                _asteroidMoveSpeedMax = 5;
                _highScoreIncValue = 1;
            }
            else if (_rocket.GetDifficulty() == 1)
            {
                _moveSize = 9;
                _bulletMoveSize = 9;
                _fireRate = 13; // MIN=4
                _spawnRate = 20;
                _maxAsteroidRow = 3;
                _healthDecRate = 20;
                _asteroidMoveSpeedMin = 2;
                _asteroidMoveSpeedMax = 6;
                _highScoreIncValue = 2;
            }
            else if (_rocket.GetDifficulty() == 2)
            {
                _moveSize = 6;
                _bulletMoveSize = 6;
                _fireRate = 15; // MIN=4
                _spawnRate = 25;
                _maxAsteroidRow = 5;
                _healthDecRate = 30;
                _asteroidMoveSpeedMin = 3;
                _asteroidMoveSpeedMax = 7;
                _highScoreIncValue = 4;
            }
        }

        private void StartGame()
        {
            _canMove = true;
            _menuOpened = false;
            _rocket.SetWidthHeight(56, 128);
            _rocket.SetDifficulty(1);
            ChangeDifficulty();
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
            InitMenuItems();
        }

        private static string GetDifficultyStrings(int difficulty)
        {
            switch (difficulty)
            {
                case 0:
                    return "Easy";
                case 1:
                    return "Medium";
                case 2:
                    return "Hard";
                default:
                    return "null";
            }
        }

        private void InitMenuItems()
        {
            _menu.Clear();
            
            var resumeItem = new MenuItem();
            resumeItem.SetItemName("Resume");
            resumeItem.SetPosition(80, 250);
            resumeItem.SetWidthHeight(400, 100);
            _menu.Add(resumeItem);
            
            var saveItem = new MenuItem();
            saveItem.SetItemName("Save");
            saveItem.SetPosition(80, 350);
            saveItem.SetWidthHeight(300, 100);
            _menu.Add(saveItem);
            
            var loadItem = new MenuItem();
            loadItem.SetItemName("Load");
            loadItem.SetPosition(80, 450);
            loadItem.SetWidthHeight(300, 100);
            _menu.Add(loadItem);
            
            var diffItem = new MenuItem();
            diffItem.SetItemName("Difficulty");
            diffItem.SetItemAttr(": " + GetDifficultyStrings(_rocket.GetDifficulty()));
            diffItem.SetPosition(80, 550);
            diffItem.SetWidthHeight(800, 100);
            _menu.Add(diffItem);
            
            var exitItem = new MenuItem();
            exitItem.SetItemName("Exit");
            exitItem.SetPosition(80, 650);
            exitItem.SetWidthHeight(300, 100);
            _menu.Add(exitItem);
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

                if (!_asteroids[i].GetExplosionStatus()) continue;
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

        private static bool GetPressedKey(string keyName)
        {
            
            return GetAsyncKeyState(Convert.ToInt32(Enum.Parse(typeof(Keys), keyName, true))) != 0;
        }

        private void GetMoves()
        {
            var rightKeyIsPressed = GetPressedKey("D");
            var leftKeyIsPressed = GetPressedKey("A");
            var upKeyIsPressed = GetPressedKey("W");
            var downIsPressed = GetPressedKey("S");
            var spaceKeyIsPressed = GetPressedKey("Space");

            if (upKeyIsPressed)
            {
                if (_rocket.CheckWorldHeights(Size.Height, _moveSize, false))
                {
                    _rocket.DecY(_moveSize);
                }
            }
            else if (downIsPressed)
            {
                if (_rocket.CheckWorldHeights(Size.Height - _rocket.GetHeight(), _moveSize, true))
                {
                    _rocket.IncY(_moveSize);
                }
            }
            if (rightKeyIsPressed)
            {
                if (_rocket.CheckWorldBorders(Size.Width - 65, _moveSize, true))
                {
                    _rocket.IsMovingRight = true;
                    _rocket.IsMovingLeft = false;
                    _rocket.IncX(_moveSize);
                }         
            }
            else if (leftKeyIsPressed)
            {
                if (_rocket.CheckWorldBorders(Size.Width, _moveSize, false))
                {
                    _rocket.IsMovingRight = false;
                    _rocket.IsMovingLeft = true;
                    _rocket.DecX(_moveSize);
                }    
            }
            else
            {
                _rocket.IsMovingRight = false;
                _rocket.IsMovingLeft = false;
            }
            if (spaceKeyIsPressed && _lastFired > _fireRate)
            {
                _lastFired = 0;
                _rocket.IncUsedBullets(1);
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
                    _bullets[i].DecY(_bulletMoveSize);
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
                    _asteroids[i].MoveAsteroid(_asteroids[i].GetMoveSpeed());
                }
            } 
        }

        private void SpawnNewAsteroids()
        {
            var rand = new Random();
            var rowSize = rand.Next(1, _maxAsteroidRow);
            List<Point> asteroidSpawnPoints = new List<Point>();
            for (var i = 0; i < rowSize; i++)
            {
                var position = new Point()
                {
                    X = rand.Next(10, Width - 60),
                    Y = rand.Next(30, 100),
                };
                asteroidSpawnPoints.Add(position);
            }
            var count = 0;
            var index = 0;
            do
            {
                if (!_asteroids[index].GetDrawingStatus() && !_asteroids[index].GetExplosionStatus())
                {
                    _asteroids[index].SetDrawingStatus(true);
                    _asteroids[index].SetMoveSpeed(rand.Next(_asteroidMoveSpeedMin, _asteroidMoveSpeedMax));
                    _asteroids[index].SetPosition(asteroidSpawnPoints[count].X, asteroidSpawnPoints[count].Y);
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
                        _rocket.DecHealth(_healthDecRate);
                    }    
                }
            }
        }

        private void UpdateLabels()
        {
            label1.Text = @"SCORE: " + _rocket.GetHighScore();
            label2.Text = @"HEALTH: " + _rocket.GetHealth();
            label2.ForeColor = _rocket.GetHealth() <= 20 ? Color.Red : Color.WhiteSmoke;
            label3.Text = @"ACCURACY: " + _rocket.GetAccuracy() + @"%";
            label4.Text = @"DIFFICULTY: " + GetDifficultyStrings(_rocket.GetDifficulty());
            var currentDateTime = DateTime.Now;
            var formattedDateTime = currentDateTime.ToString("HH:mm:ss");
            label5.Text = @"TIME: " + formattedDateTime;
        }

        private static Image DrawFinBackground()
        {
            var back = Image.FromFile(Resources + "back_fin.jpg");
            return back;
        }

        private void NormalMovingState()
        {
            GetMoves();
            _rocket.IncHighScore(_highScoreIncValue);
            if (_lastSpawned > _spawnRate + 1)
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
        }

        private void NormalLoopState()
        {
            if (_lastFired < _fireRate + 1)
            {
                _lastFired++;
            }
            if (_canMove)
            {
                NormalMovingState();
            }
            else
            {
                BackgroundImage = DrawFinBackground();
            }
            UpdateLabels();
            if (_rocket.GetHealth() > 0) return;
            _canMove = false;
            var rKeyIsPressed = GetPressedKey("R");
            if (rKeyIsPressed)
            {
                StartGame();
            }    
        }

        private Image DrawMenu()
        {
            var back = Image.FromFile(Resources + "back_menu.jpg");
            var g = Graphics.FromImage(back);
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;
            foreach (var item in _menu)
            {
                var textRect = new RectangleF(item.GetX(), item.GetY(), item.GetWidth(), item.GetHeight());

                var outText = item.GetItemName() + item.GetItemAttr();
                
                g.DrawString(outText, new Font("hooge 05_55",72), Brushes.WhiteSmoke, textRect);
                
                if (DrawHitboxes)
                {
                    var textRectBord = new Rectangle(item.GetX(), item.GetY(),
                        item.GetWidth(), item.GetHeight());
                    var itemPen = new Pen(Brushes.Red);
                    itemPen.Width = 1.0F;
                    g.DrawRectangle(itemPen, textRectBord);
                }
            }
            g.Flush();
            return back;
        }

        private void MainLoopExecute()
        {
            if (GetPressedKey("Escape"))
            {
                if (_lastOpened >= MenuOpenDelay)
                {
                    _menuOpened = !_menuOpened;
                    _lastOpened = 0;
                }
                else
                {
                    _lastOpened += 10;
                }
            }

            if (_menuOpened)
            {
                BackgroundImage = DrawMenu();
            }
            else
            {
                NormalLoopState();
            }
        }

        private void OnTimedEvent(object source, System.Timers.ElapsedEventArgs e)
        {
            MainLoopExecute();
        }

        private void ChangeDifficulty(MenuItem item)
        {
            _rocket.IncDifficulty();
            item.SetItemAttr(": " + GetDifficultyStrings(_rocket.GetDifficulty()));
            ChangeDifficulty();
        }

        private void ExecuteMenuItem(MenuItem item)
        {
            if (item.GetItemName() == "Exit")
            { 
                Application.ExitThread();    
            } 
            else if (item.GetItemName() == "Difficulty")
            {
                ChangeDifficulty(item);
            }
            else if (item.GetItemName() == "Resume")
            {
                _menuOpened = false;
            }
        }

        private void GameField_MouseDown(object sender, MouseEventArgs e)
        {
            if (!_menuOpened) return;
            foreach (var item in _menu)
            {
                var textRect = new RectangleF(item.GetX(), item.GetY(), item.GetWidth(), item.GetHeight());
                if (!textRect.Contains(e.Location)) continue;
                ExecuteMenuItem(item);
                break;
            }
        }
    }
}