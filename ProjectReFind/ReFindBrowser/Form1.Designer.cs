namespace ReFindBrowser
{
    partial class Form1
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

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.webRefind = new System.Windows.Forms.WebBrowser();
            this.txtBar = new System.Windows.Forms.TextBox();
            this.btnGo = new System.Windows.Forms.Button();
            this.lblLoad = new System.Windows.Forms.Label();
            this.txtOutputText = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // webRefind
            // 
            this.webRefind.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.webRefind.Location = new System.Drawing.Point(12, 39);
            this.webRefind.MinimumSize = new System.Drawing.Size(20, 20);
            this.webRefind.Name = "webRefind";
            this.webRefind.Size = new System.Drawing.Size(807, 299);
            this.webRefind.TabIndex = 0;
            // 
            // txtBar
            // 
            this.txtBar.Location = new System.Drawing.Point(12, 12);
            this.txtBar.Name = "txtBar";
            this.txtBar.Size = new System.Drawing.Size(726, 20);
            this.txtBar.TabIndex = 1;
            // 
            // btnGo
            // 
            this.btnGo.Location = new System.Drawing.Point(744, 10);
            this.btnGo.Name = "btnGo";
            this.btnGo.Size = new System.Drawing.Size(75, 23);
            this.btnGo.TabIndex = 2;
            this.btnGo.Text = "Go Go Go!!!";
            this.btnGo.UseVisualStyleBackColor = true;
            this.btnGo.Click += new System.EventHandler(this.btnGo_Click);
            // 
            // lblLoad
            // 
            this.lblLoad.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.lblLoad.AutoSize = true;
            this.lblLoad.Location = new System.Drawing.Point(9, 607);
            this.lblLoad.Name = "lblLoad";
            this.lblLoad.Size = new System.Drawing.Size(0, 13);
            this.lblLoad.TabIndex = 3;
            // 
            // txtOutputText
            // 
            this.txtOutputText.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtOutputText.Location = new System.Drawing.Point(12, 344);
            this.txtOutputText.Multiline = true;
            this.txtOutputText.Name = "txtOutputText";
            this.txtOutputText.ReadOnly = true;
            this.txtOutputText.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtOutputText.Size = new System.Drawing.Size(807, 244);
            this.txtOutputText.TabIndex = 4;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(831, 629);
            this.Controls.Add(this.txtOutputText);
            this.Controls.Add(this.lblLoad);
            this.Controls.Add(this.btnGo);
            this.Controls.Add(this.txtBar);
            this.Controls.Add(this.webRefind);
            this.Name = "Form1";
            this.Text = "ReFind Browser";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.WebBrowser webRefind;
        private System.Windows.Forms.TextBox txtBar;
        private System.Windows.Forms.Button btnGo;
        private System.Windows.Forms.Label lblLoad;
        private System.Windows.Forms.TextBox txtOutputText;
    }
}

