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

        public void SetDrawingStatus(bool drawingStatus)
        {
            IsDrawing = drawingStatus;
        }

        public bool GetDrawingStatus()
        {
            return IsDrawing;
        }

        public void SetWidthHeight(int width, int height)
        {
            Height = height;
            Width = width;
        }

        public bool CheckWorldHeights(int maxHeight, int moveSize, bool inc)
        {
            if ((PosY - moveSize <= 0) && !inc)
            {
                return false;
            }
            if ((PosY + moveSize >= maxHeight) && inc)
            {
                return false;
            }
            return true;    
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
        
        public void IncY(int incY)
        {
            PosY += incY;
        }
        
        public void DecX(int decX)
        {
            PosX -= decX;
        }
        
        public void DecY(int decY)
        {
            PosY -= decY;
        }
        
        public int GetWidth()
        {
            return Width;
        }
        
        public int GetHeight()
        {
            return Height;
        }

        public int GetX()
        {
            return PosX;
        }
        
        public int GetY()
        {
            return PosY;
        }

        public void SetPosition(int posX, int posY)
        {
            PosX = posX;
            PosY = posY;
        }
    }
}