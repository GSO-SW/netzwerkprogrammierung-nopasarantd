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
        }

        private void Display_Load(object sender, EventArgs e)
        {
            Engine.INTERNAL.FormDisplay = this;
            if(!Engine.INIT()) throw new Exception("Ooga Booga, something went wrong X(");
            ThreadEngine.RunWorkerAsync();

        }
        private void ThreadEngine_DoWork(object sender, DoWorkEventArgs e)
        {
            Engine.INTERNAL.THREADEDLoop();
        }


        private void Display_Paint(object sender, PaintEventArgs e)
        {
            Engine.INTERNAL.DoDraw(sender, e);
        }

        private void Display_MouseDown(object sender, MouseEventArgs e)
        {

        }

        private void Display_Resize(object sender, EventArgs e)
        {
            Engine.INTERNAL.newResize = true;
        }

        private void TimerCanvasUpdate_Tick(object sender, EventArgs e)
        {
            // TODO: Adjust update time of the timer based on the max fps of the engine

            if (Engine.INTERNAL.DOupdateCanvas)
            {
                Refresh();
                Engine.INTERNAL.DOupdateCanvas = false;
            }
        }
    }
}
