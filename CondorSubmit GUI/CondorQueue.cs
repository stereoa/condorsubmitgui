using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;
using CondorSubmitGUI.Objects.Queue;

namespace CondorSubmitGUI
{
    public partial class CondorQueue : Form
    {
        private ListViewColumnSorter lvwColumnSorter = new ListViewColumnSorter();
        private List<string> queueProcessList = new List<string>();
        private string currentProcess = "None";
        private int queueProcessPriorityLevel = 0;

        public CondorQueue()
        {
            InitializeComponent();
            QueuePriorityCombo.SelectedIndex = 2;
            GrabQueue();
        }

        private void GrabQueue()
        {
            QueueListView.Items.Clear();
            
            //First find jobs reported by condor

            Process p = new Process();
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.CreateNoWindow = true;
            p.StartInfo.FileName = "condor_q.exe";
            //Opsys doesn't matter, but the format flag won't just take characters it wants an attribute also, so I give it OpSys, since it is always set.
            p.StartInfo.Arguments = "-global -format %i| ClusterId -format %s| JobName -format %i| JobStatus -format %i| JobPrio -format %s RemoteHost -format \\n Opsys"; 
            p.StartInfo.RedirectStandardOutput = true;
            p.Start();
            
            string currentLine = null;
            string[] queueLine;
            int rowNumber = 0;
            
            QueueListView.ListViewItemSorter = null;
            
            try
            {
                while (p.StandardOutput.Peek() >= 0)
                {
                    currentLine = p.StandardOutput.ReadLine();
                    queueLine = currentLine.Split('|');
                    string id = queueLine[0];
                    string name = queueLine[1];
                    
                    int statusNum = 99;
                    int priorityNum = 99;
                    statusNum = Convert.ToInt16(queueLine[2]);
                    priorityNum = Convert.ToInt16(queueLine[3]);
                    string priority = "Unknown?";
                    string status = "Unknown?";
                    string node = "-------";

                    switch (statusNum)
                    {
                        case 0:
                            status = "Unexpanded";
                            break;
                        case 1:
                            status = "Idle";
                            break;
                        case 2:
                            status = "Running";
                            break;
                        case 3:
                            status = "Removed";
                            break;
                        case 4:
                            status = "Completed";
                            break;
                        case 5:
                            status = "Held";
                            break;
                        case 6:
                            status = "Error";
                            break;
                    }
                    switch (priorityNum)
                    {
                        case 20:
                            priority = "Highest";
                            break;
                        case 10:
                            priority = "High";
                            break;
                        case 0:
                            priority = "Medium";
                            break;
                        case -10:
                            priority = "Low";
                            break;
                        case -20:
                            priority = "Lowest";
                            break;
                        case 99:
                            priority = "UNKNOWN?";
                            break;
                    }
                    
                    if (status == "Running") 
                    {
                        node = queueLine[4];
                    }
                    
                    addJobLineToQueue(id,name,status,priority,node);
                    rowNumber++;
                }
            }
            catch
            {
            }
            
            // Then find jobs that aren't in the queue but we have logs for.

            /*string[] logFiles = Directory.GetFiles(@"\\pinmapnas01\projects\3.CONDOR\CondorLogs", "*.log");
            if (File.Exists(@"\\pinmapnas01\projects\3.CONDOR\cache")) File.Move(@"\\pinmapnas01\projects\3.CONDOR\cache",@"\\pinmapnas01\projects\3.CONDOR\CondorLogs\cache.old");
            List<Job> cacheJobs = new List<Job>();
            using (StreamReader sr = new StreamReader(@"\\pinmapnas01\projects\3.CONDOR\CondorLogs\cache.old"))
            {
                string[] jobsFromCache = Regex.Split(sr.ReadToEnd(),"\r\n");
                foreach (string job in jobsFromCache)
                {
                    string[] jobProperties = job.Split(',');
                    
                }
            }
            using (StreamWriter sw = new StreamWriter(@"\\pinmapnas01\projects\3.CONDOR\CondorLogs\cache"))
            {

            }       
            {
                string jobName = Path.GetFileNameWithoutExtension(logFile);
                if (QueueListView.FindItemWithText(jobName) == null)
                {
                    Job currentJob = new Job(jobName);
                    addJobLineToQueue(currentJob.clusterID.ToString(), currentJob.jobName, "Completed", "-------", "-------");
                    rowNumber++;
                }
                
            }*/

            // Then sort that shit

            QueueNumber.Text = "Job Count: " + rowNumber;
            lvwColumnSorter.SortColumn = 3;
            lvwColumnSorter.Order = SortOrder.Descending;
            QueueListView.ListViewItemSorter = lvwColumnSorter;
            QueueListView.Sort();
            lvwColumnSorter.SortColumn = 4;
            lvwColumnSorter.Order = SortOrder.Ascending;
            QueueListView.ListViewItemSorter = lvwColumnSorter;
            QueueListView.Sort();
        }

        private void addJobLineToQueue(string id, string name, string status, string priority, string node)
        {
            ListViewItem item = new ListViewItem();
            item.Text = id;
            item.SubItems.Add(name);
            item.SubItems.Add(status);
            item.SubItems.Add(priority);
            item.SubItems.Add(node);
            QueueListView.Items.Add(item);
        }

        private void QueueRefreshButton_Click(object sender, EventArgs e)
        {
            GrabQueue();
        }

