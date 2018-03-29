using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Dynamic;

namespace LogMonitor
{
    public abstract class Job : LogEventPublisher<LogEvents>, LogEventSubscriber<LogEvents>,IDisposable
    {
        protected TimeSpan alertSpan;
        protected ManualResetEventSlim JobAlert = new ManualResetEventSlim();
        private CancellationTokenSource alertWatchDogCancellationtokenSource;
        protected CancellationToken alertWatchDogCancellationtoken;
        protected Thread AlertThread;
        protected dynamic JobParams;
        public Job(TimeSpan ts)
        {
            alertSpan = ts;
            alertWatchDogCancellationtokenSource = new CancellationTokenSource();
            alertWatchDogCancellationtoken = alertWatchDogCancellationtokenSource.Token;
            JobParams = new ExpandoObject();
            JobParams.timewindow = alertSpan;
            JobParams.Intentions = new List<Intentions>();
        }

        public void InitJob()
        {
            AlertThread.Start();
        }
        public void CancelJob()
        {
            alertWatchDogCancellationtokenSource.Cancel();
        }

        protected abstract void JobUpdate(LogEvents input);

        public void Update(LogEvents Input)
        {
            JobUpdate(Input);
        }

        public void Wait()
        {
            if (AlertThread.IsAlive)
                AlertThread.Join();
        }

