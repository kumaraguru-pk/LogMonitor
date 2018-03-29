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
    
    public sealed class JobManager: IEnumerable,IDisposable
    {
        List<Job> Jobs;
        /*
        * These alert jobs should be scalable
        * Basically we should be able to have any number of jobs with customized  time span for each of them. 
        * May be we need to have a AlertJobsManager which creates this. 
        */
        Job reportJob;
        Job breachJob;        
        CancellationTokenSource recoverywatchDogTokensource = new CancellationTokenSource();

        public List<Job> AlertJobs { get => Jobs; }

        public JobManager()
        {
            Jobs = new List<Job>();
            reportJob = new ReportJob(JobConfig.ReportDuration);
            breachJob = new BreachDetectionJob(JobConfig.AlertDuration);
            Jobs.Add(reportJob);
            Jobs.Add(breachJob);
        }

        public void InitAlert()
        {
            foreach (Job job in Jobs)
                job.InitJob();
        }


        IEnumerator IEnumerable.GetEnumerator()
        {
            return new AlertJobEnumerator(this);
        }

     
        private bool disposedValue = false; // To detect redundant calls

        void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    if(recoverywatchDogTokensource!=null)
                    {
                        recoverywatchDogTokensource.Dispose();
                        recoverywatchDogTokensource = null;
                    }
                    if (reportJob != null)
                    {
                        reportJob.Dispose();
                        reportJob = null;
                    }
                    if (breachJob != null)
                    {
                        breachJob.Dispose();
                        breachJob = null;
                    }
                     
                } 
                disposedValue = true;
            }
        }
 
        public void Dispose()
        {
            Dispose(true);           
        } 


    }

    public class AlertJobEnumerator : IEnumerator
    {
        private int position = -1;
        private JobManager alertJobManager;

        public AlertJobEnumerator(JobManager manager)
        {
            this.alertJobManager = manager;
        }

        public AlertJobEnumerator()
        {
        }

        // The IEnumerator interface requires a MoveNext method.
        public bool MoveNext()
        {
            if (position < alertJobManager.AlertJobs.Count - 1)
            {
                position++;
                return true;
            }
            else
            {
                return false;
            }
        }

        // The IEnumerator interface requires a Reset method.
        public void Reset()
        {
            position = -1;
        }

        // The IEnumerator interface requires a Current method.
        public object Current
        {
            get
            {
                return alertJobManager.AlertJobs[position];
            }
        }
    }
    }
