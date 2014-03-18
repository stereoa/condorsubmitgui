namespace CondorSubmitGUI
{
    partial class JobStatus
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
            this.outputTB = new System.Windows.Forms.TextBox();
            this.eventsLV = new System.Windows.Forms.ListView();
            this.timeColumn = new System.Windows.Forms.ColumnHeader();
            this.eventColumn = new System.Windows.Forms.ColumnHeader();
            this.errorTB = new System.Windows.Forms.TextBox();
            this.eventsLabel = new System.Windows.Forms.Label();
            this.outputLabel = new System.Windows.Forms.Label();
            this.errorLabel = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // outputTB
            // 
            this.outputTB.Location = new System.Drawing.Point(12, 190);
            this.outputTB.Multiline = true;
            this.outputTB.Name = "outputTB";
            this.outputTB.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.outputTB.Size = new System.Drawing.Size(372, 157);
            this.outputTB.TabIndex = 0;
            // 
            // eventsLV
            // 
            this.eventsLV.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.timeColumn,
            this.eventColumn});
            this.eventsLV.Location = new System.Drawing.Point(12, 25);
            this.eventsLV.Name = "eventsLV";
            this.eventsLV.Size = new System.Drawing.Size(372, 146);
            this.eventsLV.TabIndex = 1;
            this.eventsLV.UseCompatibleStateImageBehavior = false;
            this.eventsLV.View = System.Windows.Forms.View.Details;
            // 
            // timeColumn
            // 
            this.timeColumn.Text = "Time";
            this.timeColumn.Width = 136;
            // 
            // eventColumn
            // 
            this.eventColumn.Text = "Event";
            this.eventColumn.Width = 232;
            // 
            // errorTB
            // 
            this.errorTB.Location = new System.Drawing.Point(12, 366);
            this.errorTB.Multiline = true;
            this.errorTB.Name = "errorTB";
            this.errorTB.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.errorTB.Size = new System.Drawing.Size(372, 157);
            this.errorTB.TabIndex = 2;
            // 
            // eventsLabel
            // 
            this.eventsLabel.AutoSize = true;
            this.eventsLabel.Location = new System.Drawing.Point(9, 9);
            this.eventsLabel.Name = "eventsLabel";
            this.eventsLabel.Size = new System.Drawing.Size(43, 13);
            this.eventsLabel.TabIndex = 3;
            this.eventsLabel.Text = "Events:";
            // 
            // outputLabel
            // 
            this.outputLabel.AutoSize = true;
            this.outputLabel.Location = new System.Drawing.Point(9, 174);
            this.outputLabel.Name = "outputLabel";
            this.outputLabel.Size = new System.Drawing.Size(42, 13);
            this.outputLabel.TabIndex = 4;
            this.outputLabel.Text = "Output:";
            // 
            // errorLabel
            // 
            this.errorLabel.AutoSize = true;
            this.errorLabel.Location = new System.Drawing.Point(12, 350);
            this.errorLabel.Name = "errorLabel";
            this.errorLabel.Size = new System.Drawing.Size(32, 13);
            this.errorLabel.TabIndex = 5;
            this.errorLabel.Text = "Error:";
            // 
            // JobStatus
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(396, 536);
            this.Controls.Add(this.errorLabel);
            this.Controls.Add(this.outputLabel);
            this.Controls.Add(this.eventsLabel);
            this.Controls.Add(this.errorTB);
            this.Controls.Add(this.eventsLV);
            this.Controls.Add(this.outputTB);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "JobStatus";
            this.Text = "JobStatus";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox outputTB;
        private System.Windows.Forms.ListView eventsLV;
        private System.Windows.Forms.TextBox errorTB;
        private System.Windows.Forms.Label eventsLabel;
        private System.Windows.Forms.Label outputLabel;
        private System.Windows.Forms.Label errorLabel;
        private System.Windows.Forms.ColumnHeader timeColumn;
        private System.Windows.Forms.ColumnHeader eventColumn;
    }
}