namespace CondorSubmitGUI
{
    partial class CondorQueue
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(CondorQueue));
            this.QueueListView = new System.Windows.Forms.ListView();
            this.QueueClusterId = new System.Windows.Forms.ColumnHeader();
            this.QueueJobName = new System.Windows.Forms.ColumnHeader();
            this.QueueStatusColumn = new System.Windows.Forms.ColumnHeader();
            this.QueuePriorityColmun = new System.Windows.Forms.ColumnHeader();
            this.QueueNodeColumn = new System.Windows.Forms.ColumnHeader();
            this.QueueRefreshButton = new System.Windows.Forms.Button();
            this.QueueHoldButton = new System.Windows.Forms.Button();
            this.QueueRemoveButton = new System.Windows.Forms.Button();
            this.QueueSetPriorityButton = new System.Windows.Forms.Button();
            this.QueuePriorityCombo = new System.Windows.Forms.ComboBox();
            this.QueueNumber = new System.Windows.Forms.Label();
            this.QueueProgressBar = new System.Windows.Forms.ProgressBar();
            this.QueueProcessBW = new System.ComponentModel.BackgroundWorker();
            this.QueueSelectCount = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // QueueListView
            // 
            this.QueueListView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.QueueClusterId,
            this.QueueJobName,
            this.QueueStatusColumn,
            this.QueuePriorityColmun,
            this.QueueNodeColumn});
            this.QueueListView.FullRowSelect = true;
            this.QueueListView.HideSelection = false;
            this.QueueListView.Location = new System.Drawing.Point(13, 13);
            this.QueueListView.Name = "QueueListView";
            this.QueueListView.Size = new System.Drawing.Size(685, 425);
            this.QueueListView.TabIndex = 0;
            this.QueueListView.UseCompatibleStateImageBehavior = false;
            this.QueueListView.View = System.Windows.Forms.View.Details;
            this.QueueListView.SelectedIndexChanged += new System.EventHandler(this.QueueListView_SelectedIndexChanged);
            this.QueueListView.DoubleClick += new System.EventHandler(this.QueueListView_DoubleClick);
            this.QueueListView.ColumnClick += new System.Windows.Forms.ColumnClickEventHandler(this.QueueListView_ColumnClick);
            this.QueueListView.KeyDown += new System.Windows.Forms.KeyEventHandler(this.QueueListView_KeyDown);
            // 
            // QueueClusterId
            // 
            this.QueueClusterId.Text = "ID";
            this.QueueClusterId.Width = 62;
            // 
            // QueueJobName
            // 
            this.QueueJobName.Text = "Job Name";
            this.QueueJobName.Width = 289;
            // 
            // QueueStatusColumn
            // 
            this.QueueStatusColumn.Text = "Status";
            this.QueueStatusColumn.Width = 77;
            // 
            // QueuePriorityColmun
            // 
            this.QueuePriorityColmun.Text = "Priority";
            this.QueuePriorityColmun.Width = 68;
            // 
            // QueueNodeColumn
            // 
            this.QueueNodeColumn.Text = "Node";
            this.QueueNodeColumn.Width = 187;
            // 
            // QueueRefreshButton
            // 
            this.QueueRefreshButton.Location = new System.Drawing.Point(13, 449);
            this.QueueRefreshButton.Name = "QueueRefreshButton";
            this.QueueRefreshButton.Size = new System.Drawing.Size(75, 23);
            this.QueueRefreshButton.TabIndex = 1;
            this.QueueRefreshButton.Text = "Refresh";
            this.QueueRefreshButton.UseVisualStyleBackColor = true;
            this.QueueRefreshButton.Click += new System.EventHandler(this.QueueRefreshButton_Click);
            // 
            // QueueHoldButton
            // 
            this.QueueHoldButton.Location = new System.Drawing.Point(94, 449);
            this.QueueHoldButton.Name = "QueueHoldButton";
            this.QueueHoldButton.Size = new System.Drawing.Size(101, 23);
            this.QueueHoldButton.TabIndex = 2;
            this.QueueHoldButton.Text = "Hold/Release";
            this.QueueHoldButton.UseVisualStyleBackColor = true;
            this.QueueHoldButton.Click += new System.EventHandler(this.QueueHoldButton_Click);
            // 
            // QueueRemoveButton
            // 
            this.QueueRemoveButton.Location = new System.Drawing.Point(201, 449);
            this.QueueRemoveButton.Name = "QueueRemoveButton";
            this.QueueRemoveButton.Size = new System.Drawing.Size(64, 23);
            this.QueueRemoveButton.TabIndex = 3;
            this.QueueRemoveButton.Text = "Remove";
            this.QueueRemoveButton.UseVisualStyleBackColor = true;
            this.QueueRemoveButton.Click += new System.EventHandler(this.QueueRemoveButton_Click);
            // 
            // QueueSetPriorityButton
            // 
            this.QueueSetPriorityButton.Location = new System.Drawing.Point(535, 449);
            this.QueueSetPriorityButton.Name = "QueueSetPriorityButton";
            this.QueueSetPriorityButton.Size = new System.Drawing.Size(75, 23);
            this.QueueSetPriorityButton.TabIndex = 5;
            this.QueueSetPriorityButton.Text = "Set Priority";
            this.QueueSetPriorityButton.UseVisualStyleBackColor = true;
            this.QueueSetPriorityButton.Click += new System.EventHandler(this.QueueSetPriorityButton_Click);
            // 
            // QueuePriorityCombo
            // 
            this.QueuePriorityCombo.FormattingEnabled = true;
            this.QueuePriorityCombo.Items.AddRange(new object[] {
            "Highest",
            "High",
            "Medium",
            "Low",
            "Lowest"});
            this.QueuePriorityCombo.Location = new System.Drawing.Point(614, 449);
            this.QueuePriorityCombo.Name = "QueuePriorityCombo";
            this.QueuePriorityCombo.Size = new System.Drawing.Size(82, 21);
            this.QueuePriorityCombo.TabIndex = 6;
            // 
            // QueueNumber
            // 
            this.QueueNumber.AutoSize = true;
            this.QueueNumber.Location = new System.Drawing.Point(281, 453);
            this.QueueNumber.Name = "QueueNumber";
            this.QueueNumber.Size = new System.Drawing.Size(67, 13);
            this.QueueNumber.TabIndex = 7;
            this.QueueNumber.Text = "Job Count: 0";
            // 
            // QueueProgressBar
            // 
            this.QueueProgressBar.Location = new System.Drawing.Point(13, 478);
            this.QueueProgressBar.Name = "QueueProgressBar";
            this.QueueProgressBar.Size = new System.Drawing.Size(683, 23);
            this.QueueProgressBar.TabIndex = 8;
            // 
            // QueueProcessBW
            // 
            this.QueueProcessBW.WorkerReportsProgress = true;
            this.QueueProcessBW.WorkerSupportsCancellation = true;
            this.QueueProcessBW.DoWork += new System.ComponentModel.DoWorkEventHandler(this.QueueProcessBW_DoWork);
            this.QueueProcessBW.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.QueueProcessBW_RunWorkerCompleted);
            this.QueueProcessBW.ProgressChanged += new System.ComponentModel.ProgressChangedEventHandler(this.QueueProcessBW_ProgressChanged);
            // 
            // QueueSelectCount
            // 
            this.QueueSelectCount.AutoSize = true;
            this.QueueSelectCount.Location = new System.Drawing.Point(383, 453);
            this.QueueSelectCount.Name = "QueueSelectCount";
            this.QueueSelectCount.Size = new System.Drawing.Size(56, 13);
            this.QueueSelectCount.TabIndex = 9;
            this.QueueSelectCount.Text = "0 selected";
            // 
            // CondorQueue
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(708, 510);
            this.Controls.Add(this.QueueSelectCount);
            this.Controls.Add(this.QueueProgressBar);
            this.Controls.Add(this.QueueNumber);
            this.Controls.Add(this.QueuePriorityCombo);
            this.Controls.Add(this.QueueSetPriorityButton);
            this.Controls.Add(this.QueueRemoveButton);
            this.Controls.Add(this.QueueHoldButton);
            this.Controls.Add(this.QueueRefreshButton);
            this.Controls.Add(this.QueueListView);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "CondorQueue";
            this.Text = "Queue";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ListView QueueListView;
        private System.Windows.Forms.Button QueueRefreshButton;
        private System.Windows.Forms.Button QueueHoldButton;
        private System.Windows.Forms.Button QueueRemoveButton;
        private System.Windows.Forms.ColumnHeader QueueStatusColumn;
        private System.Windows.Forms.ColumnHeader QueuePriorityColmun;
        private System.Windows.Forms.Button QueueSetPriorityButton;
        private System.Windows.Forms.ComboBox QueuePriorityCombo;
        private System.Windows.Forms.ColumnHeader QueueClusterId;
        private System.Windows.Forms.Label QueueNumber;
        private System.Windows.Forms.ProgressBar QueueProgressBar;
        private System.ComponentModel.BackgroundWorker QueueProcessBW;
        private System.Windows.Forms.Label QueueSelectCount;
        private System.Windows.Forms.ColumnHeader QueueNodeColumn;
        private System.Windows.Forms.ColumnHeader QueueJobName;
    }
}