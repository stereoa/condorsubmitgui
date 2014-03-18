using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.OleDb;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using CondorSubmitGUI.Objects.Ortho;
using CondorSubmitGUI.Objects.Geometry;
namespace CondorSubmitGUI
{

    public partial class CondorSubmitGUI : Form
    {
        #region Global Variables + Constructor
        [DllImport("mpr.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern int WNetGetConnection(
            [MarshalAs(UnmanagedType.LPTStr)] string localName,
            [MarshalAs(UnmanagedType.LPTStr)] StringBuilder remoteName,
            ref int length);

        //Current visible panel
        Panel CurrentPanel = new Panel();

        //Radiance preview window (Seperate form instance)
        RadiancePreview RPForm;

        //Sorter for listviews
        private ListViewColumnSorter lvwColumnSorter;

        //AT Global variables
        ATProject currentATProject;
        bool submitAT = false;

        public CondorSubmitGUI()
        {
            InitializeComponent();
            AppDomain currentDomain = AppDomain.CurrentDomain;
            currentDomain.UnhandledException += new UnhandledExceptionEventHandler(GlobalExceptionHandler);

            PriorityCombo.SelectedIndex = 2;
            ProcessCombo.SelectedIndex = 0;
            RectifySpacingComboBox.SelectedIndex = 7;
            ProgVersionLabel.Text = String.Format("CondorSubmit GUI v{0}.{1}", Assembly.GetExecutingAssembly().GetName().Version.Major.ToString(), Assembly.GetExecutingAssembly().GetName().Version.Minor.ToString());
            CurrentPanel = ATPanel;
            CurrentPanel.Visible = true;
            CurrentPanel.Enabled = true;
            // Create an instance of a ListView column sorter and assign it 
            // to the ListView control.
            lvwColumnSorter = new ListViewColumnSorter();
            ATBlockListView.ListViewItemSorter = lvwColumnSorter;
        }

        private void Main_Load(object sender, EventArgs e)
        {

        }
        #endregion

        #region Panels

        #region AT Panel

        private void ATISPMProjectBrowse_Click(object sender, EventArgs e)
        {
            // Displays an OpenFileDialog so the user can select a Cursor.
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "ISPM Project Files|project";
            openFileDialog.Title = "Select a ISPM Project";

            // Show the Dialog.
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                ATISPMProjectTB.Text = GetUniversalName(openFileDialog.FileName);
                currentATProject = new ATProject(ATISPMProjectTB.Text.Substring(0, ATISPMProjectTB.Text.Length - 8) + @"\photo");
                ATLoadProject(currentATProject);
            }
        }

        private void ATLoadProject(ATProject currentATProject)
        {
            ATBlockListView.Items.Clear();
            foreach (ATBlock block in currentATProject.atBlocks)
            {
                if (!block.blockName.Contains("_Bundle"))
                {
                    ListViewItem item = new ListViewItem(block.blockName);
                    ATBlockListView.Items.Add(item);
                }
            }
            ATGetBlockStatus();
            ProjectTB.Text = new DirectoryInfo(currentATProject.atDirectory).Name;
        }

