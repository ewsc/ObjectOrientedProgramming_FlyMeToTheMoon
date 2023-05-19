using System;
using System.Drawing;
using System.Windows.Forms;

namespace FlyMeToTheMoon
{
    public partial class GameField : Form
    {
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
            _rocket.IncHighScore(1);
            BackgroundImage = DrawBackground();
            SetHigh();
        }

        private void GameField_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.D:
                    
                    _rocket.IncX(MoveSize);
                    break;
                case Keys.A:
                    _rocket.DecX(MoveSize);   
                    break;
            }
        }
    }
}