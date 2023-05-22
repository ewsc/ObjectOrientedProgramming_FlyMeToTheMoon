namespace FlyMeToTheMoon
{
    public class Asteroid : DrawObject
    {
        public void MoveAsteroid(int moveSize)
        {
            IncY(moveSize);    
        } 
    }
}