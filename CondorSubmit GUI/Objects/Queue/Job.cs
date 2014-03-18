using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace CondorSubmitGUI.Objects.Queue
{
    class Job
    {
        public List<LogEvent> logEvents = new List<LogEvent>();
        public int clusterID;
        public string jobName;

        public Job(string jobName)
        {
            this.jobName = jobName;
            
            string logContents = null;
            using (StreamReader sr = new StreamReader(@"\\pinmapnas01\projects\3.CONDOR\CondorLogs\" + jobName + ".log"))
            {
                logContents = sr.ReadToEnd();
            }
            logContents = logContents.Replace("<b v=\"t\"/>","<b>True</b>");
            logContents = logContents.Replace("<b v=\"f\"/>","<b>False</b>");
            Regex eventParserRegex = new Regex(@"<c>\r\n(\x20+<a n=""(?<name>.*?)""><(?<type>.)>(?<value>.*?)</.></a>\r\n)*?</c>", RegexOptions.Singleline);

            foreach (Match match in eventParserRegex.Matches(logContents))
            {
                //parse out each events attributes
                List<LogEventAttribute> currentEventAttributes = new List<LogEventAttribute>();
               
                for (int i = 0; i < match.Groups["name"].Captures.Count; i++)
                {
                    string name = match.Groups["name"].Captures[i].Value;
                    string type = match.Groups["type"].Captures[i].Value;
                    string value = match.Groups["value"].Captures[i].Value;
                    currentEventAttributes.Add(new LogEventAttribute(name,type,value));
                }
                //find what type of event it is and create the corresponding object
                LogEventAttribute eventTypeAttribute = currentEventAttributes.Find(p => p.name.Equals("MyType"));
                string eventType = eventTypeAttribute.value;
                switch (eventType)
                {
                    case "SubmitEvent":
                        logEvents.Add(new SubmitEvent(currentEventAttributes));
                        break;
                    case "ExecuteEvent":
                        logEvents.Add(new ExecuteEvent(currentEventAttributes));
                        break;
                    case "TerminatedEvent":
                        logEvents.Add(new TerminatedEvent(currentEventAttributes));
                        break;
                    default:
                        logEvents.Add(new LogEvent(currentEventAttributes));
                        break;
                }

                
            }
            clusterID = logEvents[0].clusterId;

        }
    }
}
