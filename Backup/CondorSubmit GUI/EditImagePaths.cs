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
    public partial class EditImagePaths : Form
    {
        // Stupid hack to make an instance of the Main form, so that you can use the GetUniversalName method. 
        // Didn't want to have duplicate methods. Meh.
        private CondorSubmitGUI MainParent = new CondorSubmitGUI();
        
        public EditImagePaths()
        {
            InitializeComponent();
        }

        public EditImagePaths(CondorSubmitGUI MainForm)
        {
            MainParent = MainForm; 
            InitializeComponent();
        }

        
        private void EditIPToBrowse_Click(object sender, EventArgs e)
        {
            // Displays an OpenFileDialog 
            FolderBrowserDialog openFolderDialog = new FolderBrowserDialog();
            openFolderDialog.Description = "Select a folder";

            // Show the Dialog.
            if (openFolderDialog.ShowDialog() == DialogResult.OK)
            {
                EditIPToTB.Text = MainParent.GetUniversalName(openFolderDialog.SelectedPath);
            }
        }
    }
}
