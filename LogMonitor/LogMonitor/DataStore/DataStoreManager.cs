using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Collections.Concurrent;
namespace LogMonitor
{
    
    public abstract class IDataStoreManager<T> : LogEventPublisher<LogEvents>, LogEventSubscriber<LogEvents> 
    {
        protected T dataStore;
        public override void Notify(params object[] args)
        {
            LogEvents eventData = null;
            if ((LogEventType)args[0] == LogEventType.LogReportGeneratedEvent)
            {
                eventData = new LogTimeReportGeneratedEvent();
                eventData.ExecutionContext = args[1];

            }
            if ((LogEventType)args[0] == LogEventType.ThresholdBreachDetectedEvent)
            {
                eventData = new LogThresholdBreachDetectedEvent();
                eventData.ExecutionContext = args[1];
            }
            if ((LogEventType)args[0] == LogEventType.ThresholdBreachRecoveredEvent)
            {
                eventData = new LogThresholdBreachRecoveredEvent();
                eventData.ExecutionContext = args[1];
            }

            foreach (LogEventSubscriber<LogEvents> observors in ObservorList)
            {
                observors.Update(eventData);
            }

        }

        public void Update(LogEvents Input)
        {
            Updates(Input);

        }

        protected abstract void CheckForBreachBothWays(LogEvents Input, bool positive);
        protected abstract void CheckForBreachRecovery(LogEvents Input);
        protected abstract void PrepareReport(LogEvents Input);

        protected abstract void Updates(LogEvents Input);
    }
    
    //Specialized Manager
    public class LogDataStoreManager<T>: IDataStoreManager<T> where T : IDataStore<ConcurrentDictionary<DateTime, List<LogData>>, LogData>


    { 
        public LogDataStoreManager( )
        {
            dataStore = (T)Activator.CreateInstance(typeof(DataStoreDictionary)); 
        }

        protected override void CheckForBreachBothWays(LogEvents Input,bool positive)
        {
            dynamic intentionsOutCome = PrepareIntentionOutCome(Input);
            int thresholdToCheck=Int32.MaxValue;
            foreach(WorrySomeIntentions wsi in Input.ExecutionContext.Intentions)
            {
                if(wsi.InterestedField== DataPoints.DPLOverallTraffic)
                {
                    if(wsi.WhatToDo== Operations.AvgOf)
                    {
                        intentionsOutCome.AverageTPS = intentionsOutCome.totalCount / Input.ExecutionContext.timeWindoW;
                    }else if(wsi.WhatToDo == Operations.CountOf)
                    {
                        thresholdToCheck = wsi.Threshold;
                    }
                        
                }               
            }
            bool bNotify = false;
            LogEventType type;
            if(positive)
            {
                bNotify = intentionsOutCome.totalCount < thresholdToCheck ? true : false;
                type = LogEventType.ThresholdBreachRecoveredEvent;
            }
            else
            {
                bNotify = intentionsOutCome.totalCount > thresholdToCheck ? true : false;
                type = LogEventType.ThresholdBreachDetectedEvent;
            }
                
            
            if (bNotify)
                Notify(type, intentionsOutCome);
        }

        protected override void CheckForBreachRecovery(LogEvents Input)
        {
            CheckForBreachBothWays(Input, true);
        }

        private dynamic PrepareIntentionOutCome(LogEvents Input)
        {
            List<LogData> view = dataStore.GetViewOnTimeLine(Input.ExecutionContext.timewindow);
            dynamic intentionsOutCome = new System.Dynamic.ExpandoObject();
            intentionsOutCome.ErrorCount = 0;
            intentionsOutCome.AverageResponseSize = 0;
            intentionsOutCome.totalCount = view.Count;
            intentionsOutCome.resourceHit = new Dictionary<string, int>();
            intentionsOutCome.TimeWindow = Input.ExecutionContext.timewindow.ToString();
            if (intentionsOutCome.totalCount > 0)
            {
                foreach (LogData dt in view)
                {
                    intentionsOutCome.AverageResponseSize += dt.ResponseSize;
                    if (dt.HttpErrorCode >= 400)
                        intentionsOutCome.ErrorCount++;
                    int count = 0;
                    intentionsOutCome.resourceHit.TryGetValue(dt.HttpResource, out count);
                    intentionsOutCome.resourceHit[dt.HttpResource] = count + 1;
                }
                intentionsOutCome.AverageResponseSize /= view.Count;
            }
            return intentionsOutCome;
        }
        protected override void PrepareReport(LogEvents Input)
        {
            dynamic intentionsOutCome = PrepareIntentionOutCome(Input);
            if (intentionsOutCome.totalCount > 0)
                Notify(LogEventType.LogReportGeneratedEvent, intentionsOutCome);

        }

        protected override void Updates(LogEvents Input)
        {
            if (Input.Type == LogEventType.LogDataExtractedEvent)
            {
                dataStore.InsertData(Input.ExecutionContext);
            }
            if (Input.Type == LogEventType.ThresholdBreachCheckEvent)
            {
                CheckForBreachBothWays(Input, false);
            }
            if (Input.Type == LogEventType.ThresholdBreachRecoveryCheckEvent)
            {
                CheckForBreachRecovery(Input);
            }
            if (Input.Type == LogEventType.LogReportGenerateEvent)
            {
                PrepareReport(Input);
            }

        }

    }

}
