using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NoPasaranTD.Model
{
    public class Obstacle
    {
        public Obstacle(ObstacleType obstacleType, Rectangle hitbox)
        {
            ObstacleType = obstacleType;
            Hitbox = hitbox;
        }

        public ObstacleType ObstacleType { get; set; }
        public Rectangle Hitbox { get; set; } // Maybe change to RectangleF

        public Bitmap Image { get; } // Link with static class
    }

    public enum ObstacleType
    {
        House0 = 0,
        House1 = 1,
        Tree = 2,
        Rock = 3,
        Fence = 4
    }
}
