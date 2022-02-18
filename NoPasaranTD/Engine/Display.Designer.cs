namespace NoPasaranTD
{
    partial class Display
    {
        /// <summary>
        /// Erforderliche Designervariable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Verwendete Ressourcen bereinigen.
        /// </summary>
        /// <param name="disposing">True, wenn verwaltete Ressourcen gelöscht werden sollen; andernfalls False.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Vom Windows Form-Designer generierter Code

        /// <summary>
        /// Erforderliche Methode für die Designerunterstützung.
        /// Der Inhalt der Methode darf nicht mit dem Code-Editor geändert werden.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.ThreadEngine = new System.ComponentModel.BackgroundWorker();
            this.TimerCanvasUpdate = new System.Windows.Forms.Timer(this.components);
            this.SuspendLayout();
            // 
            // ThreadEngine
            // 
            this.ThreadEngine.DoWork += new System.ComponentModel.DoWorkEventHandler(this.ThreadEngine_DoWork);
            // 
            // Display
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.DoubleBuffered = true;
            this.Name = "Display";
            this.Text = "Display";
            this.Load += new System.EventHandler(this.Display_Load);
            this.Paint += new System.Windows.Forms.PaintEventHandler(this.Display_Paint);
            this.MouseDown += new System.Windows.Forms.MouseEventHandler(this.Display_MouseDown);
            this.Resize += new System.EventHandler(this.Display_Resize);
            this.ResumeLayout(false);

        }

        #endregion

        private System.ComponentModel.BackgroundWorker ThreadEngine;
        private System.Windows.Forms.Timer TimerCanvasUpdate;
    }
}

