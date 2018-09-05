namespace AxBQEV23K
{
    partial class AxBQEV23KControl
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(AxBQEV23KControl));
            this.axBq80xRW1 = new AxBQ80XRWLib.AxBq80xRW();
            ((System.ComponentModel.ISupportInitialize)(this.axBq80xRW1)).BeginInit();
            this.SuspendLayout();
            // 
            // axBq80xRW1
            // 
            this.axBq80xRW1.Enabled = true;
            this.axBq80xRW1.Location = new System.Drawing.Point(0, 0);
            this.axBq80xRW1.Name = "axBq80xRW1";
            this.axBq80xRW1.OcxState = ((System.Windows.Forms.AxHost.State)(resources.GetObject("axBq80xRW1.OcxState")));
            this.axBq80xRW1.Size = new System.Drawing.Size(100, 50);
            this.axBq80xRW1.TabIndex = 0;
            // 
            // AxBQEV23KControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.axBq80xRW1);
            this.Name = "AxBQEV23KControl";
            ((System.ComponentModel.ISupportInitialize)(this.axBq80xRW1)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private AxBQ80XRWLib.AxBq80xRW axBq80xRW1;
    }
}
