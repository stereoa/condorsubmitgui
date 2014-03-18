using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CondorSubmitGUI.Objects.Queue
{
    class LogEvent
    {
        public List<LogEventAttribute> eventAttributes = new List<LogEventAttribute>();
        public int clusterId;
        public DateTime eventTime;

        public LogEvent(List<LogEventAttribute> eventAttributes)
        {
            this.eventAttributes = eventAttributes;
            this.clusterId = Convert.ToInt32(eventAttributes.Find(p => p.name.Equals("Cluster")).value);
            this.eventTime = DateTime.Parse(eventAttributes.Find(p => p.name.Equals("EventTime")).value);

        }
    }

    class SubmitEvent : LogEvent
    {
        public string submitHost;

        public SubmitEvent(List<LogEventAttribute> eventAttributes)
            : base(eventAttributes)
        {
            this.submitHost = eventAttributes.Find(p => p.name.Equals("SubmitHost")).value;
        }
    }

    class ExecuteEvent : LogEvent
    {
        public string executeHost;

        public ExecuteEvent(List<LogEventAttribute> eventAttributes)
            : base(eventAttributes)
        {
            this.executeHost = eventAttributes.Find(p => p.name.Equals("ExecuteHost")).value;
        }
    }

    class TerminatedEvent : LogEvent
    {
        public bool successful;

        public TerminatedEvent(List<LogEventAttribute> eventAttributes)
            : base(eventAttributes)
        {
            this.successful = Convert.ToBoolean(eventAttributes.Find(p => p.name.Equals("TerminatedNormally")).value);
        }
    }

    class LogEventAttribute
    {
        public string name, type, value;
        public LogEventAttribute(string name, string type, string value)
        {
            this.name = name;
            this.type = type;
            this.value = value;
        }
    }
}
