using NoPasaranTD.Data;
using NoPasaranTD.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NoPasaranTD
{
    public partial class Display : Form
    {
        public Display()
        {
            InitializeComponent();
            Load += Display_Load;
        }

        private async void Display_Load(object sender, EventArgs e)
        {
            MapData mapData = new MapData();

            Map map = new Map()
            {
                BackgroundPath = "\\img\\img_background.jpg",
                BalloonPath = new Utilities.Vector2D[]
                {
                    new Utilities.Vector2D(2,3),
                    new Utilities.Vector2D(2, 4),
                    new Utilities.Vector2D(2, 5),
                },
                Obstacles = new List<Obstacle>()
                {
                    new Obstacle(ObstacleType.Rock,new Rectangle(2,2,2,2)),
                    new Obstacle(ObstacleType.Rock,new Rectangle(2,2,2,2)),
                }

            };
            //await Task.Run(() => mapData.CreateNewMapAsync("test2", map));

            Map map1 = await Task.Run(() => mapData.GetMapByPathAsync("test2"));
        }
    }
}
