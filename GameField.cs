using System;
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
        private const int MenuOpenDelay = 50;
        private const int BonusesAmount = 5;
        private const int BonusSpeed = 3;
        private const int BonusTypeMax = 6;
        private const int AsteroidAddScore = 20;
        
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
        private int _bonusSpawnRate = 200;

        private const string Resources = "../../resources/";
        private const string SaveGameFile = "../../saves/savefile.fmtm";

        private List<Bullet> _bullets = new List<Bullet>();
        private List<Asteroid> _asteroids = new List<Asteroid>();
        private List<Bonus> _bonuses = new List<Bonus>();
        private readonly List<MenuItem> _menu = new List<MenuItem>();
        private readonly List<Size> _resolutions = new List<Size>();
        private Player _rocket = new Player();
        private List<Message> _gameNotifications = new List<Message>();
        private List<Message> _scoresFallen = new List<Message>();
        
        private static System.Timers.Timer _aTimer;
        private int _lastFired;
        private int _lastBonusSpawned;
        private int _lastSpawned;
        private bool _canMove;
        private bool _menuOpened;
        private int _lastOpened;
        private int _lastJumpedMenu;
        private int _lastEnterPressed;
        private int _currentSelectedMenuItem;
        
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
            
            var startMessage = new Message();
            startMessage.SetPosition(10, Height - 100);
            startMessage.AddMessage("Game Started!", 100, ref _gameNotifications);
            
            StartGame();
            
            label6.Text = @"|| MENU";
        }

        private void InitResolution()
        {
            _resolutions.Clear();
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
                _bonusSpawnRate = 200;
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
                _bonusSpawnRate = 500;
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
                _bonusSpawnRate = 1000; 
            }
        }

        private void StartGame()
        {
            _canMove = true;
            _menuOpened = false;
            _rocket.SetWidthHeight(56, 128);
            _rocket.SetDifficulty(1);
            _rocket.SetPosition((Width / 2) - _rocket.GetWidth(), 2 * (Height / 3));
            _rocket.SetHighScore(0);
            _rocket.SetHeath(100);
            _rocket.SetUsedBullets(0);
            _rocket.SetHits(0);
            _rocket.SetResolution(0);
            _rocket.IsMovingLeft = false;
            _rocket.IsMovingRight = false;
            
            ChangeDifficulty();
            InitBullets();
            InitAsteroids();
            InitBonuses();
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
        
        private void InitBonuses()
        {
            _bonuses.Clear();
            for (var i = 0; i < BonusesAmount; i++)
            {
                var tempBonus = new Bonus();
                tempBonus.SetDrawingStatus(false);
                tempBonus.SetWidthHeight(32, 32);
                _bonuses.Add(tempBonus);
            }    
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
        
        private Image DrawBackground()
        {
            var back = _rocket.GetResolution() == 0 ? Image.FromFile(Resources + "back.jpg") : Image.FromFile(Resources + "back" + _rocket.GetResolution() + ".jpg");
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

            var drawer = new Drawer();
            back = drawer.DrawAsteroids(back, ref _asteroids, AsteroidsAmount, Resources, MaxExplosionTime, ExplosionTimer);
            back = drawer.DrawBullets(back, ref _bullets, BulletsAmount, Resources);
            back = drawer.DrawBonuses(back, ref _bonuses, BonusesAmount, Resources);
            back = CheckScores(back, 18);
            return back;
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
                var spawner = new Spawner();
                spawner.FireRocket(ref _rocket, ref _bullets, BulletsAmount);
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

        private void MoveBonuses()
        {
            for (var i = 0; i < BonusesAmount; i++)
            {
                if (!_bonuses[i].GetDrawingStatus()) continue;
                _bonuses[i].IncY(BonusSpeed);
                if (_bonuses[i].GetY() >= Height - 20)
                {
                    _bonuses[i].SetDrawingStatus(false);
                }
            }
        }

        private void ExecuteBonus(int bonusType)
        {
            if (bonusType == 1)
            {
                _rocket.IncHealth(10);
                var message = new Message();
                message.SetPosition(10, Height - 100);
                message.AddMessage("+10 HP", 100, ref _gameNotifications);
            }
            else if (bonusType == 2)
            {
                _rocket.DecHealth(10);
                var message = new Message();
                message.SetPosition(10, Height - 100);
                message.AddMessage("-10 HP", 100, ref _gameNotifications);
            }
            else if (bonusType == 3)
            {
                _fireRate -= 1;
                var message = new Message();
                message.SetPosition(10, Height - 100);
                message.AddMessage("Firerate upgraded!", 100, ref _gameNotifications);
            }
            else if (bonusType == 4)
            {
                _fireRate += 1;
                var message = new Message();
                message.SetPosition(10, Height - 100);
                message.AddMessage("Firerate downgraded!", 100, ref _gameNotifications);
            }
            else if (bonusType == 5)
            {
                _rocket.BonusSetHigh(100, true);
                var message = new Message();
                message.SetPosition(10, Height - 100);
                message.AddMessage("+100 SCORE", 100, ref _gameNotifications);
            }
            else if (bonusType == 6)
            {
                _rocket.BonusSetHigh(100, false);
                var message = new Message();
                message.SetPosition(10, Height - 100);
                message.AddMessage("-100 SCORE", 100, ref _gameNotifications);
            }
        }
        
        private void NormalMovingState()
        {
            GetMoves();
            _rocket.IncHighScore(_highScoreIncValue);
            if (_lastSpawned > _spawnRate + 1)
            {
                var spawner = new Spawner();
                spawner.SpawnNewAsteroids(ref _asteroids, AsteroidsAmount, _asteroidMoveSpeedMin, _asteroidMoveSpeedMax, _maxAsteroidRow, Width);
                _lastSpawned = 0;
            }

            if (_lastBonusSpawned > _bonusSpawnRate + 1)
            {
                var spawner = new Spawner();
                spawner.SpawnBonus(ref _bonuses, BonusesAmount, BonusTypeMax, Width);
                _lastBonusSpawned = 0;
            }
            
            _lastSpawned++;
            _lastBonusSpawned++;

            var collision = new CollisionChecker();
            collision.CheckRocketCollisions(ref _rocket, ref _asteroids, _healthDecRate, AsteroidsAmount);
            collision.CheckBulletCollisions(ref _rocket, ref _asteroids, ref _bullets, ref _scoresFallen, BulletsAmount, AsteroidsAmount, AsteroidAddScore);
            var bonusEffect = collision.CheckBonusCollision(ref _rocket, ref _bonuses, BonusesAmount);

            if (bonusEffect != -1)
            {
                ExecuteBonus(bonusEffect);
            }
            
            MoveBullets();
            MoveAsteroids();
            MoveBonuses();
            MoveScores();
            BackgroundImage = DrawBackground();
            BackgroundImage = CheckMessages(BackgroundImage, ref _gameNotifications, 72);
        }

        private void MoveScores()
        {
            foreach (var t in _scoresFallen)
            {
                t.IncY(BonusSpeed);
                if (t.GetY() <= Height - 60)
                {
                    t.SetDrawingStatus(false);
                }
            }
        }
        
        private Image CheckScores(Image back, int size)
        {
            for (var i = 0; i < _scoresFallen.Count; i++)
            {
                if (_scoresFallen[i].GetY() >= Height - 60)
                {
                    _scoresFallen.RemoveAt(i);
                }
                var g = Graphics.FromImage(back);
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                var textRect = new RectangleF(_scoresFallen[i].GetX(), _scoresFallen[i].GetY(), _scoresFallen[i].GetWidth(), _scoresFallen[i].GetHeight());
                var outText = _scoresFallen[i].GetMessage();
                g.DrawString(outText, new Font("hooge 05_55", size), Brushes.WhiteSmoke, textRect);
                g.Flush();
            }
            return back;
        }

        private Image CheckMessages(Image back, ref List<Message> messages, int size)
        {
            foreach (var message in messages)
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
                g.DrawString(outText, new Font("hooge 05_55",size), Brushes.WhiteSmoke, textRect);
                var buttonRect = new RectangleF(message.GetX() + (message.GetMessage().Length * 45), message.GetY(), 20, 20);
                g.DrawString("x", new Font("hooge 05_55", size - 50), Brushes.WhiteSmoke, buttonRect);
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

        private void MainLoopExecute(Event TimerEvent)
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
            var TimerEvent = new Event();
            TimerEvent.SetEventName("Tick");
            TimerEvent.SetTime();
            MainLoopExecute(TimerEvent);
        }

        private void ChangeDifficulty(MenuItem item)
        {
            _rocket.IncDifficulty();
            item.SetItemAttr(": " + GetDifficultyStrings(_rocket.GetDifficulty()));
            ChangeDifficulty();
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
                
                var startMessage = new Message();
                startMessage.SetPosition(10, Height - 100);
                startMessage.AddMessage("Game Loaded!", 100, ref _gameNotifications);
            }
            catch (Exception ex)
            {
                var startMessage = new Message();
                startMessage.SetPosition(10, Height - 100);
                startMessage.AddMessage(ex.ToString(), 100, ref _gameNotifications);
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
                var fileWriter = new FileWriter();
                fileWriter.SaveGame(_rocket, _asteroids, _bullets);
                _menuOpened = false;
                var startMessage = new Message();
                startMessage.SetPosition(10, Height - 100);
                startMessage.AddMessage("Game Saved!", 100, ref _gameNotifications);
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
            if (_menuOpened)
            {
                foreach (var item in _menu)
                {
                    var textRect = new RectangleF(item.GetX(), item.GetY(), item.GetWidth(), item.GetHeight());
                    if (!textRect.Contains(e.Location)) continue;
                    ExecuteMenuItem(item);
                    break;
                }
            }

            foreach (var message in _gameNotifications)
            {
                var buttonRect = new RectangleF(message.GetX() + (message.GetMessage().Length * 45), message.GetY(), 20, 20);    
                if (!buttonRect.Contains(e.Location)) continue;
                message.SetDuration(0);
                break;
            }
        }

        private void label6_Click(object sender, EventArgs e)
        {
            _menuOpened = true;
        }
    }
}