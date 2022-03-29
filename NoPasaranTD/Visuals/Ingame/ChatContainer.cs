using NoPasaranTD.Data;
using NoPasaranTD.Logic;
using System.Drawing;
using System.Windows.Forms;

namespace NoPasaranTD.Visuals.Ingame
{
    public class ChatContainer : GuiComponent
    {
        #region UI Region
        public SolidBrush Background { get; set; }

        // Textbox zur Eingabe von Nachrichten
        private readonly TextBoxContainer inputBox = new TextBoxContainer()
        {
            Background = new SolidBrush(Color.White),
            Foreground = new SolidBrush(Color.Black),
            BorderBrush = new SolidBrush(Color.Black),
            Margin = 2,
            TextFont = StandartText2Font
        };

        // Container für die Aufzählung der Nachrichtenobjekte
        private ListContainer<string, ChatItemContainer> chatObjects;

        #endregion

        // Das derzeitige Spiel
        private Game currentGame;

        private long lastSendAt = 0;

        /// <summary>
        /// Initialisiert alle UI Elemente
        /// </summary>
        /// <param name="game"></param>
        public void Init(Game game)
        {
            currentGame = game;
            currentGame.Messages.CollectionChanged += Messages_CollectionChanged;

            chatObjects = new ListContainer<string, ChatItemContainer>()
            {
                ContainerSize = new System.Drawing.Size(Bounds.Width - 6, Bounds.Height - 36),
                BackgroundColor = new SolidBrush(Color.Transparent),
                Orientation = System.Windows.Forms.Orientation.Vertical,
                ItemSize = new System.Drawing.Size(150, 30),
                Position = new System.Drawing.Point(Bounds.X + 3, Bounds.Y + 30),
                Margin = 2,
            };

            inputBox.Bounds = new Rectangle(Bounds.X + 3, Bounds.Y + Bounds.Height - 36, Bounds.Width - 6, 30);
            chatObjects.Bounds = new Rectangle(Bounds.X + 3, Bounds.Y + 30, Bounds.Width - 6, Bounds.Height - 75);
        }

        #region Event Methodes

        // Sobald die Liste der Nachrichten in der Game Instanz verändert wird, sollen die Items neu definiert werden
        private void Messages_CollectionChanged()
        {
            chatObjects.Items = currentGame.Messages;
        }

        #endregion
        #region Engine Methodes

        public override void KeyDown(KeyEventArgs e)
        {
            inputBox.KeyDown(e);
            chatObjects.KeyDown(e);

            if (e.KeyCode == Keys.Enter && inputBox.IsFocused && !currentGame.NetworkHandler.OfflineMode && currentGame.CurrentTick - lastSendAt >= 500)
            {
                lastSendAt = currentGame.CurrentTick;
                currentGame.NetworkHandler.InvokeEvent("SendMessage", "[" + currentGame.NetworkHandler.LocalPlayer.Name + "]\n" + inputBox.Text);
                inputBox.Text = "";
                inputBox.CaretIndex = 0;
            }
        }

        public override void KeyPress(KeyPressEventArgs e)
        {
            inputBox.KeyPress(e);
            chatObjects.KeyPress(e);
        }

        public override void Render(Graphics g)
        {
            if (Visible)
            {
                g.FillRectangle(Background, Bounds);
                g.DrawString("Chat", StandartHeader2Font, Brushes.Black, Bounds.X + 5, Bounds.Y + 5);

                inputBox.Render(g);
                chatObjects.Render(g);
            }
        }

        public override void Update()
        {
            inputBox.Update();
            chatObjects.Update();
        }

        public override void MouseMove(MouseEventArgs e)
        {
            inputBox.MouseMove(e);
            chatObjects.MouseMove(e);
        }

        public override void MouseDown(MouseEventArgs e)
        {
            inputBox.MouseDown(e);
            chatObjects.MouseDown(e);
        }

        public override void MouseWheel(MouseEventArgs e)
        {
            inputBox.MouseWheel(e);
            chatObjects.MouseWheel(e);
        }

        #endregion
    }
}
