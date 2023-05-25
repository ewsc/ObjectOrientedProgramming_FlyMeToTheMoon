namespace FlyMeToTheMoon
{
    public class Asteroid : DrawObject
    {
        protected bool IsExploded;
        protected int ExplosionTimer;
        protected int MoveSpeed;

        public void SetMoveSpeed(int value)
        {
            MoveSpeed = value;
        }

        public int GetMoveSpeed()
        {
            return MoveSpeed;
        }
        public void SetExplosionStatus(bool status)
        {
            IsExploded = status;
        }
        
        public void SetExplosionTimer(int value)
        {
            ExplosionTimer = value;
        }

        public void IncExplosionTimer(int value)
        {
            ExplosionTimer += value;
        }

        public int GetExplosionTimer()
        {
            return ExplosionTimer;
        }

        public bool GetExplosionStatus()
        {
            return IsExploded;
        }
        public void MoveAsteroid(int moveSize)
        {
            IncY(moveSize);    
        } 
    }
}