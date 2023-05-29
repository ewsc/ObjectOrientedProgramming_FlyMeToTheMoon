﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace FlyMeToTheMoon
{
    public sealed partial class GameField : Form
    {
        [DllImport("user32.dll")]
        private static extern short GetAsyncKeyState(int key);
        
        private const int BulletsAmount = 20;
        private const int AsteroidsAmount = 25;
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
        private int _currentSelectedMenuItem;
        
        private const string Resources = "../../resources/";
        private const string SaveGameFile = "../../saves/savefile.fmtm";

        private readonly List<Bullet> _bullets = new List<Bullet>();
        private readonly List<Asteroid> _asteroids = new List<Asteroid>();
        private readonly List<MenuItem> _menu = new List<MenuItem>();
        private readonly List<Size> _resolutions = new List<Size>();
        private readonly Player _rocket = new Player();
        private readonly List<Message> _gameNotifications = new List<Message>();
        private int _lastFired;
        private int _lastSpawned;
        private static System.Timers.Timer _aTimer;
        private bool _canMove;
        private bool _menuOpened;
        private int _lastOpened;
        private int _lastJumpedMenu;
        private int _lastEnterPressed;
        
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
            AddMessage("Game started!", 100);
            
            label1.Text = @"SCORE: " + _rocket.GetHighScore();
            label2.Text = @"HEALTH: " + _rocket.GetHealth();
            label3.Text = @"ACCURACY: " + _rocket.GetAccuracy();
            label4.Text = @"DIFFICULTY: " + GetDifficultyStrings(_rocket.GetDifficulty());
            var currentDateTime = DateTime.Now;
            label5.Text = @"TIME: " + currentDateTime;
            label6.Text = @"|| MENU";
        }

        private void InitResolution()
        {
            var res0 = new Size(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height);
            _resolutions.Add(res0);
            var res1 = new Size(1680, 1050);
            _resolutions.Add(res1);
            var res2 = new Size(1440, 900);
            _resolutions.Add(res2);
            var res3 = new Size(1400, 1050);
            _resolutions.Add(res3);
            var res4 = new Size(1366, 768);
            _resolutions.Add(res4);
            var res5 = new Size(1280, 1080);
            _resolutions.Add(res5);
        }

        private void AddMessage(string text, int duration)
        {
            var tempMessage = new Message();
            tempMessage.SetPosition(10, Height - 100);
            tempMessage.SetWidthHeight(text.Length * 50, 100);
            tempMessage.SetMessage(text);
            tempMessage.SetDuration(duration);
            tempMessage.SetDrawingStatus(true);
            _gameNotifications.Add(tempMessage);
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
            _rocket.SetResolution(0);
            _rocket.IsMovingLeft = false;
            _rocket.IsMovingRight = false;
            InitBullets();
            InitAsteroids();
            _aTimer.Enabled = true;
            InitResolution();
            InitMenuItems();
            ResizeBackgrounds();
        }

        private void ResizeBackgrounds()
        {
            var mainBack = Image.FromFile(Resources + "back.jpg");
            for (var i = 0; i < _resolutions.Count; i++)
            {
                var tempImage = (Image) new Bitmap(mainBack, _resolutions[i]);
                tempImage.Save(Resources + "back" + i + ".jpg");
            }   
        }

        private string ReturnResolutionName()
        {
            switch (_rocket.GetResolution())
            {
                case 0:
                    return "1920x1080";
                case 1:
                    return "1680x1050";
                case 2:
                    return "1440x900";
                case 3:
                    return "1400x1050";
                case 4:
                    return "1366x768";
                case 5:
                    return "1280x1080";
                default:
                    return "unknown";
            }
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
            
            var resItem = new MenuItem();
            resItem.SetItemName("Resolution");
            resItem.SetItemAttr(": " + ReturnResolutionName());
            resItem.SetPosition(80, 650);
            resItem.SetWidthHeight(1000, 100);
            _menu.Add(resItem);
            
            var exitItem = new MenuItem();
            exitItem.SetItemName("Exit");
            exitItem.SetPosition(80, 750);
            exitItem.SetWidthHeight(300, 100);
            _menu.Add(exitItem);
        }

        private void PlaceLabels()
        {
            var size = Screen.PrimaryScreen.Bounds.Width / 6;
            
            label1.Left = size * 0;
            label2.Left = size * 1;
            label3.Left = size * 2;
            label4.Left = size * 3;
            label5.Left = size * 4;
            label6.Left = size * 5;
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
            Image back;
            back = _rocket.GetResolution() == 0 ? Image.FromFile(Resources + "back.jpg") : Image.FromFile(Resources + "back" + _rocket.GetResolution() + ".jpg");
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
            BackgroundImage = CheckMessages(BackgroundImage);
        }

        private Image CheckMessages(Image back)
        {
            foreach (var message in _gameNotifications)
            {
                if (!message.GetDrawingStatus()) continue;
                if (message.GetDuration() <= 0)
                {
                    message.SetDrawingStatus(false);
                }
                message.DecDuration(1);
                var g = Graphics.FromImage(back);
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                var textRect = new RectangleF(message.GetX(), message.GetY(), message.GetWidth(), message.GetHeight());
                var outText = message.GetMessage();
                g.DrawString(outText, new Font("hooge 05_55",72), Brushes.WhiteSmoke, textRect);
                g.Flush();
                break;
            }
            return back;
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
            var downKeyIsPressed = GetPressedKey("S");
            var upKeyIsPressed = GetPressedKey("W");
            
            if (_lastJumpedMenu >= 5)
            {
                if (upKeyIsPressed && _currentSelectedMenuItem != 0)
                {
                    _currentSelectedMenuItem--;
                }
                else if (downKeyIsPressed && _currentSelectedMenuItem != _menu.Count - 1)
                {
                    _currentSelectedMenuItem++;
                }
                _lastJumpedMenu = 0;
            }

            if (_lastJumpedMenu <= 5)
            {
                _lastJumpedMenu++;
            }
            
            var back = Image.FromFile(Resources + "back_menu.jpg");
            var g = Graphics.FromImage(back);
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;
            for (var i = 0; i < _menu.Count; i++)
            {
                var textRect = new RectangleF(_menu[i].GetX(), _menu[i].GetY(), _menu[i].GetWidth(), _menu[i].GetHeight());
                var outText = _menu[i].GetItemName() + _menu[i].GetItemAttr();
                g.DrawString(outText, new Font("hooge 05_55",72), Brushes.WhiteSmoke, textRect);

                if (i != _currentSelectedMenuItem) continue;
                var textRectBord = new Rectangle(_menu[i].GetX(), _menu[i].GetY(),
                    _menu[i].GetWidth(), _menu[i].GetHeight());
                var itemPen = new Pen(Brushes.WhiteSmoke);
                itemPen.Width = 1.0F;
                g.DrawRectangle(itemPen, textRectBord);
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
                    _currentSelectedMenuItem = 0;
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
                
                var enterKeyIsPressed = GetPressedKey("Enter");
                if (_lastEnterPressed >= 20)
                {
                    if (enterKeyIsPressed)
                    {
                        ExecuteMenuItem(_menu[_currentSelectedMenuItem]);
                        _lastEnterPressed = 0;
                    }
                }
                if (_lastEnterPressed <= 20)
                {
                    _lastEnterPressed++;
                }
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

        private void SaveGame()
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
                    writer.WriteLine("score=" + _rocket.GetHighScore());      
                    writer.WriteLine("bullets=" + _rocket.GetUsedBullets());    
                    writer.WriteLine("hits=" + _rocket.GetHits());    
                    writer.WriteLine("health=" + _rocket.GetHealth());
                    writer.WriteLine("difficulty=" + _rocket.GetDifficulty());
                    writer.WriteLine("posX=" + _rocket.GetX());
                    writer.WriteLine("posY=" + _rocket.GetY());
                    writer.WriteLine("-USER");
                    
                    writer.WriteLine("+ASTEROIDS");
                    foreach (var asteroid in _asteroids)
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
                    foreach (var bullet in _bullets)
                    {
                        if (!bullet.GetDrawingStatus()) continue;
                        writer.WriteLine("posX=" + bullet.GetX());
                        writer.WriteLine("posY=" + bullet.GetY());
                        writer.WriteLine(">");
                    }
                    writer.WriteLine("-BULLETS");
                } 
                AddMessage("Game Saved!", 100);
            }
            catch (Exception ex)    
            {    
                AddMessage(ex.ToString(), 100);   
            }
        }
        
        private void AddNewAsteroid(Asteroid astro)
        {
            for (var i = 0; i < AsteroidsAmount; i++)
            {
                if (_asteroids[i].GetDrawingStatus()) continue;
                _asteroids[i] = astro;
                break;
            }  
        }
        
        private void AddBullet(Bullet bulka)
        {
            for (var i = 0; i < BulletsAmount; i++)
            {
                if (_bullets[i].GetDrawingStatus()) continue;
                _bullets[i] = bulka;
                break;
            }  
        }

        private void LoadGame()
        {
            try
            {
                if (!File.Exists(SaveGameFile)) return;
                
                var lines = File.ReadAllText(SaveGameFile);
                var pFrom = lines.IndexOf("+USER", StringComparison.Ordinal) + "+USER".Length;
                var pTo = lines.LastIndexOf("-USER", StringComparison.Ordinal);
                var result = lines.Substring(pFrom, pTo - pFrom);
                var regex = new[] { "\r\n" };
                var userStr = result.Split(regex, StringSplitOptions.None);
                
                _rocket.SetHighScore(int.Parse(userStr[1].Split('=')[1]));
                _rocket.SetUsedBullets(int.Parse(userStr[2].Split('=')[1]));
                _rocket.SetHits(int.Parse(userStr[3].Split('=')[1]));
                _rocket.SetHeath(int.Parse(userStr[4].Split('=')[1]));
                _rocket.SetDifficulty(int.Parse(userStr[5].Split('=')[1]));
                _rocket.SetPosition(int.Parse(userStr[6].Split('=')[1]), int.Parse(userStr[7].Split('=')[1]));
                    
                ChangeDifficulty();
                
                pFrom = lines.IndexOf("+ASTEROIDS", StringComparison.Ordinal) + "+ASTEROIDS".Length;
                pTo = lines.LastIndexOf("-ASTEROIDS", StringComparison.Ordinal);
                result = lines.Substring(pFrom, pTo - pFrom);
                var astroStr = result.Split('[');
                
                InitAsteroids();
                foreach (var line in astroStr)
                {
                    if (line.Length <= 5) continue;
                    var tempAstroLine = line.Split(regex, StringSplitOptions.None);
                    var tempAstro = new Asteroid();
                    tempAstro.SetPosition(int.Parse(tempAstroLine[1].Split('=')[1]), int.Parse(tempAstroLine[2].Split('=')[1]));
                    tempAstro.SetMoveSpeed(int.Parse(tempAstroLine[3].Split('=')[1]));
                    tempAstro.SetExplosionTimer(int.Parse(tempAstroLine[4].Split('=')[1]));
                    switch (tempAstroLine[5].Split('=')[1])
                    {
                        case "True":
                            tempAstro.SetExplosionStatus(true);
                            tempAstro.SetDrawingStatus(false);
                            break;
                        case "False":
                            tempAstro.SetExplosionStatus(false);
                            tempAstro.SetDrawingStatus(true);
                            break;
                    }
                    tempAstro.SetWidthHeight(64, 64);
                    AddNewAsteroid(tempAstro);
                }
                
                pFrom = lines.IndexOf("+BULLETS", StringComparison.Ordinal) + "+BULLETS".Length;
                pTo = lines.LastIndexOf("-BULLETS", StringComparison.Ordinal);
                result = lines.Substring(pFrom, pTo - pFrom);
                var bulletStr = result.Split('>');
                
                InitBullets();
                
                foreach (var line in bulletStr)
                {
                    if (line.Length <= 5) continue;
                    var tempBulletLine = line.Split(regex, StringSplitOptions.None);
                    var tempBullet = new Bullet();
                    tempBullet.SetDrawingStatus(true);
                    tempBullet.SetWidthHeight(10, 32);
                    tempBullet.SetPosition(int.Parse(tempBulletLine[1].Split('=')[1]), int.Parse(tempBulletLine[2].Split('=')[1]));
                    AddBullet(tempBullet);
                }
                
                AddMessage("Game Loaded!", 100);
            }
            catch (Exception ex)
            {
                AddMessage(ex.ToString(), 100); 
            }   
        }

        private void ChangeResolution()
        {
            Size = _resolutions[_rocket.GetResolution()];
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
            else if (item.GetItemName() == "Save")
            {
                SaveGame();
                _menuOpened = false;
            }
            else if (item.GetItemName() == "Load")
            {
                LoadGame();
                _menuOpened = false;
            }
            else if (item.GetItemName() == "Resolution")
            {
                _rocket.IncResolution();
                item.SetItemAttr(": " + ReturnResolutionName());
                ChangeResolution();
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

        private void label6_Click(object sender, EventArgs e)
        {
            _menuOpened = true;
        }
    }
}