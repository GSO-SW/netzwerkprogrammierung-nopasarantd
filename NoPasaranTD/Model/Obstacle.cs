using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace NoPasaranTD.Model
{
    [JsonObject(MemberSerialization.OptOut)]
    public class Obstacle
    {
        public Obstacle(ObstacleType obstacleType, Rectangle hitbox)
        {
            ObstacleType = obstacleType;
            Hitbox = hitbox;
        }

        public ObstacleType ObstacleType { get; set; }
        public Rectangle Hitbox { get; set; } // Maybe change to RectangleF

        [JsonIgnore]
        public Bitmap Image { get; } // TODO: Link with static class
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
