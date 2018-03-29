using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Collections.Concurrent;

namespace LogMonitor
{
   public class DataPipelineController :IDisposable
   {
        /*
         * Major modules are 
         * Input 
         * Parse
         * Store
         * Display
         */

        DataStream fdStreamer;
        Parser streamParser;
        LogDataStoreManager<IDataStore<ConcurrentDictionary<DateTime, List<LogData>>, LogData>> logDataStoreManager;
        JobManager alertJobManager;
        Presenter display;
        public DataPipelineController(String path)
        {
            /*
             * Establish the pipeline and provide necessary linkages. 
             */
            fdStreamer = new FileDataStreamer(path);
            streamParser = new IISLogParser();
            logDataStoreManager = new LogDataStoreManager<IDataStore<ConcurrentDictionary<DateTime, List<LogData>>, LogData>>();
            alertJobManager = new JobManager();
            display = new Presenter();
           
        }

        public void Init()
        {
            EstablishPipeline();
        }
       
        protected virtual void EstablishPipeline()
        {
            fdStreamer.Subscribe(streamParser);
            streamParser.Subscribe(logDataStoreManager);
            foreach(Job alertJob in alertJobManager)
            {
                /*
                 * Mutual Pub and Sub
                 */
                alertJob.Subscribe(logDataStoreManager);
                logDataStoreManager.Subscribe(alertJob);
                alertJob.Subscribe(display);
            }
            // Start the alerts.
            alertJobManager.InitAlert();
        }

         
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    if (alertJobManager != null)
                    {
                        alertJobManager.Dispose();
                        alertJobManager = null;
                    }
                }

            }

            // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
            // TODO: set large fields to null.

            disposedValue = true;
        }

        public void Dispose()
        {            
            Dispose(true);            
        } 
    }
}
