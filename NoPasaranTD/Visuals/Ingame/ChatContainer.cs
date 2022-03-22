using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NoPasaranTD.Visuals.Ingame
{
    public class ChatContainer : GuiComponent
    {
        private ButtonContainer sendButton = new ButtonContainer()
        {
            Content = "Send",
            Background = new SolidBrush(Color.FromArgb(132, 140, 156)),
            BorderBrush = new SolidBrush(Color.FromArgb(108, 113, 122)),
            Margin = 1,
            StringFont = StandartHeader2Font,
        };

        private TextBoxContainer inputBox = new TextBoxContainer()
        {
            Text = "Type Message...",
            Background = new SolidBrush(Color.White),
            BorderBrush = new SolidBrush(Color.FromArgb(108, 113, 122)),

        };

        public ListContainer<string, ChatItem> ChatHistory{ get; set; } = new ListContainer<string, ChatItem>()
        {

        };
       
        public ChatContainer()
        {
            ChatHistory.Items.CollectionChanged += Items_CollectionChanged;
        }

        private void Items_CollectionChanged()
        {
            throw new NotImplementedException();
        }
    }
}