        public void Dispose()
        {
            Dispose(true);
            
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                // free managed resources  
                if (JobAlert != null)
                {
                    JobAlert.Dispose();
                    JobAlert = null;
                }

                if (alertWatchDogCancellationtokenSource != null)
                {
                    alertWatchDogCancellationtokenSource.Dispose();
                    alertWatchDogCancellationtokenSource = null;
                }
            }
            
        }

    }

    public class ReportJob : Job
    {
        public ReportJob(TimeSpan ts)
            : base(ts)
        {
            AlertThread = new Thread(ReportAlert);
            JobParams.Intentions.Add(new Intentions(DataPoints.DPLOverallTraffic, Operations.CountOf));
            JobParams.Intentions.Add(new Intentions(DataPoints.DPHTTPResource, Operations.CountOf));
            JobParams.Intentions.Add(new Intentions(DataPoints.DPLResponseSize, Operations.AvgOf));
        }

        public override void Notify(params object[] args)
        {
            foreach (LogEventSubscriber<LogEvents> lgSub in ObservorList)
            {
                LogEvents logEvent = new LogTimeGenerateReportEvent();
                logEvent.ExecutionContext = JobParams;
                lgSub.Update(logEvent);
            }
        }

        protected override void JobUpdate(LogEvents input)
        {
            foreach (LogEventSubscriber<LogEvents> observors in ObservorList)
            {
                if (input.Type == LogEventType.LogReportGeneratedEvent)
                {
                    observors.Update(input);
                }

            }
        }

        private void ReportAlert()
        {
            try
            {
                while (alertWatchDogCancellationtoken.IsCancellationRequested != true)
                {
                    JobAlert.Wait(alertSpan, alertWatchDogCancellationtoken);
                    try
                    {
                        Notify(LogEventType.LogReportGenerateEvent);
                    }
                    catch (Exception )
                    {

                    }
                }
            }
            catch (OperationCanceledException )
            {
                //   job cancelled

            }
        }
    }


    // Acts a forwarder for recovery alerts associated with this breach
    public class BreachDetectionJob : Job
    {
        private RecoveryDetectionJob watchRecovery;
        public BreachDetectionJob(TimeSpan ts) : base(ts)
        {
            AlertThread = new Thread(BreachCheckAlert);
            JobParams.Intentions.Add(new WorrySomeIntentions(DataPoints.DPLOverallTraffic, Operations.CountOf, JobConfig.AlertThreshold)); 
        }
        public override void Notify(params object[] args)
        {

            foreach (LogEventSubscriber<LogEvents> observors in ObservorList)
            {
                LogEvents eventObj = null;
                if ((LogEventType)args[0] == LogEventType.ThresholdBreachRecoveryCheckEvent)
                {
                    eventObj = new LogThresholdBreachRecoveryCheckEvent();
                    eventObj.ExecutionContext = JobParams;
                }
                if ((LogEventType)args[0] == LogEventType.ThresholdBreachCheckEvent)
                {
                    eventObj = new LogThresholdBreachCheckEvent();
                    eventObj.ExecutionContext = JobParams;
                }
                if ((LogEventType)args[0] == LogEventType.ThresholdBreachDetectedEvent)
                {
                    eventObj = new LogThresholdBreachDetectedEvent();
                    eventObj.ExecutionContext = args[1];
                }
                if ((LogEventType)args[0] == LogEventType.ThresholdBreachRecoveredEvent)
                {
                    eventObj = new LogThresholdBreachRecoveredEvent();
                    eventObj.ExecutionContext = args[1];
                }

                observors.Update(eventObj);
            }
        }

        private void BreachCheckAlert()
        {
            try
            {
                while (alertWatchDogCancellationtoken.IsCancellationRequested != true)
                {
                    JobAlert.Wait(alertSpan, alertWatchDogCancellationtoken);
                    try
                    {
                        if (watchRecovery != null)
                            watchRecovery.Wait();

                        Notify(LogEventType.ThresholdBreachCheckEvent);
                    }
                    catch (Exception )
                    {

                    }
                }
            }
            catch (OperationCanceledException )
            {
                //   watchDogThread.

            }
        }

        protected override void JobUpdate(LogEvents Input)
        {
            switch (Input.Type)
            {
                case LogEventType.ThresholdBreachDetectedEvent:
                    //Breach Detected notify and time to  start the Recovery check   
                    // Make sure only recovery job is running for this breach
                    if (watchRecovery == null)
                    {
                        watchRecovery = new RecoveryDetectionJob(JobConfig.RecoveryDuration);
                        watchRecovery.Subscribe(this);
                        watchRecovery.InitJob();
                        Notify(Input.Type, Input.ExecutionContext);
                    }
                    break;
                case LogEventType.ThresholdBreachRecoveryCheckEvent:
                    Notify(Input.Type);
                    break;
                case LogEventType.ThresholdBreachRecoveredEvent:
                    watchRecovery.CancelJob();
                    watchRecovery = null;
                    Notify(Input.Type, Input.ExecutionContext);
                    break;
                default:
                    break;
            }
        }
    }

    public class RecoveryDetectionJob : Job, LogEventSubscriber<LogEvents>
    {
        public RecoveryDetectionJob(TimeSpan ts)
           : base(ts)
        {
            AlertThread = new Thread(RecoveryWatchAlert);
            JobParams.Intentions.Add(new WorrySomeIntentions(DataPoints.DPLOverallTraffic, Operations.CountOf, JobConfig.AlertThreshold));
        }

        private void RecoveryWatchAlert()
        {
            try
            {
                while (alertWatchDogCancellationtoken.IsCancellationRequested != true)
                {
                    JobAlert.Wait(alertSpan, alertWatchDogCancellationtoken);
                    try
                    {
                        Notify(LogEventType.ThresholdBreachRecoveryCheckEvent);
                    }
                    catch (Exception )
                    {

                    }
                }
            }
            catch (OperationCanceledException )
            {
                //   watchDogThread.

            }
        }

        public override void Notify(params object[] args)
        {

            foreach (LogEventSubscriber<LogEvents> observors in ObservorList)
            {
                LogEvents eventObj = new LogThresholdBreachRecoveryCheckEvent();
                eventObj.ExecutionContext = JobParams;
                observors.Update(eventObj);
            }
        }

        protected override void JobUpdate(LogEvents Input)
        {
            throw new NotImplementedException();
        }

    }
}
