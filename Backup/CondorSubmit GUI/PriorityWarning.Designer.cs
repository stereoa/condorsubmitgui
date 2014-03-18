namespace CondorSubmitGUI
{
    partial class PriorityWarning
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(PriorityWarning));
            this.warningLabel = new System.Windows.Forms.Label();
            this.PriorityCombo = new System.Windows.Forms.ComboBox();
            this.changeButton = new System.Windows.Forms.Button();
            this.continueButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // warningLabel
            // 
            this.warningLabel.AutoSize = true;
            this.warningLabel.Location = new System.Drawing.Point(12, 9);
            this.warningLabel.MaximumSize = new System.Drawing.Size(0, 1000);
            this.warningLabel.Name = "warningLabel";
            this.warningLabel.Size = new System.Drawing.Size(1059, 13);
            this.warningLabel.TabIndex = 0;
            this.warningLabel.Text = resources.GetString("warningLabel.Text");
            // 
            // PriorityCombo
            // 
            this.PriorityCombo.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.PriorityCombo.FormattingEnabled = true;
            this.PriorityCombo.Items.AddRange(new object[] {
            "High",
            "Medium",
            "Low",
            "Lowest"});
            this.PriorityCombo.Location = new System.Drawing.Point(155, 145);
            this.PriorityCombo.Name = "PriorityCombo";
            this.PriorityCombo.Size = new System.Drawing.Size(84, 21);
            this.PriorityCombo.TabIndex = 24;
            // 
            // changeButton
            // 
            this.changeButton.Location = new System.Drawing.Point(38, 145);
            this.changeButton.Name = "changeButton";
            this.changeButton.Size = new System.Drawing.Size(111, 21);
            this.changeButton.TabIndex = 25;
            this.changeButton.Text = "Change priority to:";
            this.changeButton.UseVisualStyleBackColor = true;
            this.changeButton.Click += new System.EventHandler(this.changeButton_Click);
            // 
            // continueButton
            // 
            this.continueButton.Location = new System.Drawing.Point(38, 116);
            this.continueButton.Name = "continueButton";
            this.continueButton.Size = new System.Drawing.Size(201, 23);
            this.continueButton.TabIndex = 26;
            this.continueButton.Text = "Continue submitting, I don\'t care!";
            this.continueButton.UseVisualStyleBackColor = true;
            this.continueButton.Click += new System.EventHandler(this.continueButton_Click);
            // 
            // PriorityWarning
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(292, 175);
            this.ControlBox = false;
            this.Controls.Add(this.continueButton);
            this.Controls.Add(this.changeButton);
            this.Controls.Add(this.PriorityCombo);
            this.Controls.Add(this.warningLabel);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "PriorityWarning";
            this.Text = "Priority Warning!";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label warningLabel;
        private System.Windows.Forms.ComboBox PriorityCombo;
        private System.Windows.Forms.Button changeButton;
        private System.Windows.Forms.Button continueButton;
    }
}