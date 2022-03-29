using NoPasaranTD.Engine;
using NoPasaranTD.Logic;
using NoPasaranTD.Utilities;
using System;
using System.Collections.Concurrent;
using System.Drawing;
using System.Linq;

namespace NoPasaranTD.Visuals.Ingame
{
    [Serializable]
    public struct PlayerCursorInfo
    {
        public string Username { get; }
        public Vector2D Position { get; }
        public PlayerCursorInfo(string username, Vector2D pos)
        {
            Username = username;
            Position = pos;
        }
    }

    public class CursorLayout : GuiComponent
    {

        // Mouse Cursor Paketeinstellungen
        private const int MOUSE_SEND_INTERVAL = 50;

        private const int CIRCLE_RADIUS = 8;
        private const int CROSS_RADIUS = 20;

        private readonly Game game;
        private readonly ConcurrentDictionary<string, PlayerCursorInfo> playerCursors;
        public CursorLayout(Game game)
        {
            this.game = game;
            playerCursors = new ConcurrentDictionary<string, PlayerCursorInfo>();
            game.NetworkHandler.EventHandlers.Add("TransferMousePosition", TransferMousePosition);
        }

        public override void Update()
        {
            // eigene Maus schicken
            if (!game.NetworkHandler.OfflineMode && game.CurrentTick % MOUSE_SEND_INTERVAL == 0)
            {
                game.NetworkHandler.InvokeEvent("TransferMousePosition", new PlayerCursorInfo(
                    game.NetworkHandler.LocalPlayer.Name,
                    new Vector2D(StaticEngine.MouseX, StaticEngine.MouseY)
                ), false);
            }
        }

        public override void Render(Graphics g)
        {
            // zeichne die Maus Positionen von anderen wenn online
            if (!game.NetworkHandler.OfflineMode)
            {
                for (int i = playerCursors.Count - 1; i >= 0; i--)
                {
                    PlayerCursorInfo info = playerCursors.Values.ElementAt(i);
                    g.FillEllipse(Brushes.White,
                        info.Position.X - CIRCLE_RADIUS, info.Position.Y - CIRCLE_RADIUS, 2 * CIRCLE_RADIUS, 2 * CIRCLE_RADIUS
                    );

                    g.DrawLine(Pens.LightGray, info.Position.X - CROSS_RADIUS, info.Position.Y, info.Position.X + CROSS_RADIUS, info.Position.Y);
                    g.DrawLine(Pens.Gray, info.Position.X - CROSS_RADIUS, info.Position.Y - 1, info.Position.X - CROSS_RADIUS, info.Position.Y - 1);
                    g.DrawLine(Pens.Gray, info.Position.X - CROSS_RADIUS, info.Position.Y + 1, info.Position.X - CROSS_RADIUS, info.Position.Y + 1);
                    g.DrawLine(Pens.Gray, info.Position.X + CROSS_RADIUS, info.Position.Y - 1, info.Position.X + CROSS_RADIUS, info.Position.Y - 1);
                    g.DrawLine(Pens.Gray, info.Position.X + CROSS_RADIUS, info.Position.Y + 1, info.Position.X + CROSS_RADIUS, info.Position.Y + 1);


                    g.DrawLine(Pens.LightGray, info.Position.X, info.Position.Y - CROSS_RADIUS, info.Position.X, info.Position.Y + CROSS_RADIUS);
                    g.DrawLine(Pens.Gray, info.Position.X - 1, info.Position.Y - CROSS_RADIUS, info.Position.X - 1, info.Position.Y - CROSS_RADIUS);
                    g.DrawLine(Pens.Gray, info.Position.X + 1, info.Position.Y - CROSS_RADIUS, info.Position.X + 1, info.Position.Y - CROSS_RADIUS);
                    g.DrawLine(Pens.Gray, info.Position.X - 1, info.Position.Y + CROSS_RADIUS, info.Position.X - 1, info.Position.Y + CROSS_RADIUS);
                    g.DrawLine(Pens.Gray, info.Position.X + 1, info.Position.Y + CROSS_RADIUS, info.Position.X + 1, info.Position.Y + CROSS_RADIUS);


                    g.FillEllipse(Brushes.Purple,
                        info.Position.X - (CIRCLE_RADIUS - 1), info.Position.Y - (CIRCLE_RADIUS - 1),
                        (CIRCLE_RADIUS - 1) * 2, (CIRCLE_RADIUS - 1) * 2
                    );
                    g.FillEllipse(Brushes.LightGray,
                        info.Position.X - (CIRCLE_RADIUS - 5), info.Position.Y - (CIRCLE_RADIUS - 5),
                        (CIRCLE_RADIUS - 5) * 2, (CIRCLE_RADIUS - 5) * 2
                    );

                    g.DrawString(info.Username, StandartText1Font, Brushes.Black,
                        info.Position.X + 10, info.Position.Y
                    );
                }
            }
        }

        private void TransferMousePosition(object t)
        {
            PlayerCursorInfo cursorInfo = (PlayerCursorInfo)t;
            if (!cursorInfo.Username.Equals(game.NetworkHandler.LocalPlayer.Name))
            {
                playerCursors[cursorInfo.Username] = cursorInfo;
            }
        }
    }
}
