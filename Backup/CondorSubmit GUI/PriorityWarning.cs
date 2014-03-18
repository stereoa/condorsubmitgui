using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace CondorSubmitGUI
{
    public partial class PriorityWarning : Form
    {
        private CondorSubmitGUI MainForm = new CondorSubmitGUI();
        
        public PriorityWarning(CondorSubmitGUI MainForm)
        {
            this.MainForm = MainForm;
            InitializeComponent();
            this.warningLabel.Text = "You are currently submitting a large amount of jobs with\nthe highest priority. This will effectively lock out any\nsmall jobs that need to be run quickly for a long period\nof time until your jobs are finished.\n\n                     What would you like to do?";
        }

        private void changeButton_Click(object sender, EventArgs e)
        {
            MainForm.PriorityCombo.Text = this.PriorityCombo.Text;
            this.Close();
            
        }

        private void continueButton_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
