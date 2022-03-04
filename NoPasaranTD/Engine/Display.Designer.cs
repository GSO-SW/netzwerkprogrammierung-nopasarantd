namespace NoPasaranTD.Engine
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
            this.tmrGameUpdate = new System.Windows.Forms.Timer(this.components);
            this.SuspendLayout();
            // 
            // tmrGameUpdate
            // 
            this.tmrGameUpdate.Enabled = true;
            this.tmrGameUpdate.Interval = 1;
            this.tmrGameUpdate.Tick += new System.EventHandler(this.tmrGameUpdate_Tick);
            // 
            // Display
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.DoubleBuffered = true;
            this.Name = "Display";
            this.ShowIcon = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "NoPasaranTD";
            this.Paint += new System.Windows.Forms.PaintEventHandler(this.Display_Paint);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.Display_KeyDown);
            this.KeyUp += new System.Windows.Forms.KeyEventHandler(this.Display_KeyUp);
            this.MouseDown += new System.Windows.Forms.MouseEventHandler(this.Display_MouseDown);
            this.MouseMove += new System.Windows.Forms.MouseEventHandler(this.Display_MouseMove);
            this.MouseUp += new System.Windows.Forms.MouseEventHandler(this.Display_MouseUp);
            this.MouseWheel += new System.Windows.Forms.MouseEventHandler(this.Display_MouseWheel);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Timer tmrGameUpdate;
    }
}

