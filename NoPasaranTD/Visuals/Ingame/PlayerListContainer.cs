using NoPasaranTD.Logic;
using NoPasaranTD.Networking;
using System.Drawing;
using System.Windows.Forms;

namespace NoPasaranTD.Visuals.Ingame
{
    public class PlayerListContainer : GuiComponent
    {
        public ListContainer<NetworkClient, PlayerItemContainer> PlayersContainer { get; set; }

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

        /// <summary>
        /// Initialisiert alle GUI Components der Spielerliste
        /// </summary>
        /// <param name="game"></param>
        public void Init(Game game)
        {
            currentGame = game;

            PlayersContainer = new ListContainer<NetworkClient, PlayerItemContainer>()
            {
                Items = new NotifyCollection<NetworkClient>(currentGame.NetworkHandler.Participants),
                Margin = 3,
                Orientation = Orientation.Vertical,
                ItemSize = new Size(Bounds.Width - 12, 25),
                Position = new Point(Bounds.X + 3, Bounds.Y + 30),
                ContainerSize = new Size(Bounds.Width - 6, Bounds.Height - 36),
                BackgroundColor = Background,
            };

            // Übergibt den Items den derzeitigen Host des Spieles
            if (!currentGame.NetworkHandler.OfflineMode)
            {
                PlayersContainer.ListArgs = new object[] { currentGame.NetworkHandler.LocalPlayer, currentGame.NetworkHandler.Participants[0] };
                PlayersContainer.DefineItems();
            }
        }

        public override void Render(Graphics g)
        {
            if (Visible)
            {
                g.FillRectangle(Background, Bounds);
                PlayersContainer.Render(g);
                g.DrawString("Players: ", StandartText1Font, Brushes.Black, Bounds.X + 5, Bounds.Y + 5);
            }
        }

        public override void Update()
        {
            PlayersContainer.Update();
        }
    }
}
