using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks; 
using System.IO;

namespace LogMonitor
{

    abstract class DataStream : LogEventPublisher<LogEvents>
    {
        public DataStream(String path)
        {
            streamPath = path;
        }
        public abstract List<String> Read();

        protected String streamPath;
    }

    class FileDataStreamer : DataStream
    {
        long lastLogReadPosition;
        public FileDataStreamer(String path) : base(path)
        {
            var monitor = new FileSystemWatcher();
            monitor.Path = Path.GetDirectoryName(streamPath);
            //monitor.Filter = Path.GetFileName(streamPath);
            monitor.NotifyFilter = NotifyFilters.FileName | NotifyFilters.DirectoryName | NotifyFilters.Attributes | NotifyFilters.Size | NotifyFilters.LastWrite | NotifyFilters.LastAccess | NotifyFilters.CreationTime | NotifyFilters.Security;
            monitor.Changed += new FileSystemEventHandler(OnDataArrived);
            monitor.EnableRaisingEvents = true;
            lastLogReadPosition = 0;

        }

        public override void Notify(params object[] args)
        {
            foreach (LogEventSubscriber<LogEvents> subscriber in ObservorList)
            {
                LogEvents eventObj = new LogDataArrivedEvent();
                eventObj.ExecutionContext = args[0];
                subscriber.Update(eventObj);
            }
        }

        public override List<String> Read()
        {
            List<String> logStatements = new List<String>();

            using (Stream logStream = File.Open(streamPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                logStream.Seek(lastLogReadPosition, SeekOrigin.Begin);
                StreamReader logReader = new StreamReader(logStream);
                String logStatement = "";
                while ((logStatement = logReader.ReadLine()) != null)
                {
                    logStatements.Add(logStatement);
                }
                lastLogReadPosition = logStream.Position;
            }
            return logStatements;
        }



        private void OnDataArrived(object source, FileSystemEventArgs e)
        {
            if (e.FullPath == streamPath)
            {
                List<String> lstLines = Read();
                if (lstLines.Count >= 1)
                    Notify(lstLines);
            }
        }
    }
}
