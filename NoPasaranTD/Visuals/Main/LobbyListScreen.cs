﻿using NoPasaranTD.Engine;
using NoPasaranTD.Networking;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace NoPasaranTD.Visuals.Main
{
    public class LobbyListScreen : GuiComponent
    {

        private readonly ListContainer<NetworkLobby, LobbyItemContainer> lobbyList;
        private readonly ButtonContainer btnUpdatePlayer;
        private readonly ButtonContainer btnPlayLocalGame;
        private readonly ButtonContainer btnCreateLobby;

        private readonly GuiMainMenu parent;
        public LobbyListScreen(GuiMainMenu parent)
        {
            this.parent = parent;
            lobbyList = new ListContainer<NetworkLobby, LobbyItemContainer>()
            {
                Margin = 10,
                ItemSize = new Size((int)(StaticEngine.RenderWidth / 1.5f) - 20, 100),

                Position = new Point(StaticEngine.RenderWidth / 6, StaticEngine.RenderHeight / 6 - 20),
                ContainerSize = new Size((int)(StaticEngine.RenderWidth / 1.5f), (int)(StaticEngine.RenderHeight / 1.5f)),
                BackgroundColor = new SolidBrush(Color.FromArgb(225, Color.Gray)),

                Items = new NotifyCollection<NetworkLobby>()
            };
            lobbyList.SelectionChanged += JoinLobby;

            // Aktualisiere Spielerinformationen
            btnUpdatePlayer = GuiMainMenu.CreateButton("Login", new Rectangle(5, 5, 100, 30));

            // TODO: Mit TextBox aktualisieren
            btnUpdatePlayer.ButtonClicked += () => UpdatePlayer(new NetworkClient("SKRR" + Environment.TickCount));

            // Erstelle neue Lobby
            btnCreateLobby = GuiMainMenu.CreateButton("Create Lobby", new Rectangle(
                lobbyList.Position.X + lobbyList.ContainerSize.Width - 305,
                lobbyList.Position.Y + lobbyList.ContainerSize.Height + 5,
                150, 30
            ));
            btnCreateLobby.ButtonClicked += () => CreateLobby(parent.LocalPlayer);

            // Lade privates Spiel
            btnPlayLocalGame = GuiMainMenu.CreateButton("Play Local Game", new Rectangle(
                lobbyList.Position.X + lobbyList.ContainerSize.Width - 150,
                lobbyList.Position.Y + lobbyList.ContainerSize.Height + 5,
                150, 30
            ));
            btnPlayLocalGame.ButtonClicked += () => Program.LoadGame("spentagon");
        }

        /// <summary>
        /// Aktualisiert die liste an Lobbies
        /// </summary>
        /// <param name="lobbies">Die neuen Lobbyinformationen</param>
        public void UpdateLobbies(List<NetworkLobby> lobbies)
        {
            lobbyList.Items.Clear();
            foreach (NetworkLobby lobby in lobbies)
                lobbyList.Items.Add(lobby);
            lobbyList.DefineItems();
        }

        #region Screen events
        private void UpdatePlayer(NetworkClient client)
        {
            if (parent.DiscoveryClient == null) return;

            parent.LocalPlayer = client;
            if (parent.DiscoveryClient.LoggedIn)
                parent.DiscoveryClient.UpdatePlayerAsync(client);
            else
                parent.DiscoveryClient.LoginAsync(client);
            btnUpdatePlayer.Content = "Update Info";
        }

        private void CreateLobby(NetworkClient host)
        {
            // TODO: Change lobby name by textbox
            if (parent.DiscoveryClient == null || !parent.DiscoveryClient.LoggedIn) return;
            parent.DiscoveryClient.CreateLobbyAsync(new NetworkLobby(host, "Lobby Name (By textbox)" + Environment.TickCount));
        }

        private void JoinLobby()
        {
            if (parent.DiscoveryClient == null || !parent.DiscoveryClient.LoggedIn) return;
            parent.DiscoveryClient.JoinLobbyAsync(lobbyList.SelectedItem);
        }
        #endregion

        #region Implementation region
        public override void Update()
        {
            if (!lobbyList.Active) return;
            lobbyList.Update();
        }

        public override void Render(Graphics g)
        {
            if (!Visible) return;
            btnUpdatePlayer.Render(g);
            btnPlayLocalGame.Render(g);
            btnCreateLobby.Render(g);
            lobbyList.Render(g);

            if(parent.LocalPlayer != null)
            {
                string text = "Logged in as: " + parent.LocalPlayer.Name;
                Size size = TextRenderer.MeasureText(text, StandartText1Font);
                g.DrawString(text, StandartText1Font, Brushes.Black, 
                    StaticEngine.RenderWidth - size.Width, 
                    StaticEngine.RenderHeight - size.Height
                );
            }
        }

        public override void KeyUp(KeyEventArgs e)
        {
            if (!Active) return;
            lobbyList.KeyUp(e);
        }

        public override void KeyDown(KeyEventArgs e)
        {
            if (!Active) return;
            lobbyList.KeyUp(e);
        }

        public override void MouseUp(MouseEventArgs e)
        {
            if (!Active) return;
            lobbyList.MouseUp(e);
        }

        public override void MouseDown(MouseEventArgs e)
        {
            if (!Active) return;
            btnUpdatePlayer.MouseDown(e);
            btnPlayLocalGame.MouseDown(e);
            btnCreateLobby.MouseDown(e);
            lobbyList.MouseDown(e);
        }

        public override void MouseMove(MouseEventArgs e)
        {
            if (!Active) return;
            lobbyList.MouseMove(e);
        }

        public override void MouseWheel(MouseEventArgs e)
        {
            if (!Active) return;
            lobbyList.MouseWheel(e);
        }
        #endregion

    }
}
