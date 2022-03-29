using NoPasaranTD.Engine;
using NoPasaranTD.Networking;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NoPasaranTD.Visuals.Ingame
{
    public class CursorLayout : GuiComponent
    {

        // Mouse Cursor Paketeinstellungen
        private int MouseSendInterval = 200;
        private static List<(int X, int Y, int TTL, ulong currentTick)> usersMousePos = new List<(int X, int Y, int TTL, ulong currentTick)>();
        private static List<string> usersMouseTag = new List<string>();

        private readonly Game game;
        public CursorLayout(Game game)
        {
            this.game = game;
            game.NetworkHandler.EventHandlers.Add("TransferMousePosition", TransferMousePosition);
        }

        public override void Update()
        {
            // eigene Maus schicken
            if (game.CurrentTick % MouseSendInterval == 0)
            {
                if (game.NetworkHandler.LocalPlayer != null)
                {
                    var networkPackage = new NetworkPackageMousePosition();
                    networkPackage.Pos = (StaticEngine.MouseX, StaticEngine.MouseY);
                    networkPackage.CurrentTick = game.CurrentTick;

                    // TODO ergänzen: den Username mitschicken statt das id ding -26.3.2022 
                    networkPackage.Username = game.NetworkHandler.LocalPlayer.Name;

                    game.NetworkHandler.InvokeEvent("TransferMousePosition", networkPackage, false);

                    if (game.CurrentTick % 1000 == 0)
                    {
                        for (int i = 0; i < usersMousePos.Count; i++)
                        {
                            if (usersMousePos[i].TTL < Environment.TickCount)
                            {
                                usersMousePos.RemoveAt(i);
                                usersMouseTag.RemoveAt(i);
                            }
                        }
                    }
                }
            }
        }

        public override void Render(Graphics g)
        {
            // zeichne die Maus Positionen von anderen wenn online
            if (!game.NetworkHandler.OfflineMode)
            {
                for (int i = 0; i < usersMousePos.Count; i++)
                {
                    g.DrawString(usersMouseTag[i], SystemFonts.DefaultFont, Brushes.Black,
                        usersMousePos[i].X + 15, usersMousePos[i].Y - 5);
                    g.DrawRectangle(Pens.Red, usersMousePos[i].X - 5, usersMousePos[i].Y - 5, 10, 10);
                }
            }
        }

        private void TransferMousePosition(object t)
        {
            var networkPackage = t as NetworkPackageMousePosition;
            if (networkPackage == null // aus irgend einem Grund ist das schon mal passiert und hat zu Null reference Excep. geführt. Wenn sehr viele Events empfangen werden könnte es passieren
                || networkPackage.Username == game.NetworkHandler.LocalPlayer.Name) return;

            bool hasFound = false;
            for (int i = 0; i < usersMousePos.Count; i++)
                if (usersMouseTag[i] == networkPackage.Username)
                {
                    hasFound = true;
                    if (usersMousePos[i].currentTick < networkPackage.CurrentTick)
                        usersMousePos[i] =
                            (networkPackage.Pos.X, networkPackage.Pos.Y, networkPackage.TTL + Environment.TickCount, networkPackage.CurrentTick);
                }
            if (!hasFound)
            {
                usersMousePos.Add(
                    (networkPackage.Pos.X, networkPackage.Pos.Y, networkPackage.TTL + Environment.TickCount, networkPackage.CurrentTick));
                usersMouseTag.Add(networkPackage.Username);
            }
        }

        [Serializable]
        private class NetworkPackageMousePosition
        {
            public (int X, int Y) Pos = (0, 0);
            public string Username = string.Empty;

            public ulong CurrentTick = 0;
            public int TTL = 2000; // wird nicht überschrieben und in ms
        }
    }
}
