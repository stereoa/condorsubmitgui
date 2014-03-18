namespace CondorSubmitGUI
{
    partial class EditImagePaths
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(EditImagePaths));
            this.EditIPToTB = new System.Windows.Forms.TextBox();
            this.EditIPFromLabel = new System.Windows.Forms.Label();
            this.EditIPToLabel = new System.Windows.Forms.Label();
            this.EditIPApply = new System.Windows.Forms.Button();
            this.EditIPFromCB = new System.Windows.Forms.ComboBox();
            this.EditIPToBrowse = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // EditIPToTB
            // 
            this.EditIPToTB.Location = new System.Drawing.Point(12, 64);
            this.EditIPToTB.Name = "EditIPToTB";
            this.EditIPToTB.Size = new System.Drawing.Size(234, 20);
            this.EditIPToTB.TabIndex = 1;
            // 
            // EditIPFromLabel
            // 
            this.EditIPFromLabel.AutoSize = true;
            this.EditIPFromLabel.Location = new System.Drawing.Point(12, 7);
            this.EditIPFromLabel.Name = "EditIPFromLabel";
            this.EditIPFromLabel.Size = new System.Drawing.Size(73, 13);
            this.EditIPFromLabel.TabIndex = 2;
            this.EditIPFromLabel.Text = "Change From:";
            // 
            // EditIPToLabel
            // 
            this.EditIPToLabel.AutoSize = true;
            this.EditIPToLabel.Location = new System.Drawing.Point(12, 48);
            this.EditIPToLabel.Name = "EditIPToLabel";
            this.EditIPToLabel.Size = new System.Drawing.Size(63, 13);
            this.EditIPToLabel.TabIndex = 3;
            this.EditIPToLabel.Text = "Change To:";
            // 
            // EditIPApply
            // 
            this.EditIPApply.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.EditIPApply.Location = new System.Drawing.Point(203, 91);
            this.EditIPApply.Name = "EditIPApply";
            this.EditIPApply.Size = new System.Drawing.Size(75, 23);
            this.EditIPApply.TabIndex = 4;
            this.EditIPApply.Text = "Apply";
            this.EditIPApply.UseVisualStyleBackColor = true;
            // 
            // EditIPFromCB
            // 
            this.EditIPFromCB.FormattingEnabled = true;
            this.EditIPFromCB.Location = new System.Drawing.Point(12, 23);
            this.EditIPFromCB.Name = "EditIPFromCB";
            this.EditIPFromCB.Size = new System.Drawing.Size(268, 21);
            this.EditIPFromCB.TabIndex = 5;
            // 
            // EditIPToBrowse
            // 
            this.EditIPToBrowse.Location = new System.Drawing.Point(253, 64);
            this.EditIPToBrowse.Name = "EditIPToBrowse";
            this.EditIPToBrowse.Size = new System.Drawing.Size(25, 20);
            this.EditIPToBrowse.TabIndex = 6;
            this.EditIPToBrowse.Text = "...";
            this.EditIPToBrowse.UseVisualStyleBackColor = true;
            this.EditIPToBrowse.Click += new System.EventHandler(this.EditIPToBrowse_Click);
            // 
            // EditImagePaths
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(292, 120);
            this.Controls.Add(this.EditIPToBrowse);
            this.Controls.Add(this.EditIPFromCB);
            this.Controls.Add(this.EditIPApply);
            this.Controls.Add(this.EditIPToLabel);
            this.Controls.Add(this.EditIPFromLabel);
            this.Controls.Add(this.EditIPToTB);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "EditImagePaths";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Edit Image Paths";
            this.ResumeLayout(false);
            this.PerformLayout();
            //
            
            

        }

        #endregion

        private System.Windows.Forms.Label EditIPFromLabel;
        private System.Windows.Forms.Label EditIPToLabel;
        private System.Windows.Forms.Button EditIPApply;
        public System.Windows.Forms.TextBox EditIPToTB;
        public System.Windows.Forms.ComboBox EditIPFromCB;
        private System.Windows.Forms.Button EditIPToBrowse;
    }
}