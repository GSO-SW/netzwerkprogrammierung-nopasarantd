using NoPasaranTD.Engine;
using NoPasaranTD.Networking;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace NoPasaranTD.Visuals.Ingame
{
    public class PlayerListContainer : GuiComponent
    {
        public ListContainer<Networking.NetworkClient,PlayerItemContainer> PlayersContainer { get; set; }

        /// <summary>
        /// Die Hintergrundfarbe des Containers
        /// </summary>
        public Brush Background { get; set; } = new SolidBrush(Color.FromArgb(87, 117, 255));

        /// <summary>
        /// Die Schriftfarbe der Texte
        /// </summary>
        public Brush Foreground { get; set; }

        /// <summary>
        /// Die Fonts der Texte
        /// </summary>
        public Font TextFont { get; set; }

        public Brush BorderBrush { get; set; } = new SolidBrush(Color.FromArgb(114, 133, 219));
        public int BorderSize { get; set; } = 1;

        private Game currentGame;

        public PlayerListContainer()
        {
            
        }

        public void Init(Game game)
        {
            currentGame = game;

            PlayersContainer = new ListContainer<NetworkClient, PlayerItemContainer>()
            {
                Items = new NotifyCollection<NetworkClient>(currentGame.NetworkHandler.Participants),
                Margin = 3,
                Orientation = Orientation.Vertical,
                ItemSize = new System.Drawing.Size(180, 25),
                Position = new System.Drawing.Point(Bounds.X + 5, Bounds.Y + 5),
                ContainerSize = new System.Drawing.Size(Bounds.Width - 10, Bounds.Height - 10),
                BackgroundColor = new SolidBrush(Color.FromArgb(250, 143, 167, 186)),
                ListArgs = new object[] { currentGame.NetworkHandler.LocalPlayer },
            };

            PlayersContainer.DefineItems();
        }

        public override void Render(Graphics g)
        {
            if (Visible)
            {
                g.FillRectangle(BorderBrush, Bounds);
                g.FillRectangle(Background, new Rectangle(Bounds.X + BorderSize, Bounds.Y + BorderSize, Bounds.Width - BorderSize * 2, Bounds.Height - BorderSize * 2));
                PlayersContainer.Render(g);
            }        
        }

        public override void Update() =>
            PlayersContainer.Update();
    }
}
