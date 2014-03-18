using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using CondorSubmitGUI.Objects.Queue;

namespace CondorSubmitGUI
{
    public partial class JobStatus : Form
    {

        public string jobName;

        public JobStatus(string jobName)
        {
            InitializeComponent();  
            this.Text = jobName;
            this.jobName = jobName;
            ParseLogs();
        }

        private void ParseLogs()
        {
            if (File.Exists(@"\\pinmapnas01\projects\3.CONDOR\CondorLogs\" + jobName + ".log"))
            {
                Job currentJob = new Job(jobName);
                foreach (LogEvent currentEvent in currentJob.logEvents)
                {
                    ListViewItem item = new ListViewItem();
                    item.Text = currentEvent.eventTime.ToString();
                   
                    switch (currentEvent.GetType().Name)
                    {
                        case "ExecuteEvent":
                            ExecuteEvent currentExEvent = currentEvent as ExecuteEvent;
                            item.SubItems.Add("Job executed by " + currentExEvent.executeHost);
                            break;
                        case "SubmitEvent":
                            SubmitEvent currentSubEvent = currentEvent as SubmitEvent;
                            item.SubItems.Add("Job executed by " + currentSubEvent.submitHost);
                            break;
                        case "TerminatedEvent":

                            break;

                        case "HeldEvent":
                            break;
                    }
                    eventsLV.Items.Add(item);
                }
            }
                
            if (File.Exists(@"\\pinmapnas01\projects\3.CONDOR\CondorLogs\" + jobName + ".out"))
            {
                using (StreamReader sr = new StreamReader(@"\\pinmapnas01\projects\3.CONDOR\CondorLogs\" + jobName + ".out"))
                {
                    outputTB.Text = sr.ReadToEnd();
                }
            }
            else
            {

            }

            if (File.Exists(@"\\pinmapnas01\projects\3.CONDOR\CondorLogs\" + jobName + ".err"))
            {
                using (StreamReader sr = new StreamReader(@"\\pinmapnas01\projects\3.CONDOR\CondorLogs\" + jobName + ".err"))
                {
                    errorTB.Text = sr.ReadToEnd();
                }
            }
            else
            {
                
            }
        }
    }
}
