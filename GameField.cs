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
        private const int MoveSize = 3;
        private const int BulletsAmount = 15;

        private readonly List<Bullet> _bullets = new List<Bullet>();
        private readonly Player _rocket = new Player();
        private int _lastFired;
        public GameField()
        {
            InitializeComponent();
            _rocket.SetWidthHeight(64, 64);
            _rocket.SetPosition(Width / 2 + _rocket.GetWidth(), 369);
            _rocket.SetHighScore(0);
            InitBullets();
        }

        private void InitBullets()
        {
            for (var i = 0; i < BulletsAmount; i++)
            {
                var tempBullet = new Bullet();
                tempBullet.SetDrawingStatus(false);
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
                if (_rocket.CheckWorldBorders(Width, MoveSize, true)) { 
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
            if (spaceKeyIsPressed && _lastFired > 20)
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
                    _bullets[i].DecY(2);
                }
            } 
        }
        
        private void Timer_Tick(object sender, EventArgs e)
        {
            _lastFired++;
            GetMoves();
            _rocket.IncHighScore(2);
            MoveBullets();
            BackgroundImage = DrawBackground();
            SetHigh();
        }
    }
}