        private void QueueHoldButton_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem item in QueueListView.SelectedItems)
            {
                queueProcessList.Add(item.Text);
            }
            currentProcess = "Hold";
            QueueRefreshButton.Enabled = false;
            QueueHoldButton.Enabled = false;
            QueueRemoveButton.Enabled = false;
            QueueSetPriorityButton.Enabled = false;
            QueueProcessBW.RunWorkerAsync(queueProcessList);
        }

        private void QueueRemoveButton_Click(object sender, EventArgs e)
        {
            if (QueueListView.SelectedItems.Count == QueueListView.Items.Count)
            {
                queueProcessList.Add("-all");
            }
            else
            {
                foreach (ListViewItem item in QueueListView.SelectedItems)
                {
                    queueProcessList.Add(item.Text);
                }
            }
            currentProcess = "Remove";
            QueueRefreshButton.Enabled = false;
            QueueHoldButton.Enabled = false;
            QueueRemoveButton.Enabled = false;
            QueueSetPriorityButton.Enabled = false;
            QueueProcessBW.RunWorkerAsync(queueProcessList);
        }

        private void QueueSetPriorityButton_Click(object sender, EventArgs e)
        {
            queueProcessList.Clear();
            switch (QueuePriorityCombo.Text)
            {
                case "Highest":
                    queueProcessPriorityLevel = 20;
                    break;
                case "High":
                    queueProcessPriorityLevel = 10;
                    break;
                case "Medium":
                    queueProcessPriorityLevel = 0;
                    break;
                case "Low":
                    queueProcessPriorityLevel = -10;
                    break;
                case "Lowest":
                    queueProcessPriorityLevel = -20;
                    break;
            } 
            foreach (ListViewItem item in QueueListView.SelectedItems)
            {
                queueProcessList.Add(item.Text);
            }
            currentProcess = "Priority";
            QueueRefreshButton.Enabled = false;
            QueueHoldButton.Enabled = false;
            QueueRemoveButton.Enabled = false;
            QueueSetPriorityButton.Enabled = false;
            QueueProcessBW.RunWorkerAsync(queueProcessList);
        }

        private void QueueListView_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            // Create an instance of a ListView column sorter and assign it 
            // to the ListView control.
            QueueListView.ListViewItemSorter = lvwColumnSorter;
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
            QueueListView.Sort();
        }

        private void QueueListView_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.A && e.Control)
            {
                foreach (ListViewItem item in QueueListView.Items)
                {
                    item.Selected = true;
                }
            }
        }

        private void QueueProcessBW_DoWork(object sender, DoWorkEventArgs e)
        {
            List<string> queueProcessList = (List<string>)e.Argument;
            double numToProcess = queueProcessList.Count;
            double numProcessed = 0;
            switch (currentProcess)
            {
                case "Priority":
                    foreach (string id in queueProcessList)
                    {
                        Process p = new Process();
                        p.StartInfo.UseShellExecute = false;
                        p.StartInfo.CreateNoWindow = true;
                        p.StartInfo.FileName = "condor_prio.exe";
                        p.StartInfo.Arguments = "-n cluster_01 -p " + queueProcessPriorityLevel + " " + id;
                        p.Start();
                        p.WaitForExit();
                        numProcessed++;
                        QueueProcessBW.ReportProgress(Convert.ToInt32((numProcessed / numToProcess) * 100));

                    }
                    break;
                case "Remove":
                    foreach (string id in queueProcessList)
                    {
                        Process p = new Process();
                        p.StartInfo.UseShellExecute = false;
                        p.StartInfo.CreateNoWindow = true;
                        p.StartInfo.FileName = "condor_rm.exe";
                        p.StartInfo.Arguments = "-name cluster_01 " + id;
                        p.Start();
                        p.WaitForExit();
                        numProcessed++;
                        QueueProcessBW.ReportProgress(Convert.ToInt32((numProcessed / numToProcess) * 100));
                    }
                    break;
                case "Hold":
                    foreach (string id in queueProcessList)
                    {
                        Process p = new Process();
                        p.StartInfo.UseShellExecute = false;
                        p.StartInfo.CreateNoWindow = true;
                        p.StartInfo.FileName = "condor_release.exe";
                        /*}
                        else
                        {
                            p.StartInfo.FileName = "condor_hold.exe";

                        }*/
                        p.StartInfo.Arguments = "-name cluster_01 " + id;
                        p.Start();
                        p.WaitForExit();
                        numProcessed++;
                        QueueProcessBW.ReportProgress(Convert.ToInt32((numProcessed / numToProcess) * 100));
                    }
                    break;
            }
        }

        private void QueueProcessBW_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            QueueProgressBar.Value = e.ProgressPercentage;
        }

        private void QueueProcessBW_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            GrabQueue();
            QueueRefreshButton.Enabled = true;
            QueueHoldButton.Enabled = true;
            QueueRemoveButton.Enabled = true;
            QueueSetPriorityButton.Enabled = true;
        }

        private void QueueListView_SelectedIndexChanged(object sender, EventArgs e)
        {
            QueueSelectCount.Text = QueueListView.SelectedItems.Count + " selected";
        }

        private void QueueListView_DoubleClick(object sender, EventArgs e)
        {
            string jobName = QueueListView.SelectedItems[0].SubItems[1].Text;
            if (File.Exists(@"\\pinmapnas01\projects\3.CONDOR\CondorLogs\" + jobName + ".log"))
            {
                JobStatus form = new JobStatus(jobName);
                form.ShowDialog();
            }
            else
            {
                MessageBox.Show("Log file doesn't exist!");
            }
        }
    }
}