        private void ATISPMProjectTB_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Return)
            {
                ATISPMProjectTB.Text = GetUniversalName(ATISPMProjectTB.Text);
                currentATProject = new ATProject(ATISPMProjectTB.Text.Substring(0, ATISPMProjectTB.Text.Length - 8) + @"\photo");
                ATLoadProject(currentATProject);
            }
        }

        private void ATBlockListView_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.A && e.Control)
            {
                foreach (ListViewItem item in ATBlockListView.Items)
                {
                    item.Selected = true;
                }
            }
        }

        private void ATMergeButton_Click(object sender, EventArgs e)
        {
            List<string> ATBlocks = new List<string>();
            foreach (ListViewItem item in ATBlockListView.SelectedItems)
            {
                //check to make sure its not already merged
                if (item.SubItems[1].Text.Contains("Finished"))
                {
                    ATBlocks.Add(item.Text);
                }
            }

            StatusLabel.Text = "Merging..";
            MergeATBW.RunWorkerAsync(ATBlocks);
        }

        private void ATRefreshButton_Click(object sender, EventArgs e)
        {
            ATGetBlockStatus();
        }

        private void ATBlockListView_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            SortListView((ListView)sender, e);
        }

        private void ATCreateBlocks_Click(object sender, EventArgs e)
        {
            List<string> ATBlocks = new List<string>();
            foreach (ListViewItem item in ATBlockListView.SelectedItems)
            {
                ATBlocks.Add(item.Text);
            }
            PrepATBW.RunWorkerAsync(ATBlocks);
            StatusLabel.Text = "Preparing..";

        }

        private void ATGetBlockStatus()
        {
            try
            {
                using (StreamReader sr = new StreamReader(currentATProject.atDirectory + @"\BLOCKS.ini"))
                {
                    string text = null;
                    while (sr.Peek() >= 0)
                    {
                        text = sr.ReadLine();
                        // find a block status "block" and parse out info
                        if (text.Contains("AATBlocks"))
                        {
                            string block = text.Substring(11, text.Length - 12);
                            int blockindex = ATBlockListView.Items.IndexOf(ATBlockListView.FindItemWithText(block, false, 0, false));
                            //make sure that it returned an actual zero and not a fail
                            if ((blockindex == 0) && (ATBlockListView.Items[0].Text.Equals(block)) || (blockindex > 0))
                            {
                                text = sr.ReadLine();
                                string state = text.Substring(6, text.Length - 6);
                                ListViewItem item = new ListViewItem();
                                item.Text = block;
                                item.SubItems.Add(state);
                                ATBlockListView.Items[blockindex] = item;
                            }
                        }
                    }
                }
                foreach (ListViewItem item in ATBlockListView.Items)
                {
                    if (item.SubItems.Count != 2)
                    {
                        item.SubItems.Add("Unprocessed");
                    }
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }

        #endregion

        #region Grid Points Panel

        private void GridPointsInputBrowse_Click(object sender, EventArgs e)
        {
            // Displays an OpenFileDialog 
            FolderBrowserDialog openFolderDialog = new FolderBrowserDialog();
            openFolderDialog.Description = "Select an input folder";

            // Show the Dialog.
            if (openFolderDialog.ShowDialog() == DialogResult.OK)
            {
                GridPointsInputTB.Text = GetUniversalName(openFolderDialog.SelectedPath);
                GridPointsOutputTB.Text = GridPointsInputTB.Text + @"\Gridded_TXT";
            }
        }

        private void GridPointsOutputBrowse_Click(object sender, EventArgs e)
        {
            // Displays an OpenFileDialog 
            FolderBrowserDialog openFolderDialog = new FolderBrowserDialog();
            openFolderDialog.Description = "Select an output folder";

            // Show the Dialog.
            if (openFolderDialog.ShowDialog() == DialogResult.OK)
            {
                GridPointsOutputTB.Text = GetUniversalName(openFolderDialog.SelectedPath);
            }
        }

        private void GridPointsBoundaryBrowse_Click(object sender, EventArgs e)
        {
            // Displays an OpenFileDialog so the user can select a Cursor.
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Shapefiles|*.shp";
            openFileDialog.Title = "Select a boundary file for the grid";

            // Show the Dialog.
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                GridPointsBoundaryTB.Text = GetUniversalName(openFileDialog.FileName);
            }
        }

        private void GridPointsInputTB_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Return)
            {
                GridPointsInputTB.Text = GetUniversalName(GridPointsInputTB.Text);
                GridPointsOutputTB.Text = GridPointsInputTB.Text + @"\Gridded_TXT";
            }
        }

        private void GridPointsOutputTB_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Return)
            {
                GridPointsOutputTB.Text = GetUniversalName(GridPointsOutputTB.Text);
            }
        }

        private void GridPointsBoundaryTB_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Return)
            {
                GridPointsBoundaryTB.Text = GetUniversalName(GridPointsBoundaryTB.Text);
            }
        }

        #endregion

        #region Mosaic Panel
        private void MosaicOPJBrowse_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "OrthoPro Project Files|*.opj";
            openFileDialog.Title = "Select an OrthoPro Project";

            // Show the Dialog.
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                MosaicOPJTB.Text = GetUniversalName(openFileDialog.FileName);
                ReadOPJ();
            }
        }

        private void MosaicOPJTB_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Return)
            {
                if (!File.Exists(MosaicOPJTB.Text))
                {
                    MessageBox.Show("File Not Found");
                }
                else
                {
                    MosaicOPJTB.Text = GetUniversalName(MosaicOPJTB.Text);
                    ReadOPJ();
                }
            }
        }

        private void MosaicListView_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.A && e.Control)
            {
                foreach (ListViewItem item in MosaicListView.Items)
                {
                    item.Selected = true;
                }
            }
        }

        private void MosaicListView_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            SortListView((ListView)sender, e);
        }

        // Read the opj and get information related to tiles out of it.
        public void ReadOPJ()
        {
            StatusLabel.Text = "Loading...";
            Application.DoEvents();
            string myConnString = "Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" + MosaicOPJTB.Text;
            string mySelectQuery = "";
            mySelectQuery = "SELECT szProductName FROM Products";

            using (OleDbConnection myConnection = new OleDbConnection(myConnString))
            {

                OleDbCommand myCommand = new OleDbCommand(mySelectQuery, myConnection);
                myConnection.Open();
                using (OleDbDataReader myReader = myCommand.ExecuteReader())
                {
                    MosaicListView.Items.Clear();
                    while (myReader.Read())
                    {
                        MosaicListView.Items.Add(myReader[0].ToString());
                    }
                }
            }
            StatusLabel.Text = "Idle";
        }

        private void MosaicOutputBrowse_Click(object sender, EventArgs e)
        {
            // Displays an OpenFileDialog 
            FolderBrowserDialog openFolderDialog = new FolderBrowserDialog();
            openFolderDialog.Description = "Select an output folder";

            // Show the Dialog.
            if (openFolderDialog.ShowDialog() == DialogResult.OK)
            {
                MosaicOutputTB.Text = GetUniversalName(openFolderDialog.SelectedPath);
            }
        }

        private void MosaicOverwriteCB_CheckedChanged(object sender, EventArgs e)
        {
            if (MosaicOverwriteCB.Checked)
            {
                MosaicOutputTB.Enabled = false;
                MosaicOutputLabel.Enabled = false;
                MosaicOutputBrowse.Enabled = false;
            }
            else
            {
                MosaicOutputTB.Enabled = true;
                MosaicOutputLabel.Enabled = true;
                MosaicOutputBrowse.Enabled = true;
            }
        }

        #endregion

        #region Move/Copy Panel
        private void MCAddButton_Click(object sender, EventArgs e)
        {
            MCAddFolder(MCInputTB.Text);
        }

        private void MCRemoveButton_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem item in MCListView.SelectedItems)
            {
                MCListView.Items[MCListView.Items.IndexOf(item)].Remove();
            }
        }

        private void MCAddTXT_Click(object sender, EventArgs e)
        {
            // Displays an OpenFileDialog so the user can select a Cursor.
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Text File|*.txt";
            openFileDialog.Title = "Select a list of photo paths";

            // Show the Dialog.
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                MCAddFolder(openFileDialog.FileName);
            }
        }

        private void MCInputBrowse_Click(object sender, EventArgs e)
        {
            // Displays an OpenFolderDialog 
            FolderBrowserDialog openFolderDialog = new FolderBrowserDialog();
            openFolderDialog.Description = "Select an input folder";

            // Show the Dialog.
            if (openFolderDialog.ShowDialog() == DialogResult.OK)
            {
                MCInputTB.Text = GetUniversalName(openFolderDialog.SelectedPath);
            }
        }

        private void MCOutputBrowse_Click(object sender, EventArgs e)
        {
            // Displays an OpenFileDialog 
            FolderBrowserDialog openFolderDialog = new FolderBrowserDialog();
            openFolderDialog.Description = "Select an output folder";

            // Show the Dialog.
            if (openFolderDialog.ShowDialog() == DialogResult.OK)
            {
                MCOutputTB.Text = GetUniversalName(openFolderDialog.SelectedPath);
            }
        }

        private void MCInputTB_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Return)
            {
                MCAddFolder(MCInputTB.Text);
            }
        }

        private void MCOutputTB_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Return)
            {
                MCOutputTB.Text = GetUniversalName(MCOutputTB.Text);
            }
        }

        private void MCListView_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            SortListView((ListView)sender, e);
        }

        private void MCAddFolder(string folder)
        {
            string folderPath = GetUniversalName(folder);
            ListViewItem item = new ListViewItem();
            item.Text = folderPath;

            string[] files = Directory.GetFiles(folder, "*", SearchOption.AllDirectories);
            item.SubItems.Add(files.Length.ToString());
            MCListView.Items.Add(item);
            MCInputTB.Text = null;
        }

        #endregion

        #region Radiance Panel

        private void RadianceSharpenCheckbox_CheckedChanged(object sender, EventArgs e)
        {
            switch (RadianceSharpenCheckbox.Checked)
            {
                case true:
                    RadianceSharpenGB.Enabled = true;
                    break;
                case false:
                    RadianceSharpenGB.Enabled = false;
                    break;
            }
        }

        private void RadianceNumBandsTB_SelectedIndexChanged(object sender, EventArgs e)
        {
            switch (RadianceNumBandsTB.Text)
            {
                case "3":
                    RadianceANUD.Visible = false;
                    RadianceANUD.Enabled = false;
                    RadianceALumLabel.Visible = false;
                    RadianceALumLabel.Enabled = false;
                    break;
                case "4":
                    RadianceANUD.Visible = true;
                    RadianceANUD.Enabled = true;
                    RadianceALumLabel.Visible = true;
                    RadianceALumLabel.Enabled = true;
                    break;
            }
        }

        private void RadianceInputBrowse_Click(object sender, EventArgs e)
        {
            if (RPForm != null) RPForm.Close();
            // Displays an OpenFolderDialog 
            FolderBrowserDialog openFolderDialog = new FolderBrowserDialog();
            openFolderDialog.Description = "Select an input folder";

            // Show the Dialog.
            if (openFolderDialog.ShowDialog() == DialogResult.OK)
            {
                RadianceInputTB.Text = GetUniversalName(openFolderDialog.SelectedPath);
                RadianceOutputTB.Text = RadianceInputTB.Text + @"\radianced";
                RadianceShowPreview();
            }
        }

        private void RadianceShowPreview()
        {
            RPForm = new RadiancePreview(this);
            if (!RPForm.IsDisposed) RPForm.Show(this);
        }

        private void RadianceOutputBrowse_Click(object sender, EventArgs e)
        {
            // Displays an OpenFolderDialog 
            FolderBrowserDialog openFolderDialog = new FolderBrowserDialog();
            openFolderDialog.Description = "Select an output folder";

            // Show the Dialog.
            if (openFolderDialog.ShowDialog() == DialogResult.OK)
            {
                RadianceOutputTB.Text = GetUniversalName(openFolderDialog.SelectedPath);
            }
        }
        private void RadianceInputTB_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Return)
            {
                RadianceInputTB.Text = GetUniversalName(RadianceInputTB.Text);
                RadianceShowPreview();
            }
        }
        #endregion

        #region Rectify Panel
        private void RectifyISPMBrowse_Click(object sender, EventArgs e)
        {
            // Displays an OpenFileDialog so the user can select a Cursor.
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "ISPM Project Files|project";
            openFileDialog.Title = "Select a ISPM Project";

            // Show the Dialog.
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                RectifyISPMProjTB.Text = GetUniversalName(openFileDialog.FileName);
                currentATProject = new ATProject(RectifyISPMProjTB.Text.Substring(0, RectifyISPMProjTB.Text.Length - 8) + @"\photo");
                RectifyLoadProject(currentATProject);
            }
        }

        private void RectifyISPMProjTB_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Return)
            {
                if (!File.Exists(RectifyISPMProjTB.Text))
                {
                    MessageBox.Show("File Not Found");
                }
                else
                {
                    RectifyISPMProjTB.Text = GetUniversalName(RectifyISPMProjTB.Text);
                    currentATProject = new ATProject(RectifyISPMProjTB.Text.Substring(0, RectifyISPMProjTB.Text.Length - 8) + @"\photo");
                    RectifyLoadProject(currentATProject);
                }
            }
        }

        private void OutputFolderBrowse_Click(object sender, EventArgs e)
        {
            // Displays an OpenFileDialog 
            FolderBrowserDialog openFolderDialog = new FolderBrowserDialog();
            openFolderDialog.Description = "Select an output folder";

            // Show the Dialog.
            if (openFolderDialog.ShowDialog() == DialogResult.OK)
            {
                RectifyOutputTB.Text = GetUniversalName(openFolderDialog.SelectedPath);
            }
        }

        private void RectifyListView_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.A && e.Control)
            {
                foreach (ListViewItem item in RectifyListView.Items)
                {
                    item.Selected = true;
                }
            }

        }

        private void EditImagePaths_Click(object sender, EventArgs e)
        {
            EditImagePaths form = new EditImagePaths();
            foreach (ListViewItem item in RectifyListView.Items)
            {
                string itempath = Path.GetDirectoryName(item.SubItems[2].Text);
                if (!form.EditIPFromCB.Items.Contains(itempath)) form.EditIPFromCB.Items.Add(itempath);
            }

            while (form.ShowDialog(this) != DialogResult.Cancel)
            {
                foreach (ListViewItem item in RectifyListView.Items)
                {
                    item.SubItems[2].Text = Regex.Replace(item.SubItems[2].Text, Regex.Escape(form.EditIPFromCB.Text), form.EditIPToTB.Text, RegexOptions.IgnoreCase);
                }
                form.EditIPFromCB.Items.Clear();

                foreach (ListViewItem item in RectifyListView.Items)
                {
                    string itempath = Path.GetDirectoryName(item.SubItems[2].Text);
                    if (!form.EditIPFromCB.Items.Contains(itempath)) form.EditIPFromCB.Items.Add(itempath);
                }
                form.EditIPFromCB.Text = form.EditIPFromCB.Items[0].ToString();
            }
        }

        private void RectifyOutputTB_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Return)
            {
                RectifyOutputTB.Text = GetUniversalName(RectifyOutputTB.Text);
            }
        }

        private void RectifyAddButton_Click(object sender, EventArgs e)
        {

            // Displays an OpenFileDialog so the user can select a Cursor.
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Elevation Files|*.asc;*.dem";
            openFileDialog.Title = "Select elevation files";
            openFileDialog.Multiselect = true;
            // Show the Dialog.
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                foreach (String file in openFileDialog.FileNames)
                {
                    ListViewItem item = new ListViewItem();
                    item.Text = Path.GetFileName(file);
                    item.SubItems.Add(GetUniversalName(file));
                    RectifyElevationLV.Items.Add(item);
                }
            }
        }

        private void RectifyRemoveButton_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem item in RectifyElevationLV.SelectedItems)
            {
                RectifyElevationLV.Items[RectifyElevationLV.Items.IndexOf(item)].Remove();
            }
        }

        private void RectifyListView_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            SortListView((ListView)sender, e);
        }

        private void RectifyElevationLV_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            SortListView((ListView)sender, e);

        }

        private void RectifySelectByFile_Click(object sender, EventArgs e)
        {
            // Displays an OpenFileDialog so the user can select a Cursor.
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Text File|*.txt";
            openFileDialog.Title = "Select a file containing images to select";

            // Show the Dialog.
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                RectifyListView.SelectedItems.Clear();
                using (StreamReader sr = new StreamReader(openFileDialog.FileName))
                {
                    while (sr.Peek() >= 0)
                    {
                        string currentLine = sr.ReadLine();
                        foreach (ListViewItem item in RectifyListView.Items)
                        {
                            if (item.SubItems[1].Text.Equals(currentLine))
                            {
                                item.Selected = true;
                                continue;
                            }

                        }
                    }
                }
            }
        }

        private void RectifyDefProjBrowseTB_Click(object sender, EventArgs e)
        {
            // Displays an OpenFileDialog so the user can select a Cursor.
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Coordinate System File|*.csf";
            openFileDialog.Title = "Select a CSF";

            // Show the Dialog.
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                RectifyDefProjTB.Text = GetUniversalName(openFileDialog.FileName);
            }
        }

        private void RectifyDefProj_CheckedChanged(object sender, EventArgs e)
        {
            RectifyDefProjTB.Enabled = true;
            RectifyDefProjBrowse.Enabled = true;
            RectifyDefProjOutLabel.Enabled = true;
        }

        private void RectifyUseATProj_CheckedChanged(object sender, EventArgs e)
        {
            RectifyDefProjTB.Enabled = false;
            RectifyDefProjBrowse.Enabled = false;
            RectifyDefProjOutLabel.Enabled = false;
        }

        private void RectifyLoadProject(ATProject currentATProject)
        {
            RectifyListView.Items.Clear();
            foreach (Photo photo in currentATProject.atPhotos)
            {
                ListViewItem item = new ListViewItem();
                item.Text = photo.flight;
                item.SubItems.Add(photo.photoName);
                item.SubItems.Add(photo.filePath);
                RectifyListView.Items.Add(item);
            }
            ProjectTB.Text = new DirectoryInfo(currentATProject.atDirectory).Name;
        }

        private void RectifyCreateCSF_Click(object sender, EventArgs e)
        {
            Process p;
            string programFiles = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
            p = new Process();
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.FileName = programFiles + @"\Common Files\Intergraph\CoordSystems\Program\DefCSF.exe";
            p.Start();
        }

        private void RectifyElevationLV_DragEnter(object sender, DragEventArgs e)
        {
            e.Effect = DragDropEffects.All;
        }

        private void RectifyElevationLV_DragDrop(object sender, DragEventArgs e)
        {
            // Extract the data from the DataObject-Container into a string list
            string[] fileList = (string[])e.Data.GetData(DataFormats.FileDrop, false);

            foreach (String file in fileList)
            {
                ListViewItem item = new ListViewItem();
                item.Text = Path.GetFileName(file);
                item.SubItems.Add(GetUniversalName(file));
                RectifyElevationLV.Items.Add(item);
            }
        }

        #endregion

        #region Resize/Resample Panel

        private void RRInputBrowse_Click(object sender, EventArgs e)
        {
            // Displays an OpenFileDialog 
            FolderBrowserDialog openFolderDialog = new FolderBrowserDialog();
            openFolderDialog.Description = "Select an input folder";

            // Show the Dialog.
            if (openFolderDialog.ShowDialog() == DialogResult.OK)
            {
                RRInputTB.Text = GetUniversalName(openFolderDialog.SelectedPath);
                RROutputTB.Text = RRInputTB.Text + @"\resized";
            }
        }

        private void RRInputTB_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Return)
            {
                RRInputTB.Text = GetUniversalName(RRInputTB.Text);
                RROutputTB.Text = RRInputTB.Text + @"\resized";
            }
        }

        private void RROutputBrowse_Click(object sender, EventArgs e)
        {
            // Displays an OpenFileDialog 
            FolderBrowserDialog openFolderDialog = new FolderBrowserDialog();
            openFolderDialog.Description = "Select an output folder";

            // Show the Dialog.
            if (openFolderDialog.ShowDialog() == DialogResult.OK)
            {
                RROutputTB.Text = GetUniversalName(openFolderDialog.SelectedPath);
            }
        }

        private void RROutputTB_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Return)
            {
                RROutputTB.Text = GetUniversalName(RROutputTB.Text);
            }
        }

        #endregion

        #region SID Panel
        private void SIDAdd_Click(object sender, EventArgs e)
        {
            SIDAddFolder();
        }

        private void SIDRemove_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem item in SIDQueueLV.SelectedItems)
            {
                SIDQueueLV.Items[SIDQueueLV.Items.IndexOf(item)].Remove();
            }
        }

        private void SIDInputButton_Click(object sender, EventArgs e)
        {
            // Displays an OpenFileDialog 
            FolderBrowserDialog openFolderDialog = new FolderBrowserDialog();
            openFolderDialog.Description = "Select an input folder";

            // Show the Dialog.
            if (openFolderDialog.ShowDialog() == DialogResult.OK)
            {
                SIDInputTB.Text = GetUniversalName(openFolderDialog.SelectedPath);
                SIDOutputTB.Text = SIDInputTB.Text + @"\MrSIDs";
            }
        }

        private void SIDOutputButton_Click(object sender, EventArgs e)
        {
            // Displays an OpenFileDialog 
            FolderBrowserDialog openFolderDialog = new FolderBrowserDialog();
            openFolderDialog.Description = "Select an output folder";

            // Show the Dialog.
            if (openFolderDialog.ShowDialog() == DialogResult.OK)
            {
                SIDOutputTB.Text = GetUniversalName(openFolderDialog.SelectedPath);
            }
        }

        private void SIDInputTB_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Return)
            {
                SIDAddFolder();
            }
        }

        private void SIDOutputTB_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Return)
            {
                SIDOutputTB.Text = GetUniversalName(SIDOutputTB.Text);
            }
        }

        private void SIDQueueLV_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.A && e.Control)
            {
                foreach (ListViewItem item in SIDQueueLV.Items)
                {
                    item.Selected = true;
                }
            }
        }

        private void SIDAddFolder()
        {
            SIDInputTB.Text = GetUniversalName(SIDInputTB.Text);
            ListViewItem item = new ListViewItem();
            item.Text = SIDInputTB.Text;
            item.SubItems.Add(SIDCompressTB.Text);
            item.SubItems.Add(SIDIFormatCB.Text);
            item.SubItems.Add(SIDOFormatCB.Text);
            item.SubItems.Add(SIDTransNUD.Value.ToString());
            SIDQueueLV.Items.Add(item);
            SIDInputTB.Text = null;
        }

        private void SIDQueueLV_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            SortListView((ListView)sender, e);
        }
        #endregion

        #region Thumbs Panel

        private void ThumbsInputBrowse_Click(object sender, EventArgs e)
        {
            // Displays an OpenFolderDialog 
            FolderBrowserDialog openFolderDialog = new FolderBrowserDialog();
            openFolderDialog.Description = "Select an input folder";

            // Show the Dialog.
            if (openFolderDialog.ShowDialog() == DialogResult.OK)
            {
                ThumbsInputFolderTB.Text = GetUniversalName(openFolderDialog.SelectedPath);
                ThumbsOutputFolderTB.Text = ThumbsInputFolderTB.Text + @"\thumbs";
            }
        }

        private void ThumbsInputFolderTB_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Return)
            {
                ThumbsInputFolderTB.Text = GetUniversalName(ThumbsInputFolderTB.Text);
                ThumbsOutputFolderTB.Text = ThumbsInputFolderTB.Text + @"\thumbs";
            }
        }

        private void ThumbsOutputBrowse_Click(object sender, EventArgs e)
        {
            // Displays an OpenFileDialog 
            FolderBrowserDialog openFolderDialog = new FolderBrowserDialog();
            openFolderDialog.Description = "Select an output folder";

            // Show the Dialog.
            if (openFolderDialog.ShowDialog() == DialogResult.OK)
            {
                ThumbsOutputFolderTB.Text = GetUniversalName(openFolderDialog.SelectedPath);
            }
        }

        #endregion

        #region Image Convert Panel

        private void ImageConvertInputBrowse_Click(object sender, EventArgs e)
        {
            // Displays an OpenFolderDialog 
            FolderBrowserDialog openFolderDialog = new FolderBrowserDialog();
            openFolderDialog.Description = "Select an input folder";

            // Show the Dialog.
            if (openFolderDialog.ShowDialog() == DialogResult.OK)
            {
                ImageConvertInputTB.Text = GetUniversalName(openFolderDialog.SelectedPath);
                ImageConvertOutputTB.Text = ImageConvertInputTB.Text + @"\overviews";
            }
        }

        private void ImageConvertListButton_Click(object sender, EventArgs e)
        {
            // Displays an OpenFileDialog 
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Directory List|*.txt";
            openFileDialog.Title = "Select a text file containing directories.";

            // Show the Dialog.
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                string directoryText = "";
                using (StreamReader sr = new StreamReader(openFileDialog.FileName))
                {
                    directoryText = sr.ReadToEnd();
                }
                string[] directories = Regex.Split(directoryText, "\r\n");
                foreach (string directory in directories)
                {
                    if (!directory.Equals("")) ImageConvertAddFolder(directory);
                }
            }
        }

        private void ImageConvertOutputBrowse_Click(object sender, EventArgs e)
        {
            // Displays an OpenFileDialog 
            FolderBrowserDialog openFolderDialog = new FolderBrowserDialog();
            openFolderDialog.Description = "Select an output folder";

            // Show the Dialog.
            if (openFolderDialog.ShowDialog() == DialogResult.OK)
            {
                ImageConvertOutputTB.Text = GetUniversalName(openFolderDialog.SelectedPath);
            }
        }

        private void ImageConvertInputTB_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Return)
            {
                ImageConvertAddFolder(ImageConvertInputTB.Text);
            }
        }

        private void ImageConvertOutputTB_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Return)
            {
                ImageConvertOutputTB.Text = GetUniversalName(ImageConvertOutputTB.Text);
            }
        }

        private void ImageConvertAddButton_Click(object sender, EventArgs e)
        {
            ImageConvertAddFolder(ImageConvertInputTB.Text);
        }

        private void ImageConvertRemoveButton_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem item in ImageConvertInputLV.SelectedItems)
            {
                ImageConvertInputLV.Items[ImageConvertInputLV.Items.IndexOf(item)].Remove();
            }
        }

        private void ImageConvertAddTXTButton_Click(object sender, EventArgs e)
        {
            // Displays an OpenFileDialog so the user can select a Cursor.
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Text File|*.txt";
            openFileDialog.Title = "Select a list of photo paths";

            // Show the Dialog.
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                ImageConvertAddFolder(openFileDialog.FileName);
            }
        }

        private void ImageConvertAddFolder(string folder)
        {
            string folderPath = GetUniversalName(folder);
            ListViewItem item = new ListViewItem();
            item.Text = folderPath;
            item.SubItems.Add(ImageConvertCompress.Checked.ToString());
            item.SubItems.Add(ImageConvertOverview.Checked.ToString());
            item.SubItems.Add(ImageConvertTile.Checked.ToString());

            string bandLetters = "";
            if (ImageConvertRedCB.Checked)
            {
                bandLetters += 'r';
            }
            if (ImageConvertGreenCB.Checked)
            {
                bandLetters += 'g';
            }
            if (ImageConvertBlueCB.Checked)
            {
                bandLetters += 'b';
            }
            if (ImageConvertAlphaCB.Checked)
            {
                bandLetters += 'a';
            }

            item.SubItems.Add(bandLetters);
            item.SubItems.Add(ImageConvert16BitCB.Checked.ToString());
            ImageConvertInputLV.Items.Add(item);
            ImageConvertInputTB.Text = null;
        }

        private void ImageConvertInputLV_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            SortListView((ListView)sender, e);
        }

        #endregion

        #endregion

        #region Helper Functions

        private void ProcessCombo_SelectedIndexChanged(object sender, EventArgs e)
        {
            switch (ProcessCombo.Text)
            {
                case "AT":
                    CurrentPanel.Visible = false;
                    CurrentPanel.Enabled = false;
                    CurrentPanel = ATPanel;
                    CurrentPanel.Visible = true;
                    CurrentPanel.Enabled = true;
                    break;
                case "Grid Points":
                    CurrentPanel.Visible = false;
                    CurrentPanel.Enabled = false;
                    CurrentPanel = GridPointsPanel;
                    CurrentPanel.Visible = true;
                    CurrentPanel.Enabled = true;
                    break;
                case "Image Conversion":
                    CurrentPanel.Visible = false;
                    CurrentPanel.Enabled = false;
                    CurrentPanel = ImageConvertPanel;
                    CurrentPanel.Visible = true;
                    CurrentPanel.Enabled = true;
                    break;
                case "Mosaic":
                    CurrentPanel.Visible = false;
                    CurrentPanel.Enabled = false;
                    CurrentPanel = MosaicPanel;
                    CurrentPanel.Visible = true;
                    CurrentPanel.Enabled = true;
                    break;
                case "Move/Copy":
                    CurrentPanel.Visible = false;
                    CurrentPanel.Enabled = false;
                    CurrentPanel = MoveCopyPanel;
                    CurrentPanel.Visible = true;
                    CurrentPanel.Enabled = true;
                    break;
                case "MrSID Encoding":
                    CurrentPanel.Visible = false;
                    CurrentPanel.Enabled = false;
                    CurrentPanel = SIDPanel;
                    CurrentPanel.Visible = true;
                    CurrentPanel.Enabled = true;
                    break;
                case "Radiance":
                    CurrentPanel.Visible = false;
                    CurrentPanel.Enabled = false;
                    CurrentPanel = RadiancePanel;
                    CurrentPanel.Visible = true;
                    CurrentPanel.Enabled = true;
                    break;
                case "Rectify":
                    CurrentPanel.Visible = false;
                    CurrentPanel.Enabled = false;
                    CurrentPanel = RectifyPanel;
                    CurrentPanel.Visible = true;
                    CurrentPanel.Enabled = true;
                    break;
                case "Resize/Resample":
                    CurrentPanel.Visible = false;
                    CurrentPanel.Enabled = false;
                    CurrentPanel = ResizeResamplePanel;
                    CurrentPanel.Visible = true;
                    CurrentPanel.Enabled = true;
                    break;
                case "Thumbs":
                    CurrentPanel.Visible = false;
                    CurrentPanel.Enabled = false;
                    CurrentPanel = ThumbsPanel;
                    CurrentPanel.Visible = true;
                    CurrentPanel.Enabled = true;
                    break;
            }
            if ((CheckForm(RPForm)) && ProcessCombo.Text != "Radiance")
            {
                RPForm.Close();
            }
            StatusLabel.Text = "Idle";
            SubmitProgressBar.Value = 0;
        }

        private void SubmitButton_Click(object sender, EventArgs e)
        {
            //Check to make sure all forms are filled and valid
            bool readyToSubmit = true;
            string reasonForError = "";
            switch (ProcessCombo.Text)
            {
                case "Rectify":
                    if (RectifyOutputTB.Text.Length == 0)
                    {
                        readyToSubmit = false;
                        reasonForError = "Missing output folder!";
                    }
                    if (RectifyListView.SelectedItems.Count == 0)
                    {
                        readyToSubmit = false;
                        reasonForError = "No images selected!";
                    }
                    if (RectifyPixelUnits.Text == "")
                    {
                        readyToSubmit = false;
                        reasonForError = "No pixel units selected!";
                    }
                    break;

                case "Grid Points":
                    if (GridPointsOutputTB.Text.Length == 0)
                    {
                        readyToSubmit = false;
                        reasonForError = "Missing output folder!";
                    }
                    break;
                case "Mosaic":
                    if ((!MosaicOverwriteCB.Checked) && (MosaicOutputTB.Text.Length < 1))
                    {
                        readyToSubmit = false;
                        reasonForError = "Missing output folder!";
                    }
                    break;
            }

            //If all good, go; otherwise provide error message
            if (readyToSubmit)
            {
                CheckPriority();
                PrepSubmit();
            }
            else MessageBox.Show("Cannot submit! " + reasonForError);
        }

        private void QueueButton_Click(object sender, EventArgs e)
        {
            CondorQueue form = new CondorQueue();
            form.ShowDialog();
        }

        private void PrepSubmit()
        {

            #region PreSubmit
            if (!SubmitBW.IsBusy)
            {
                //variable used if additional prepping is needed
                bool preppingSubmit = false;
                RectifyJob rectifyJob = null;
                List<string> ATBlocks = null;

                // disable the gui while running
                CurrentPanel.Enabled = false;
                ProcessCombo.Enabled = false;

                //create the submit file
                if (File.Exists(@"C:\condorsubmitgui.sub")) File.Delete(@"C:\condorsubmitgui.sub");
                using (StreamWriter sw = new StreamWriter(@"C:\condorsubmitgui.sub"))
                {
                    sw.WriteLine(@"universe=vanilla");
                    sw.Write("requirements= ");
                    if (ProcessCombo.Text == "Mosaic") sw.Write("CanRunMosaic == TRUE &&");
                    sw.WriteLine("OpSys != \"Dummy\" && Arch != \"Dummy\"");

                    // set priority
                    int priorityLevel = 0;
                    switch (PriorityCombo.Text)
                    {
                        case "Highest":
                            priorityLevel = 20;
                            break;
                        case "High":
                            priorityLevel = 10;
                            break;
                        case "Medium":
                            priorityLevel = 0;
                            break;
                        case "Low":
                            priorityLevel = -10;
                            break;
                        case "Lowest":
                            priorityLevel = -20;
                            break;
                    }
                    sw.WriteLine(@"priority=" + priorityLevel);

                    //transfer orthopro files for mosaic process to condor nods
                    if (ProcessCombo.Text == "Mosaic")
                    {
                        string OPJDir = Path.GetDirectoryName(MosaicOPJTB.Text);
                        string OPJName = Path.GetFileNameWithoutExtension(MosaicOPJTB.Text);
                        sw.WriteLine("transfer_input_files=" + OPJDir + @"\" + OPJName + ".opj," + OPJDir + @"\OP" + OPJName + ".mdb");
                    }

                    //define where to put logs
                    sw.WriteLine(@"transfer_executable=true");
                    sw.WriteLine(@"log=X:\3.CONDOR\CondorLogs\CONDOR.log");
                    sw.WriteLine(@"log_xml=true");
                    sw.WriteLine();
                    sw.WriteLine(@"run_as_owner=true");

                    //used for multiple input folders and handling their output location
                    string outputDirectory = null;

            #endregion

                    //conditional based job builder
                    switch (ProcessCombo.Text)
                    {

                        #region Submit Rectify
                        case "Rectify":
                            //calculate the pixel size for input to CSRectPhoto.exe
                            double pixelSize = System.Convert.ToDouble(RectifyPixelSizeTB.Text);
                            switch (RectifyPixelUnits.Text)
                            {
                                case "sf":
                                    //for survey feet
                                    pixelSize = ((pixelSize * 1200) / 3937);
                                    break;
                                case "ft":
                                    //for international feet
                                    pixelSize = pixelSize * 0.3048;
                                    break;
                                case "m":
                                    //for meters (nothing needs to be done)
                                    break;
                            }
                            Math.Round(pixelSize, 14);

                            //Create output directory
                            Directory.CreateDirectory(RectifyOutputTB.Text);

                            //create list of photos to rectify
                            List<Photo> rectifyPhotos = new List<Photo>();
                            foreach (ListViewItem item in RectifyListView.SelectedItems)
                            {
                                Photo currentPhoto = currentATProject.FindPhoto(item.SubItems[1].Text);
                                currentPhoto.filePath = item.SubItems[2].Text;
                                rectifyPhotos.Add(currentPhoto);
                            }

                            //build list of elevation tiles to use
                            List<ElevationTile> elevationTiles = new List<ElevationTile>();

                            foreach (ListViewItem item in RectifyElevationLV.Items)
                            {
                                elevationTiles.Add(new ElevationTile(item.SubItems[1].Text));
                            }


                            //assign output csf
                            string outputCSF = "";
                            if (RectifyUseATProj.Checked) outputCSF = currentATProject.atDirectory + @"\csf\project.csf";
                            else outputCSF = RectifyDefProjTB.Text;

                            //create rectify job object
                            rectifyJob = new RectifyJob(rectifyPhotos, elevationTiles, RectifyCompressCheckBox.Checked, RectifySpacingComboBox.Text, pixelSize, RectifyVoidColorNUD.Value, RectifyOutputTB.Text, currentATProject.atDirectory, outputCSF);

                            preppingSubmit = true;
                            StatusLabel.Text = "Preparing";
                            break;
                        #endregion

                        #region Submit Mosaic
                        case "Mosaic":
                            string myConnString = "Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" + MosaicOPJTB.Text;
                            using (OleDbConnection myConnection = new OleDbConnection(myConnString))
                            {
                                myConnection.Open();
                                foreach (ListViewItem tile in MosaicListView.SelectedItems)
                                {
                                    string mySelectQuery = "SELECT nProductID FROM Products WHERE szProductName = \"" + tile.Text + "\"";
                                    string tileNumber = "";

                                    OleDbCommand myCommand = new OleDbCommand(mySelectQuery, myConnection);
                                    using (OleDbDataReader myReader = myCommand.ExecuteReader())
                                    {
                                        myReader.Read();
                                        tileNumber = myReader[0].ToString();

                                    }

                                    if ((MosaicOverwriteCB.Checked) || ((!MosaicOverwriteCB.Checked) && (!File.Exists(MosaicOutputTB.Text + @"\" + tile.Text + ".tif"))))
                                    {
                                        string jobName = "Mosaic-" + ProjectTB.Text + "-" + tile.Text;
                                        using (StreamWriter swbat = new StreamWriter(@"\\pinmapnas01\projects\3.CONDOR\CondorBat\" + jobName + ".bat"))
                                        {
                                            swbat.WriteLine(@"set path=%PATH%;C:\Program Files\Imagestation Orthopro;C:\Program Files (x86)\Imagestation Orthopro");
                                            swbat.WriteLine("OrthoArea.exe -i " + Path.GetFileName(MosaicOPJTB.Text) + " -p " + tileNumber);
                                            swbat.WriteLine(@"del \\pinmapnas01\projects\3.CONDOR\CondorBat\" + jobName + ".bat");
                                        }
                                        sw.WriteLine(@"executable=\\pinmapnas01\projects\3.CONDOR\CondorBat\" + jobName + ".bat");
                                        sw.WriteLine("+JobName=\"" + jobName + "\"");
                                        sw.WriteLine(@"output=X:\3.CONDOR\CondorLogs\" + jobName + ".out");
                                        sw.WriteLine(@"error=X:\3.CONDOR\CondorLogs\" + jobName + ".err");
                                        sw.WriteLine(@"log=X:\3.CONDOR\CondorLogs\" + jobName + ".log");
                                        sw.WriteLine("queue");
                                    }
                                }
                            }
                            break;
                        #endregion

                        #region Submit AT
                        case "AT":
                            sw.WriteLine(@"notify_user=3175095285@vtext.com");
                            sw.WriteLine(@"+EmailAttributes=Args");
                            submitAT = true;
                            ATBlocks = new List<string>();
                            foreach (ListViewItem item in ATBlockListView.SelectedItems)
                            {
                                ATBlocks.Add(item.Text);
                                string jobName = "AT-" + ProjectTB.Text + "-" + item.Text;
                                using (StreamWriter swbat = new StreamWriter(@"\\pinmapnas01\projects\3.CONDOR\CondorBat\" + jobName + ".bat"))
                                {
                                    swbat.WriteLine(@"set path=%PATH%;C:\Program Files\Common Files\ZI Imaging;C:\Program Files (x86)\Common Files\ZI Imaging");
                                    swbat.WriteLine("aatnt.exe " + currentATProject.atDirectory + " " + item.Text + " " + currentATProject.atDirectory + @"\" + item.Text + @"\" + item.Text + @".ste");
                                    swbat.WriteLine(@"del \\pinmapnas01\projects\3.CONDOR\CondorBat\" + jobName + ".bat");
                                }
                                sw.WriteLine(@"executable=\\pinmapnas01\projects\3.CONDOR\CondorBat\" + jobName + ".bat");
                                sw.WriteLine("+JobName=\"" + jobName + "\"");
                                sw.WriteLine(@"output=X:\3.CONDOR\CondorLogs\" + jobName + ".out");
                                sw.WriteLine(@"error=X:\3.CONDOR\CondorLogs\" + jobName + ".err");
                                sw.WriteLine(@"log=X:\3.CONDOR\CondorLogs\" + jobName + ".log");
                                sw.WriteLine("queue");
                            }
                            preppingSubmit = true;
                            break;
                        #endregion

                        #region Submit Thumbs
                        case "Thumbs":
                            string[] images = Directory.GetFiles(ThumbsInputFolderTB.Text, @"*.tif");
                            foreach (string image in images)
                            {
                                string jobName = "Thumbnails-" + ProjectTB.Text + "-" + Path.GetFileNameWithoutExtension(image);
                                using (StreamWriter swbat = new StreamWriter(@"\\pinmapnas01\projects\3.CONDOR\CondorBat\" + jobName + ".bat"))
                                {
                                    swbat.WriteLine(@"set path=%PATH%;C:\Program Files\ISRU\bin;C:\Program Files (x86)\ISRU\bin");
                                    swbat.WriteLine("makethumbs.exe /d /m 1500 /i " + image + @" /f " + ThumbsOutputFolderTB.Text);
                                    swbat.WriteLine(@"del \\pinmapnas01\projects\3.CONDOR\CondorBat\" + jobName + ".bat");
                                }
                                sw.WriteLine(@"executable=\\pinmapnas01\projects\3.CONDOR\CondorBat\" + jobName + ".bat");
                                sw.WriteLine("+JobName=\"" + jobName + "\"");
                                sw.WriteLine(@"output=X:\3.CONDOR\CondorLogs\" + jobName + ".out");
                                sw.WriteLine(@"error=X:\3.CONDOR\CondorLogs\" + jobName + ".err");
                                sw.WriteLine(@"log=X:\3.CONDOR\CondorLogs\" + jobName + ".log");
                                sw.WriteLine("queue");
                            }
                            break;
                        #endregion

                        #region Submit Move/Copy
                        case "Move/Copy":
                            foreach (ListViewItem item in MCListView.Items)
                            {
                                //get a list of all the files to move
                                string[] files;
                                if (item.Text.Contains(".txt"))
                                {
                                    using (StreamReader sr = new StreamReader(item.Text))
                                    {
                                        files = Regex.Split(sr.ReadToEnd(), "\r\n");
                                    }
                                }
                                else
                                {
                                    files = Directory.GetFiles(item.Text, "*", SearchOption.AllDirectories);
                                }
                                //get a list of directories and prepare the destination
                                string[] directories = Directory.GetDirectories(item.Text, "*", SearchOption.AllDirectories);
                                foreach (string directory in directories) Directory.CreateDirectory(MCOutputTB.Text + directory);

                                //make move command for each file
                                foreach (string file in files)
                                {
                                    string jobName = "MoveCopy-" + ProjectTB.Text + "-" + Path.GetFileName(file);
                                    string outputFolder = MCOutputTB.Text + "\\" + Path.GetDirectoryName(file);
                                    using (StreamWriter swbat = new StreamWriter(@"\\pinmapnas01\projects\3.CONDOR\CondorBat\" + jobName + ".bat"))
                                    {
                                        swbat.WriteLine("move " + "\"" + file + "\" " + outputFolder);

                                    }
                                    sw.WriteLine(@"executable=\\pinmapnas01\projects\3.CONDOR\CondorBat\" + jobName + ".bat");
                                    sw.WriteLine("+JobName=\"" + jobName + "\"");
                                    sw.WriteLine(@"output=X:\3.CONDOR\CondorLogs\" + jobName + ".out");
                                    sw.WriteLine(@"error=X:\3.CONDOR\CondorLogs\" + jobName + ".err");
                                    sw.WriteLine(@"log=X:\3.CONDOR\CondorLogs\" + jobName + ".log");
                                    sw.WriteLine("queue");
                                }
                            }
                            break;
                        #endregion
                        #region Submit Image Convert
                        case "Image Conversion":
                            foreach (ListViewItem item in ImageConvertInputLV.Items)
                            {
                                bool compress = Convert.ToBoolean(item.SubItems[1].Text);
                                bool tiling = Convert.ToBoolean(item.SubItems[2].Text);
                                bool overview = Convert.ToBoolean(item.SubItems[3].Text);
                                bool sixteenbit = Convert.ToBoolean(item.SubItems[5].Text);
                                string bandsToUse = item.SubItems[4].Text;

                                if (ImageConvertOutputTB.Text.Length < 1)
                                {
                                    outputDirectory = item.Text + @"\processed";
                                }
                                else
                                {
                                    outputDirectory = ImageConvertOutputTB.Text;
                                }
                                Directory.CreateDirectory(outputDirectory);

                                if (item.Text.Contains(".txt"))
                                {
                                    using (StreamReader sr = new StreamReader(item.Text))
                                    {
                                        images = Regex.Split(sr.ReadToEnd(), "\r\n");
                                    }
                                }
                                else
                                {
                                    images = Directory.GetFiles(item.Text, @"*.tif");
                                }
                                foreach (string image in images)
                                {
                                    if ((ImageConvertOverwriteCB.Checked) || ((!ImageConvertOverwriteCB.Checked) && (!File.Exists(outputDirectory + @"\" + Path.GetFileName(image)))))
                                    {
                                        string jobName = "ImageConvert-" + ProjectTB.Text + "-" + Path.GetFileNameWithoutExtension(image);

                                        using (StreamWriter swbat = new StreamWriter(@"\\pinmapnas01\projects\3.CONDOR\CondorBat\" + jobName + ".bat"))
                                        {
                                            swbat.WriteLine(@"set path=%PATH%;C:\Program Files\ISRU\bin;C:\Program Files (x86)\ISRU\bin");
                                            swbat.WriteLine(@"mkdir C:\temp\processed");
                                            swbat.WriteLine(@"xcopy /y /d " + "\"" + image + "\" C:\\temp");
                                            swbat.Write("mr_file.exe -T ");
                                            if (sixteenbit) swbat.Write("-m 0 -M 65535 ");
                                            if (compress) swbat.Write("-C j -Q 10 ");
                                            if (overview) swbat.Write("-K g ");
                                            if (tiling) swbat.Write("-t 256 ");
                                            switch (bandsToUse)
                                            {
                                                case "rgb":
                                                    swbat.Write("-x 123 ");
                                                    break;
                                                case "rga":
                                                    swbat.Write("-x 412 ");
                                                    break;
                                                case "rgba":
                                                    swbat.Write("-E ");
                                                    break;
                                                default:
                                                    swbat.Write("-e" + bandsToUse + " ");
                                                    break;
                                            }

                                            swbat.WriteLine(@"C:\temp\" + Path.GetFileName(image) + @" C:\temp\processed\" + Path.GetFileName(image));
                                            swbat.WriteLine(@"xcopy /y /d C:\temp\processed\" + Path.GetFileName(image) + " \"" + outputDirectory + "\"");
                                            swbat.WriteLine(@"del /s C:\temp\" + Path.GetFileName(image) + ".*");
                                            swbat.WriteLine(@"del \\pinmapnas01\projects\3.CONDOR\CondorBat\" + jobName + ".bat");
                                        }
                                        sw.WriteLine(@"executable=\\pinmapnas01\projects\3.CONDOR\CondorBat\" + jobName + ".bat");
                                        sw.WriteLine("+JobName=\"" + jobName + "\"");
                                        sw.WriteLine(@"output=X:\3.CONDOR\CondorLogs\" + jobName + ".out");
                                        sw.WriteLine(@"error=X:\3.CONDOR\CondorLogs\" + jobName + ".err");
                                        sw.WriteLine(@"log=X:\3.CONDOR\CondorLogs\" + jobName + ".log");
                                        sw.WriteLine("queue");
                                    }
                                }
                            }
                            break;
                        #endregion

                        #region Submit Resize
                        case "Resize/Resample":
                            images = Directory.GetFiles(RRInputTB.Text, @"*.tfw");
                            Directory.CreateDirectory(RROutputTB.Text);
                            foreach (string image in images)
                            {
                                if ((RROverwriteCB.Checked) || ((!RROverwriteCB.Checked) && (!File.Exists(outputDirectory + @"\" + Path.GetFileName(image)))))
                                {
                                    string jobName = "ResizeResample-" + Path.GetFileNameWithoutExtension(image);
                                    using (StreamWriter swbat = new StreamWriter(@"\\pinmapnas01\projects\3.CONDOR\CondorBat\" + jobName + ".bat"))
                                    {
                                        swbat.WriteLine(@"set path=%PATH%;C:\Program Files\FWTools2.4.7\bin;C:\Program Files (x86)\FWTools2.4.7\bin");
                                        swbat.WriteLine("gdalwarp.exe -tr " + RRXResTB.Text + " " + RRYResTB.Text + " -co \\\"TFW=YES\\\" -co \\\"PHOTOMETRIC=RGB\\\" -co \\\"ALPHA=NO\\\" " + Path.GetFullPath(image).Substring(0, Path.GetFullPath(image).Length - 4) + ".tif " + RROutputTB.Text + @"\" + Path.GetFileNameWithoutExtension(image) + ".tif");
                                        swbat.WriteLine(@"del \\pinmapnas01\projects\3.CONDOR\CondorBat\" + jobName + ".bat");
                                    }
                                    sw.WriteLine(@"executable=\\pinmapnas01\projects\3.CONDOR\CondorBat\" + jobName + ".bat");
                                    sw.WriteLine("+JobName=\"" + jobName + "\"");
                                    sw.WriteLine(@"output=X:\3.CONDOR\CondorLogs\" + jobName + ".out");
                                    sw.WriteLine(@"error=X:\3.CONDOR\CondorLogs\" + jobName + ".err");
                                    sw.WriteLine(@"log=X:\3.CONDOR\CondorLogs\" + jobName + ".log");
                                    sw.WriteLine("queue");
                                }
                            }
                            break;
                        #endregion

                        #region Submit Grid Points
                        case "Grid Points":
                            string[] dgns = Directory.GetFiles(GridPointsInputTB.Text, @"*.dgn");
                            foreach (string dgn in dgns)
                            {
                                string jobName = "GridPoints-" + ProjectTB.Text + "-" + Path.GetFileNameWithoutExtension(dgn);
                                using (StreamWriter swbat = new StreamWriter(@"\\pinmapnas01\projects\3.CONDOR\CondorBat\" + jobName + ".bat"))
                                {
                                    swbat.WriteLine(@"set path=%PATH%;C:\Program Files\FME;C:\Program Files (x86)\FME");
                                    swbat.WriteLine(@"fme.exe \\pinmapnas01\projects\2.PSEXEC\FME_CLIP\dgnto10kgrid.fmw --SourceDataset_DGNV8 " + dgn + " --DestDataset_TEXTLINE " + GridPointsOutputTB.Text + @"\%1 --SourceDataset_SHAPE " + GridPointsBoundaryTB.Text);
                                    swbat.WriteLine(@"del \\pinmapnas01\projects\3.CONDOR\CondorBat\" + jobName + ".bat");
                                }
                                sw.WriteLine(@"executable=\\pinmapnas01\projects\3.CONDOR\CondorBat\" + jobName + ".bat");
                                sw.WriteLine(@"arguments=$$(Name)");
                                sw.WriteLine("+JobName=\"" + jobName + "\"");
                                sw.WriteLine(@"output=X:\3.CONDOR\CondorLogs\" + jobName + ".out");
                                sw.WriteLine(@"error=X:\3.CONDOR\CondorLogs\" + jobName + ".err");
                                sw.WriteLine(@"log=X:\3.CONDOR\CondorLogs\" + jobName + ".log");
                                sw.WriteLine("queue");
                            }
                            break;
                        #endregion

                        #region Submit SID Encoding
                        case "MrSID Encoding":
                            foreach (ListViewItem item in SIDQueueLV.Items)
                            {
                                int compressionRatio = Convert.ToInt32(item.SubItems[1].Text);
                                string inputFormat = item.SubItems[2].Text;
                                string outputFormat = item.SubItems[3].Text;
                                string transparencyColor = item.SubItems[4].Text;
                                string outputExtension = item.SubItems[3].Text;
                                string inputExtension = item.SubItems[2].Text;
                                switch (inputExtension)
                                {
                                    case "mg2":
                                    case "mg3":
                                    case "mg4":
                                        inputExtension = "sid";
                                        break;
                                    case "tifw":
                                    case "tifg":
                                        inputExtension = ".tif";
                                        break;
                                }

                                switch (outputExtension)
                                {
                                    case "mg2":
                                    case "mg3":
                                    case "mg4":
                                        outputExtension = "sid";
                                        break;
                                    case "tifw":
                                    case "tifg":
                                        outputExtension = ".tif";
                                        break;
                                }

                                if (SIDOutputTB.Text.Length < 1)
                                {
                                    outputDirectory = item.Text + @"\processed";
                                }
                                else
                                {
                                    outputDirectory = SIDOutputTB.Text;
                                }
                                Directory.CreateDirectory(outputDirectory);
                                images = Directory.GetFiles(item.Text, @"*.tif");
                                foreach (string image in images)
                                {
                                    string jobName = "MrSID-" + ProjectTB.Text + "-" + Path.GetFileNameWithoutExtension(image);
                                    jobName = jobName.Replace(' ', '_');

                                    if ((SIDOverwriteCB.Checked) || ((!SIDOverwriteCB.Checked) && (!File.Exists(outputDirectory + @"\" + Path.GetFileNameWithoutExtension(image) + ".sid"))))
                                    {
                                        using (StreamWriter swbat = new StreamWriter(@"\\pinmapnas01\projects\3.CONDOR\CondorBat\" + jobName + ".bat"))
                                        {
                                            swbat.WriteLine(@"set path=%PATH%;C:\Program Files\LizardTech\GeoExpress 8\bin;C:\Program Files (x86)\LizardTech\GeoExpress 8\bin");
                                            swbat.WriteLine("xcopy /y /d \"" + Path.GetDirectoryName(image) + @"\" + Path.GetFileNameWithoutExtension(image) + ".t*\" C:\\temp");
                                            swbat.WriteLine("mrsidgeoencoderu -wf -if " + inputFormat + @" -tempdir C:\temp -compressionratio " + compressionRatio + " -tpd " + transparencyColor + " -input \"C:\\temp\\" + Path.GetFileNameWithoutExtension(image) + inputExtension + "\" -output \"C:\\temp\\processed\\" + Path.GetFileNameWithoutExtension(image) + "." + outputExtension + "\" -outputformat " + outputFormat);
                                            swbat.WriteLine(@"xcopy /y /d C:\temp\processed\" + Path.GetFileNameWithoutExtension(image) + ".* \"" + outputDirectory + "\"");
                                            swbat.WriteLine("del /s /q \"C:\\temp\\" + Path.GetFileNameWithoutExtension(image) + ".*\"");
                                            swbat.WriteLine("del \\\\pinmapnas01\\projects\\3.CONDOR\\CondorBat\\\"" + jobName + ".bat\"");
                                        }

                                        sw.WriteLine(@"executable=\\pinmapnas01\projects\3.CONDOR\CondorBat\" + jobName + ".bat");
                                        sw.WriteLine("+JobName=\"" + jobName + "\"");
                                        sw.WriteLine(@"output=X:\3.CONDOR\CondorLogs\" + jobName + ".out");
                                        sw.WriteLine(@"error=X:\3.CONDOR\CondorLogs\" + jobName + ".err");
                                        sw.WriteLine(@"log=X:\3.CONDOR\CondorLogs\" + jobName + ".log");
                                        sw.WriteLine("queue");
                                    }
                                }
                            }
                            break;
                        #endregion

                        #region Submit Radiance
                        case "Radiance":
                            images = Directory.GetFiles(RadianceInputTB.Text, @"*.tif");
                            foreach (string image in images)
                            {
                                sw.WriteLine(@"executable=\\pinmapnas01\projects\3.CONDOR\CondorBat\" + Path.GetFileNameWithoutExtension(image) + ".bat");
                                sw.WriteLine(@"output=X:\3.CONDOR\CondorLogs\" + Path.GetFileNameWithoutExtension(image) + ".out");
                                sw.WriteLine(@"error=X:\3.CONDOR\CondorLogs\" + Path.GetFileNameWithoutExtension(image) + ".err");
                                sw.WriteLine(@"log=X:\3.CONDOR\CondorLogs\" + Path.GetFileNameWithoutExtension(image) + ".log");
                                sw.WriteLine("queue");

                                using (StreamWriter swbat = new StreamWriter(@"\\pinmapnas01\projects\3.CONDOR\CondorBat\Radiance-" + Path.GetFileNameWithoutExtension(image) + ".bat"))
                                {
                                    swbat.WriteLine("mkdir " + RadianceInputTB.Text + @"\done");
                                    swbat.WriteLine("mkdir " + RadianceOutputTB.Text + @"\channels\red");
                                    swbat.WriteLine("mkdir " + RadianceOutputTB.Text + @"\channels\green");
                                    swbat.WriteLine("mkdir " + RadianceOutputTB.Text + @"\channels\blue");
                                    swbat.WriteLine("mkdir " + RadianceOutputTB.Text + @"\channels\dodged\red");
                                    swbat.WriteLine("mkdir " + RadianceOutputTB.Text + @"\channels\dodged\green");
                                    swbat.WriteLine("mkdir " + RadianceOutputTB.Text + @"\channels\dodged\blue");
                                    swbat.WriteLine("mkdir " + RadianceOutputTB.Text + @"\channels\dodged\merged");
                                    swbat.WriteLine("mkdir " + RadianceOutputTB.Text + @"\sharpened");
                                    swbat.WriteLine("mkdir " + RadianceOutputTB.Text + @"\completed");

                                    swbat.WriteLine("\"c:\\program files\\ImageMagick-6.7.0-Q16\\convert.exe\" -sharpen " + RadianceRadiusTB.Text + "X" + RadianceSigmaTB.Text + " -brightness-contrast " + RadianceBrightTB.Text + "X" + RadianceContrastTB.Text + " " + image + " " + RadianceOutputTB.Text + @"\sharpened\" + Path.GetFileName(image));

                                    if (RadianceNumBandsTB.Text == "4")
                                    {
                                        swbat.WriteLine("mkdir " + RadianceOutputTB.Text + @"\channels\dodged\alpha");
                                        swbat.WriteLine("mkdir " + RadianceOutputTB.Text + @"\channels\alpha");
                                        swbat.WriteLine("mr_file.exe -o " + RadianceNumBandsTB.Text + " -T " + RadianceOutputTB.Text + @"\sharpened\" + Path.GetFileName(image) + " " + RadianceOutputTB.Text + @"\channels\red\" + Path.GetFileName(image) + " " + RadianceOutputTB.Text + @"\channels\green\" + Path.GetFileName(image) + " " + RadianceOutputTB.Text + @"\channels\blue\" + Path.GetFileName(image) + " " + RadianceOutputTB.Text + @"\channels\alpha\" + Path.GetFileName(image));
                                    }
                                    else
                                    {
                                        swbat.WriteLine("mr_file.exe -o " + RadianceNumBandsTB.Text + " -T " + RadianceOutputTB.Text + @"\sharpened\" + Path.GetFileName(image) + " " + RadianceOutputTB.Text + @"\channels\red\" + Path.GetFileName(image) + " " + RadianceOutputTB.Text + @"\channels\green\" + Path.GetFileName(image) + " " + RadianceOutputTB.Text + @"\channels\blue\" + Path.GetFileName(image));
                                    }

                                    swbat.WriteLine("DodgeCmd.exe -i " + RadianceOutputTB.Text + @"\channels\blue\" + Path.GetFileName(image) + @" -o " + RadianceOutputTB.Text + @"\channels\dodged\blue\" + Path.GetFileName(image) + @" -T -u -c " + RadianceBNUD.Value + " -n -t 128 -g -0 +g 0 -k 15 -f " + RadianceFillNUD.Value);
                                    swbat.WriteLine("DodgeCmd.exe -i " + RadianceOutputTB.Text + @"\channels\green\" + Path.GetFileName(image) + @" -o " + RadianceOutputTB.Text + @"\channels\dodged\green\" + Path.GetFileName(image) + @" -T -u -c " + RadianceGNUD.Value + " -n -t 128 -g -0 +g 0 -k 15 -f " + RadianceFillNUD.Value);
                                    swbat.WriteLine("DodgeCmd.exe -i " + RadianceOutputTB.Text + @"\channels\red\" + Path.GetFileName(image) + @" -o " + RadianceOutputTB.Text + @"\channels\dodged\red\" + Path.GetFileName(image) + @" -T -u -c " + RadianceRNUD.Value + " -n -t 128 -g -0 +g 0 -k 15 -f " + RadianceFillNUD.Value);

                                    // 3/4 band specific parameters
                                    if (RadianceNumBandsTB.Text == "4")
                                    {
                                        swbat.WriteLine("DodgeCmd.exe -i " + RadianceOutputTB.Text + @"\channels\alpha\" + Path.GetFileName(image) + @" -o " + RadianceOutputTB.Text + @"\channels\dodged\alpha\" + Path.GetFileName(image) + @" -T -u -c " + RadianceANUD.Value + " -n -t 128 -g -0 +g 0 -k 15 -f " + RadianceFillNUD.Value);
                                        swbat.WriteLine("mr_file.exe -T -i " + RadianceNumBandsTB.Text + " -o 1 -E -K g -S 256 -C j -Q 5 " + RadianceOutputTB.Text + @"\channels\dodged\red\" + Path.GetFileName(image) + " " + RadianceOutputTB.Text + @"\channels\dodged\green\" + Path.GetFileName(image) + " " + RadianceOutputTB.Text + @"\channels\dodged\blue\" + Path.GetFileName(image) + " " + RadianceOutputTB.Text + @"\channels\dodged\alpha\" + Path.GetFileName(image) + " " + RadianceOutputTB.Text + @"\completed\" + Path.GetFileName(image));
                                    }
                                    else
                                    {
                                        swbat.WriteLine("mr_file.exe -T -i " + RadianceNumBandsTB.Text + " -o 1 -E -K g -S 256 -C j -Q 5 " + RadianceOutputTB.Text + @"\channels\dodged\red\" + Path.GetFileName(image) + " " + RadianceOutputTB.Text + @"\channels\dodged\green\" + Path.GetFileName(image) + " " + RadianceOutputTB.Text + @"\channels\dodged\blue\" + Path.GetFileName(image) + " " + RadianceOutputTB.Text + @"\completed\" + Path.GetFileName(image));
                                    }

                                    swbat.WriteLine("del /s " + RadianceOutputTB.Text + @"\channels\" + Path.GetFileName(image));
                                    swbat.WriteLine("move " + RadianceInputTB.Text + @"\" + Path.GetFileNameWithoutExtension(image) + ".t* " + RadianceInputTB.Text + @"\done");
                                    swbat.WriteLine(@"del \\pinmapnas01\projects\3.CONDOR\CondorBat\Radiance-" + Path.GetFileNameWithoutExtension(image) + ".bat");
                                }
                            }
                            break;
                        #endregion

                    }
                }
                SubmitProgressBar.Value = 0;
                if (preppingSubmit)
                {
                    switch (ProcessCombo.Text)
                    {
                        case "AT":
                            PrepATBW.RunWorkerAsync(ATBlocks);
                            break;
                        case "Rectify":
                            PrepRectifyBW.RunWorkerAsync(rectifyJob);
                            break;
                    }
                    StatusLabel.Text = "Preparing...";

                }
                else
                {
                    SubmitBW.RunWorkerAsync(GetJobCount());
                    StatusLabel.Text = "Submitting...";
                }

            }
            else
            {
                MessageBox.Show("I'm already submitting something, sheesh!");
            }
        }

        private void CheckPriority()
        {
            int numToBeSubmitted = GetJobCount();
            //Check to make sure we aren't locking up the pool with a large number of "Highest" priority jobs
            if ((numToBeSubmitted > 100) && (PriorityCombo.Text == "Highest"))
            {
                PriorityWarning form = new PriorityWarning(this);
                form.ShowDialog(this);
            }
        }

        private int GetJobCount()
        {
            int numToBeSubmitted = 0;
            switch (ProcessCombo.Text)
            {
                case "AT":
                    numToBeSubmitted = ATBlockListView.SelectedItems.Count;
                    break;
                case "Grid Points":
                    numToBeSubmitted = Directory.GetFiles(GridPointsInputTB.Text).Length;
                    break;
                case "Image Conversion":
                    foreach (ListViewItem item in ImageConvertInputLV.Items)
                    {
                        if (item.Text.Contains(".txt"))
                        {
                            numToBeSubmitted += File.ReadAllLines(item.Text).Length;
                        }
                        else
                        {
                            numToBeSubmitted += Directory.GetFiles(item.Text, "*.tif").Length;
                        }
                    }
                    break;
                case "Mosaic":
                    numToBeSubmitted = MosaicListView.SelectedItems.Count;
                    break;
                case "MrSID Encoding":
                    foreach (ListViewItem item in ImageConvertInputLV.Items)
                    {
                        numToBeSubmitted += Directory.GetFiles(item.Text, "*.sid").Length;
                    }
                    break;
                case "Radiance":

                    break;
                case "Rectify":
                    numToBeSubmitted = RectifyListView.SelectedItems.Count;
                    break;
                case "Resize/Resample":
                    numToBeSubmitted += Directory.GetFiles(RRInputTB.Text, "*.tif").Length;
                    break;
                case "Thumbs":
                    numToBeSubmitted += Directory.GetFiles(ThumbsInputFolderTB.Text, "*.tif").Length;
                    break;
            }
            return numToBeSubmitted;
        }

        private bool CheckForm(Form form)
        {
            foreach (Form f in Application.OpenForms)
                if (form == f)
                    return true;

            return false;
        }

        public string GetUniversalName(string originalPath)
        {
            StringBuilder sb = new StringBuilder(512);
            int size = sb.Capacity;

            // look for the {LETTER}: combination ...
            if (originalPath.Length > 2 && originalPath[1] == ':')
            {
                // don't use char.IsLetter here - as that can be misleading
                // the only valid drive letters are a-z && A-Z.
                char c = originalPath[0];
                if ((c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z'))
                {
                    int error = WNetGetConnection(originalPath.Substring(0, 2),
                        sb, ref size);

                    if (error == 0)
                    {
                        DirectoryInfo dir = new DirectoryInfo(originalPath);

                        string path = Path.GetFullPath(originalPath).Substring(Path.GetPathRoot(originalPath).Length);
                        return Path.Combine(sb.ToString().TrimEnd(), path);
                    }
                }
            }

            return originalPath;
        }

        static void GlobalExceptionHandler(object sender, UnhandledExceptionEventArgs args)
        {
            Exception e = (Exception)args.ExceptionObject;
            StreamWriter sw = new StreamWriter(@"C:\Condor\gui.log", true);
            sw.WriteLine("CondorSubmit GUI caught : " + DateTime.UtcNow + '\t' + e.Message);
            sw.Close();
        }

        private void SortListView(ListView currentListView, ColumnClickEventArgs e)
        {
            // Create an instance of a ListView column sorter and assign it 
            // to the ListView control.
            currentListView.ListViewItemSorter = lvwColumnSorter;
            // Determine if clicked column is already the column that is being sorted.
            if (e.Column == lvwColumnSorter.SortColumn)
            {
                // Reverse the current sort direction for this column.
                if (lvwColumnSorter.Order == SortOrder.Ascending)
                {
                    lvwColumnSorter.Order = SortOrder.Descending;
                }
                else
                {
                    lvwColumnSorter.Order = SortOrder.Ascending;
                }
            }
            else
            {
                // Set the column number that is to be sorted; default to ascending.
                lvwColumnSorter.SortColumn = e.Column;
                lvwColumnSorter.Order = SortOrder.Ascending;
            }

            // Perform the sort with these new sort options.
            currentListView.Sort();
        }

        #endregion

        #region Background Workers

        #region SubmitBW
        private void SubmitBW_DoWork(object sender, DoWorkEventArgs e)
        {
            //since the eidCounter is an int we first cast the argument variable to int and then to double 
            //double since when calculating the 
            int numToBeSubmitted = (int)e.Argument;
            int numSubmitted = 0;

            // Start the child process.
            Process p = new Process();
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.CreateNoWindow = true;
            p.StartInfo.FileName = "condor_submit.exe";
            p.StartInfo.Arguments = @"-name cluster_01.pinnaclemapping.lcl C:\condorsubmitgui.sub";
            p.StartInfo.RedirectStandardOutput = true;
            p.StartInfo.WorkingDirectory = @"C:\";
            p.Start();

            //count the .'s to determine submit progress
            char[] charStandardOutput = new char[1];
            int progress = 0;
            while (!p.HasExited)
            {
                p.StandardOutput.Read(charStandardOutput, 0, 1);
                if (charStandardOutput[0] == '.') numSubmitted++;
                if (numSubmitted < numToBeSubmitted) progress = (int)((float)numSubmitted / (float)numToBeSubmitted * 100);
                SubmitBW.ReportProgress(progress);
            }
            SubmitBW.ReportProgress(100);
            p = new Process();
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.CreateNoWindow = true;
            p.StartInfo.FileName = "condor_reschedule.exe";
            p.StartInfo.Arguments = "-all";
            p.Start();

        }

        private void SubmitBW_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            SubmitProgressBar.Value = e.ProgressPercentage;
        }

        private void SubmitBW_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            CurrentPanel.Enabled = true;
            ProcessCombo.Enabled = true;
            StatusLabel.Text = "Submitted!";
            Process p = new Process();
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.CreateNoWindow = true;
            p.StartInfo.FileName = "condor_reschedule.exe";
            p.StartInfo.Arguments = @"-all";
            p.StartInfo.RedirectStandardOutput = true;
            p.Start();
        }
        #endregion

        #region PrepATBW
        private void PrepATBW_DoWork(object sender, DoWorkEventArgs e)
        {
            //Load in background worker argument object (basically the selected blocks)
            //and prepare the variables for the progress bar
            List<string> ATBlocks = (List<string>)e.Argument;
            int numToBeSubmitted = ATBlocks.Count;
            int numSubmitted = 0;
            int progress = 0;

            //Create each selected block's folder
            foreach (string block in ATBlocks)
            {
                ATBlock currentBlock = currentATProject.FindBlock(block);
                if (Directory.Exists(currentATProject.atDirectory + @"\" + currentBlock.blockName)) Directory.Delete(currentATProject.atDirectory + @"\" + currentBlock.blockName, true);

                //Copy files from main project to block subfolder
                Directory.CreateDirectory(currentATProject.atDirectory + @"\" + currentBlock.blockName);
                File.Copy(currentATProject.atDirectory + @"\project", currentATProject.atDirectory + @"\" + currentBlock.blockName + @"\project");
                File.Copy(currentATProject.atDirectory + @"\camera", currentATProject.atDirectory + @"\" + currentBlock.blockName + @"\camera");
                File.Copy(currentATProject.atDirectory + @"\control", currentATProject.atDirectory + @"\" + currentBlock.blockName + @"\control");
                File.Copy(currentATProject.atDirectory + @"\triang", currentATProject.atDirectory + @"\" + currentBlock.blockName + @"\triang");
                if (File.Exists(currentATProject.atDirectory + @"\pcgrids.dat")) File.Copy(currentATProject.atDirectory + @"\pcgrids.dat", currentATProject.atDirectory + @"\" + currentBlock.blockName + @"\pcgrids.dat");
                Directory.CreateDirectory(currentATProject.atDirectory + @"\" + currentBlock.blockName + @"\csf");
                File.Copy(currentATProject.atDirectory + @"\csf\project.csf", currentATProject.atDirectory + @"\" + currentBlock.blockName + @"\csf\project.csf");

                string currentLine = "";
                double avgFlyingHeight = 0;
                using (StreamWriter sw = new StreamWriter(currentATProject.atDirectory + @"\" + currentBlock.blockName + @"\photo"))
                {

                    //Create block photo file by looping through the images in the block and
                    //finding their corresponding photo_measurements and parameters and
                    //writing them to the seperate photo file
                    List<string> photosToRemove = new List<string>();
                    foreach (string photo in currentBlock.blockPhotos)
                    {
                        Photo currentPhoto = currentATProject.FindPhoto(photo);
                        if (currentPhoto != null)
                        {
                            sw.WriteLine("begin photo_measurements " + currentPhoto.photoName + "\tstrip_id " + currentPhoto.flight);

                            //import photo measurements to new block or not
                            if (ATLeavePointsCB.Checked)
                            {
                                if (currentPhoto.photoMeasurements != null)
                                {
                                    foreach (string measurement in currentPhoto.photoMeasurements)
                                    {
                                        sw.WriteLine(measurement);
                                    }
                                }
                            }
                            sw.WriteLine("end photo_measurements");
                            sw.Write(sw.NewLine);
                        }
                        else
                        {
                            photosToRemove.Add(photo);
                        }

                    }
                    foreach (string photo in photosToRemove)
                    {
                        currentBlock.blockPhotos.Remove(photo);
                    }
                    //write the specific photo parameters from the parent project to the photo file for the current block.
                    foreach (string photo in currentBlock.blockPhotos)
                    {

                        Photo currentPhoto = currentATProject.FindPhoto(photo);
                        sw.WriteLine("begin photo_parameters " + currentPhoto.photoName + "\tstrip_id " + currentPhoto.flight);

                        if (currentPhoto.cameraName != null) sw.WriteLine(" camera_name:\t" + currentPhoto.cameraName);
                        if (currentPhoto.cameraOrient != null) sw.WriteLine(" camera_orientation:\t" + currentPhoto.cameraOrient);
                        if (currentPhoto.filePath != null) sw.WriteLine(" image_id:\t" + currentPhoto.filePath);
                        if (currentPhoto.photoKey != null) sw.WriteLine(" key:\t" + currentPhoto.photoKey);
                        if (currentPhoto.gpsTimestamp != null) sw.WriteLine(" GPS_TimeStamp:\t" + currentPhoto.gpsTimestamp);
                        if (currentPhoto.viewGeometry != null) sw.WriteLine(" view_geometry: " + currentPhoto.viewGeometry);
                        if (currentPhoto.eoParams != null) sw.WriteLine(" EO_parameters:" + currentPhoto.eoParams);
                        if (currentPhoto.givenParams != null) sw.WriteLine(" GIVEN_parameters:" + currentPhoto.givenParams);
                        if (currentPhoto.givenStdDev != null) sw.WriteLine(" GIVEN_std_devs:" + currentPhoto.givenStdDev);
                        if (currentPhoto.footprintCoords != null) sw.WriteLine(" footprint: " + currentPhoto.footprintCoords);
                        if (currentPhoto.activeElevation != null) sw.WriteLine(" active_elevation: " + currentPhoto.activeElevation);
                        if (currentPhoto.driveType != null) sw.WriteLine(" DRIVE_type: " + currentPhoto.driveType);
                        if (currentPhoto.imageSize != null) sw.WriteLine(" image_size:\t" + currentPhoto.imageSize);
                        if (currentPhoto.sensorID != null) sw.WriteLine(" sensor_id:\t" + currentPhoto.sensorID);
                        sw.WriteLine("end photo_parameters");
                        sw.Write(sw.NewLine);
                        avgFlyingHeight += currentPhoto.flyingHeight;

                    }
                    avgFlyingHeight /= currentBlock.blockPhotos.Count;

                    //write the specific block photos to the photo file for the current block.
                    sw.WriteLine("begin block " + currentBlock.blockName);
                    foreach (string photo in currentBlock.blockPhotos)
                    {
                        Photo currentPhoto = currentATProject.FindPhoto(photo);
                        sw.WriteLine("  " + currentPhoto.photoName + "  strip_id " + currentPhoto.flight);
                    }
                    sw.WriteLine("end block");
                    sw.Write(sw.NewLine);

                    //Finished writing this blocks photo file
                }

                //Find ground elevation
                double avgElevation = 0;
                if (ATCalculateGround.Checked)
                {
                    using (StreamReader sr = new StreamReader(currentATProject.atDirectory + @"\elevationdb"))
                    {
                        currentLine = sr.ReadLine();
                        if (currentLine.Split(',').Length == 2)
                        {
                            foreach (string photo in currentBlock.blockPhotos)
                            {
                                while (!currentLine.Split(',')[0].Equals(photo.ToString()))
                                {
                                    currentLine = sr.ReadLine();
                                }
                                try
                                {
                                    avgElevation += Convert.ToDouble(currentLine.Split(',')[1]);
                                    sr.BaseStream.Seek(0, SeekOrigin.Begin);
                                }
                                catch
                                {
                                }
                            }
                        }
                        else
                        {
                            MessageBox.Show("Unexpected number of columns in elevation db.\nExpected 2. Using at project ground elevation.");
                        }
                    }
                    avgElevation /= currentBlock.blockPhotos.Count;
                }

                //copy the project to the block's project file and insert the averaged ground elevation of the block 
                using (StreamWriter sw = new StreamWriter(currentATProject.atDirectory + @"\" + currentBlock.blockName + @"\project"))
                {
                    using (StreamReader sr = new StreamReader(currentATProject.atDirectory + @"\project"))
                    {


                        //insert the average flying height (of the block) into the project file
                        while ((currentLine = sr.ReadLine()) != null)
                        {
                            if (currentLine.Contains("flying_height"))
                            {
                                sw.WriteLine(" flying_height:\t" + avgFlyingHeight.ToString());
                                if (ATCalculateGround.Checked)
                                {
                                    sw.WriteLine(" average_elev_grnd:\t" + avgElevation.ToString());
                                    sr.ReadLine();
                                }
                                sw.WriteLine(sr.ReadToEnd());
                            }
                            else
                            {
                                sw.WriteLine(currentLine);
                            }
                        }
                    }
                }

                //find the pixel size from the camera file
                double pixelSize = 0;
                using (StreamReader sr = new StreamReader(currentATProject.atDirectory + @"\camera"))
                {
                    while ((currentLine = sr.ReadLine()) != null)
                    {
                        if (currentLine.Contains("media_type"))
                        {
                            if (currentLine.Contains("film"))
                            {
                                pixelSize = 12;
                                break;
                            }
                        }
                        if (currentLine.Contains("pixel_size"))
                        {
                            Regex rx = new Regex(@"\spixel_size:(\s)*(?<pixelsize>.*?)\s");
                            pixelSize = Convert.ToDouble(rx.Match(currentLine).Groups["pixelsize"].Value);
                            pixelSize = Math.Round(pixelSize);
                            break;
                        }

                    }
                }

                //find block number from the running count, update the count, and then write out block ste
                int blockNum = 0;
                if (File.Exists(currentATProject.atDirectory + @"\blockorder"))
                {
                    StreamReader blockOrderSR = new StreamReader(currentATProject.atDirectory + @"\blockorder");
                    blockNum = Convert.ToInt16(blockOrderSR.ReadLine());
                    blockOrderSR.Close();
                }
                blockNum++;
                using (StreamWriter blockOrderSW = new StreamWriter(currentATProject.atDirectory + @"\blockorder"))
                {
                    blockOrderSW.Write(blockNum);
                }

                //Create the .ste file
                using (StreamWriter sw = new StreamWriter(currentATProject.atDirectory + @"\" + currentBlock.blockName + @"\" + currentBlock.blockName + @".ste"))
                {
                    sw.WriteLine("PHO_PROJECTPATH         " + currentATProject.atDirectory);
                    sw.WriteLine("PHO_BLOCKNAME           " + currentBlock.blockName);
                    sw.WriteLine("PHO_BLOCKNO             " + blockNum);
                    sw.WriteLine("PHO_TRACK_LEVEL         0    " + pixelSize.ToString());
                    sw.WriteLine("PHO_TRACK_NPTS          3");
                    sw.WriteLine("PHO_PHOTOWRITE_MODE       1                           # => 0/1       -> No/Yes");
                    if (ATUseGPSCheckBox.Checked)
                    {
                        sw.WriteLine("PHO_GPSINSEO_MODE         1                           # => 0/1       -> No/Yes");
                    }
                    else
                    {
                        sw.WriteLine("PHO_GPSINSEO_MODE         0                           # => 0/1       -> No/Yes");
                    }
                    sw.WriteLine("PHO_QCQA_MODE             0                           # => 0/1       -> No/Yes");
                    sw.WriteLine("PHO_AUTO_ENHANCE_MODE     1                           # => 0/1       -> No/Yes");
                    sw.WriteLine("PHO_SHADOW_POINTS_MODE    0                           # => 0/1       -> No/Yes");
                    sw.WriteLine("PHO_COLORCHANNEL_MODE     4                           # => 0/1/2/3/4 -> BW/Red/Green/Blue/Intensity");
                    sw.WriteLine("PHO_PIXELBITS             8");
                    sw.WriteLine("PHO_AATRUN_MODE           0");
                    sw.WriteLine("STRINGENT_MATCHING        1                           # => 0/1       -> No/Yes");
                    sw.WriteLine("THINWEAK_REMOVESINGLES    1                           # => 0/1       -> No/Yes");
                    sw.WriteLine("THINWEAK_THINNINGMODE     2                           # => 0/1/2     -> No thinning/Thin on 3x3/Thin on 5x5");
                    sw.WriteLine("THINWEAK_WEAKANALYSISMODE 2                           # => 0/1/2     -> No analysis/Analyze on 3x3/Analyze on 5x5");
                    sw.WriteLine("THINWEAK_PARAMETERSET     THIN");
                }
            }

            //update the progress and move on to the next block
            numSubmitted++;
            if (numSubmitted < numToBeSubmitted) progress = (int)((float)numSubmitted / (float)numToBeSubmitted * 100);
            PrepATBW.ReportProgress(progress);
        }

        private void PrepATBW_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            SubmitProgressBar.Value = e.ProgressPercentage;
        }

        private void PrepATBW_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (submitAT)
            {
                SubmitBW.RunWorkerAsync();
                submitAT = false;
                StatusLabel.Text = "Submitting";
            }
            else
            {
                StatusLabel.Text = "Idle";
            }
        }
        #endregion

        #region MergeATBW
        private void MergeATBW_DoWork(object sender, DoWorkEventArgs e)
        {

        }

        private void MergeATBW_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            SubmitProgressBar.Value = e.ProgressPercentage;
            ATGetBlockStatus();
        }

        private void MergeATBW_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            StatusLabel.Text = "Idle";
        }
        #endregion

        #region PrepRectifyBW
        private void PrepRectifyBW_DoWork(object sender, DoWorkEventArgs e)
        {
            RectifyJob rectifyJob = (RectifyJob)e.Argument;

            int numToBePrepped = rectifyJob.photos.Count + rectifyJob.elevationTiles.Count;
            int numPrepped = 0;
            int progress = 0;
            int nullJobs = 0;

            using (StreamWriter sw = new StreamWriter(@"C:\condorsubmitgui.sub", true))
            {
                if (RectifyGenerateFOFs.Checked)
                {
                    foreach (ElevationTile currentTile in rectifyJob.elevationTiles)
                    {
                        currentTile.initElevationTile();
                    }
                }
                //for each photo to be rectified 
                foreach (Photo currentPhoto in rectifyJob.photos)
                {
                    if (RectifyGenerateFOFs.Checked)
                    {
                        string[] footprintItems = new string[7];
                        footprintItems = currentPhoto.footprintCoords.Split(' ');
                        List<Point> footprintPoints = new List<Point>();
                        for (int i = 0; i < 8; i += 2)
                        {
                            float currentX = float.Parse(footprintItems[i]);
                            float currentY = float.Parse(footprintItems[i + 1]);
                            footprintPoints.Add(new Point(currentX, currentY));
                        }

                        Directory.CreateDirectory(currentATProject.atDirectory + @"\fof");
                        if (File.Exists(currentATProject.atDirectory + @"\fof\" + currentPhoto.photoName + ".fof")) File.Delete(currentATProject.atDirectory + @"\fof\" + currentPhoto.photoName + ".fof");
                        using (StreamWriter fofsw = new StreamWriter(currentATProject.atDirectory + @"\fof\" + currentPhoto.photoName + ".fof", true))
                        {
                            foreach (ElevationTile currentTile in rectifyJob.elevationTiles)
                            {
                                if (currentTile.isIntersecting(currentPhoto)) fofsw.WriteLine(currentTile.filePath + "|" + currentATProject.atDirectory + @"\csf\project.csf");
                            }
                        }
                    }
                    try
                    {
                        if ((RectifyOverwriteCB.Checked) || ((!RectifyOverwriteCB.Checked) && (!File.Exists(rectifyJob.rectifyOutput + @"\O" + currentPhoto.photoName + ".tif"))))
                        {
                            string jobName = "Rectify-" + ProjectTB.Text + "-" + currentPhoto.photoName;
                            //make the command 
                            using (StreamWriter swbat = new StreamWriter(@"\\pinmapnas01\projects\3.CONDOR\CondorBat\" + jobName + ".bat"))
                            {
                                swbat.WriteLine(@"set path=%PATH%;C:\Program Files\ImageStation OrthoPro;C:\Program Files (x86)\ImageStation OrthoPro");
                                swbat.WriteLine(@"mkdir C:\temp\processed");
                                swbat.WriteLine(@"xcopy /y /d " + "\"" + currentPhoto.filePath + "\" C:\\temp");
                                swbat.Write(@"CSRectPhoto -i C:\temp\" + currentPhoto.photoName + ".tif -R " + currentPhoto.flight + " -p " + currentPhoto.photoName + " -b ");
                                swbat.Write(currentATProject.atDirectory + " -M -E " + currentATProject.atDirectory + @"\fof\" + currentPhoto.photoName + @".fof -Z -F " + rectifyJob.voidColor + " -W -B -r cc -c -0.667 -f ");
                                swbat.Write(rectifyJob.pixelSpacing + " -s " + rectifyJob.pixelSize + " -D -O " + rectifyJob.outputCSF + @" -o C:\temp\processed\O" + currentPhoto.photoName + ".tif -T tiff ");
                                if (rectifyJob.compress)
                                {
                                    swbat.Write("-w 31 -Q 5 ");
                                }
                                else
                                {
                                    swbat.Write("-w 28 ");
                                }
                                swbat.WriteLine("-u 256 -v 99 -y gaussian");
                                swbat.WriteLine(@"xcopy /y /d C:\temp\processed\O" + currentPhoto.photoName + ".t* \"" + rectifyJob.rectifyOutput + "\"");
                                swbat.WriteLine(@"del /s C:\temp\*" + currentPhoto.photoName + ".*");
                                swbat.WriteLine(@"del \\pinmapnas01\projects\3.CONDOR\CondorBat\" + jobName + ".bat");
                            }

                            sw.WriteLine(@"executable=X:\3.CONDOR\CondorBat\" + jobName + ".bat");
                            sw.WriteLine("+JobName=\"" + jobName + "\"");
                            sw.WriteLine(@"output=X:\3.CONDOR\CondorLogs\" + jobName + ".out");
                            sw.WriteLine(@"error=X:\3.CONDOR\CondorLogs\" + jobName + ".err");
                            sw.WriteLine(@"log=X:\3.CONDOR\CondorLogs\" + jobName + ".log");
                            sw.WriteLine("queue");
                        }
                        numPrepped++;
                        if (numPrepped < numToBePrepped) progress = (int)((float)numPrepped / (float)numToBePrepped * 100);
                        PrepRectifyBW.ReportProgress(progress);
                    }
                    catch
                    {
                        nullJobs++;
                    }
                }
            }
        }

        private void PrepRectifyBW_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            SubmitProgressBar.Value = e.ProgressPercentage;
        }

        private void PrepRectifyBW_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            StatusLabel.Text = "Submitting";
            SubmitBW.RunWorkerAsync(GetJobCount());
        }
        #endregion

        private void RectifyEveryOther_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < RectifyListView.Items.Count - 1; i += 2)
            {
                RectifyListView.Items[i].Selected = true;
            }
        }

        #endregion

    }
}
