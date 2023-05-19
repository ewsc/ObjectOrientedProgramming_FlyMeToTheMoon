using System.Drawing;

namespace FlyMeToTheMoon
{
    public class DrawObject
    {
        protected int PosX;
        protected int PosY;
        protected int Width;
        protected int Height;
        protected bool IsDrawing;

        public void SetDrawingStatus()
        {
            IsDrawing = true;
        }

        public void SetWidhtHeight(int width, int height)
        {
            Height = height;
            Width = width;
        }

        public bool CheckWorldBorders(int maxBord, int moveSize, bool inc)
        {
            if ((PosX - moveSize <= 0) && !inc)
            {
                return false;
            }
            if ((PosX + moveSize >= maxBord) && inc)
            {
                return false;
            }
            return true;
        }

        public void IncX(int incX)
        {
            PosX += incX;
        }
        
        public void DecX(int decX)
        {
            PosX -= decX;
        }
        
        public int GetWidth()
        {
            return Width;
        }

        public int GetX()
        {
            return PosX;
        }
        
        public int GetY()
        {
            return PosY;
        }

        public void setPosition(int posX, int posY)
        {
            PosX = posX;
            PosY = posY;
        }
    }
}