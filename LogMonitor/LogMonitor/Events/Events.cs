using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogMonitor
{
    public enum LogEventType
    {
        LogDataArrivedEvent,
        LogDataExtractedEvent,
        LogReportGenerateEvent,
        LogReportGeneratedEvent,
        ThresholdBreachCheckEvent,
        ThresholdBreachDetectedEvent,
        ThresholdBreachRecoveryCheckEvent,
        ThresholdBreachRecoveredEvent
    }

    public abstract class LogEvents
    {
        private dynamic runtimedata;
        private LogEventType type;

        public LogEventType Type
        {
            get { return type; }
            set { type = value; }
        }

        public dynamic ExecutionContext { get => runtimedata; set => runtimedata = value; }

        public LogEvents(LogEventType eventType)
        {
            type = eventType;
        }



    }
    public class LogTimeGenerateReportEvent : LogEvents
    {
        public LogTimeGenerateReportEvent()
            : base(LogMonitor.LogEventType.LogReportGenerateEvent)
        {

        }
    }

    public class LogTimeReportGeneratedEvent : LogEvents
    {
        public LogTimeReportGeneratedEvent()
            : base(LogMonitor.LogEventType.LogReportGeneratedEvent)
        {

        }
    }

    public class LogThresholdBreachCheckEvent : LogEvents
    {
        public LogThresholdBreachCheckEvent()
         : base(LogMonitor.LogEventType.ThresholdBreachCheckEvent)
        {

        }

    }

    public class LogThresholdBreachDetectedEvent : LogEvents
    {
        public LogThresholdBreachDetectedEvent()
         : base(LogMonitor.LogEventType.ThresholdBreachDetectedEvent)
        {

        }

    }

    public class LogThresholdBreachRecoveredEvent : LogEvents
    {
        public LogThresholdBreachRecoveredEvent()
            : base(LogMonitor.LogEventType.ThresholdBreachRecoveredEvent)
        {

        }
    }

    public class LogThresholdBreachRecoveryCheckEvent : LogEvents
    {
        public LogThresholdBreachRecoveryCheckEvent()
            : base(LogMonitor.LogEventType.ThresholdBreachRecoveryCheckEvent)
        {

        }
    }

    public class LogDataArrivedEvent : LogEvents
    {
        public LogDataArrivedEvent()
            : base(LogMonitor.LogEventType.LogDataArrivedEvent)
        {

        }
    }


    public class LogDataExtractedEvent : LogEvents
    {
        public LogDataExtractedEvent()
            : base(LogMonitor.LogEventType.LogDataExtractedEvent)
        {

        }
    }
}
