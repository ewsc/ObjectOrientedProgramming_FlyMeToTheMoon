using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace FlyMeToTheMoon
{
    public partial class GameField : Form
    {
        [DllImport("user32.dll")]
        static extern short GetAsyncKeyState(int key);
        
        private const string Resources = "../../resources/";
        private const int MoveSize = 3;

        private readonly Player _rocket = new Player();
        public GameField()
        {
            InitializeComponent();
            _rocket.SetWidhtHeight(64, 64);
            _rocket.setPosition(Width / 2 + _rocket.GetWidth(), 369);
            _rocket.SetHighScore(0);
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
            return back;
        }
        
        private void Timer_Tick(object sender, EventArgs e)
        {
            bool rightKeyIsPressed = GetAsyncKeyState(Convert.ToInt32(Keys.D)) != 0;
            bool leftKeyIsPressed = GetAsyncKeyState(Convert.ToInt32(Keys.A)) != 0;

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
            
            _rocket.IncHighScore(1);
            BackgroundImage = DrawBackground();
            SetHigh();
        }
    }
}