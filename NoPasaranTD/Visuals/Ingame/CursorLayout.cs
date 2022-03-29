using NoPasaranTD.Engine;
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
                    g.DrawRectangle(Pens.Red,
                        info.Position.X - 5, info.Position.Y - 5, 10, 10
                    );

                    g.DrawString(info.Username, StandartText1Font, Brushes.Black,
                        info.Position.X + 15, info.Position.Y - 5);
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
