using NoPasaranTD.Engine;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NoPasaranTD.Visuals.Ingame
{
    public class ChatContainer : GuiComponent
    {
        #region UI Region
        public SolidBrush Background { get; set; }

        private TextBoxContainer inputBox = new TextBoxContainer()
        {
            Background = new SolidBrush(Color.White),
            Foreground = new SolidBrush(Color.Black),     
            BorderBrush = new SolidBrush(Color.Black),
            Margin = 2,
            TextFont = StandartText2Font
        };
        private ListContainer<string, ChatItemContainer> chatObjects;

        #endregion

        private Game currentGame;

        public void Init(Game game)
        {                      
            currentGame = game;
            currentGame.Messages.CollectionChanged += Messages_CollectionChanged;

            chatObjects = new ListContainer<string,ChatItemContainer>()
            {
                ContainerSize = new System.Drawing.Size(Bounds.Width - 6, Bounds.Height - 36),
                BackgroundColor = new SolidBrush(Color.Transparent),
                Orientation = System.Windows.Forms.Orientation.Vertical,
                ItemSize = new System.Drawing.Size(100, 50),
                Position = new System.Drawing.Point(Bounds.X + 3, Bounds.Y + 3),
                Margin = 2,
            };

            inputBox.Bounds = new Rectangle(Bounds.X + 3, Bounds.Y + Bounds.Height - 36, Bounds.Width - 6, 30);
            chatObjects.Bounds = new Rectangle(Bounds.X + 3, Bounds.Y + 3, Bounds.Width - 6, Bounds.Height - 36);
            chatObjects.DefineItems();
        }

        private void Messages_CollectionChanged()
        {
            chatObjects.Items = currentGame.Messages;
            chatObjects.DefineItems();
        }

        public override void KeyDown(KeyEventArgs e)
        {
            inputBox.KeyDown(e);
            chatObjects.KeyDown(e);

            if (e.KeyCode == Keys.Enter && inputBox.IsFocused)
                currentGame.NetworkHandler.InvokeEvent("SendMessage", inputBox.Text);               
        }

        public override void KeyPress(KeyPressEventArgs e)
        {
            inputBox.KeyPress(e);
            chatObjects.KeyPress(e);
        }

        public override void Render(Graphics g)
        {           
            g.FillRectangle(Background, Bounds);

            inputBox.Render(g);
            chatObjects.Render(g);
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
    }
}
