using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace LogMonitor
{    
    abstract class Parser : LogEventPublisher<LogEvents>,LogEventSubscriber<LogEvents>
    {
        public void Update(LogEvents Input)
        {       
            Parse(Input);
        }
         protected abstract void Parse(LogEvents input);

        public override void Notify(params object[] args)
        {
            foreach(LogEventSubscriber<LogEvents> sub in ObservorList)
            {
                LogEvents eventDataExtracted = new LogDataExtractedEvent();
                eventDataExtracted.ExecutionContext = args[0];
                sub.Update(eventDataExtracted);
            }
        }

    }
    class IISLogParser : Parser
    {
        protected override void Parse(LogEvents input)
        {
            List<String> logLines = input.ExecutionContext;
            List<LogData> logExtractedData = new List<LogData>();
            foreach (String str in logLines)
            {
                String[] val = str.Split(' ');
                //2018-03-27 03:14:45 naws131 10.100.86.134 GET /WSHandlerV2.ashx NAWS_USER_ID=6920513 443 10.1.20.3 - 200 15
                LogData data = new LogData();
                DateTime dt;
                String dateTime = val[0] +" " + val[1];
                 DateTime.TryParse(dateTime, out dt);
                data.Time = dt;
                data.ServerAddress = val[2];
                data.OriginatingIP = val[3];
                data.HttpVerb = val[4];
                data.HttpResource = val[5];
                data.QueryString = val[6];
                int port = 0;
                Int32.TryParse(val[7],out port);
                data.Port = port;
                data.DestinationIP = val[8];
                data.ClientID1 = val[9];
                int outval = 200;
                Int32.TryParse(val[10], out outval);
                data.HttpErrorCode = outval;
                Int32.TryParse(val[11], out outval);
                data.ResponseSize = outval;
                data.RawLog = str;
                logExtractedData.Add(data);
            } 
            if(logExtractedData.Count>0)
                Notify(logExtractedData);
        }
    }
}
