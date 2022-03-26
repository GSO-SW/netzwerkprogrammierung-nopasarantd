using Newtonsoft.Json;
using NoPasaranTD.Data;
using System.Drawing;

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
        public Bitmap Image => StaticInfo.GetObstacleImage(ObstacleType);  // TODO: Link with static class
    }

    public enum ObstacleType
    {
        Pool = 0,
        Factory = 1,
        Tree = 2,
        Rock = 3,
        Fence = 4
    }
}
