using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogMonitor
{
   
    public abstract class IPresenter: LogEventSubscriber<LogEvents>
    {
        public void Update(LogEvents Input)
        {
            switch (Input.Type)
            {
                case LogEventType.LogReportGeneratedEvent:
                    PrepareReport(Input.ExecutionContext);
                    break;
                case LogEventType.ThresholdBreachDetectedEvent:
                    PrepareBreachReport(Input.ExecutionContext);
                    break;
                case LogEventType.ThresholdBreachRecoveredEvent:
                    PrepareBreachRecoveryReport(Input.ExecutionContext);
                    break;
                default:
                    break;
            }

        }
        protected abstract void PrepareBreachReport(dynamic result);
        protected abstract void   PrepareBreachRecoveryReport(dynamic result);

        protected abstract void PrepareReport(dynamic result);
    }
    public class Presenter :  IPresenter
    {
        String lastGoodReport = "";
        String currAlert = "";       

        private void WriteAlert()
        {
            if (!String.IsNullOrEmpty(currAlert))
            {
                Console.WriteLine(currAlert);
                Console.WriteLine();
                Console.WriteLine();
            }
        }

       

        public String GetTopHits(Dictionary<String, int> dict,int Top)
        {
            String str = "";

            foreach (var item in dict.OrderByDescending(key => key.Value))
            {
                str += "Resource Name = " + item.Key + " - hits =" + item.Value + "\r\n";
                Top--;
                if (Top <= 0) break;

            }
            return str;
        }

        private void AddLinesIfNecessary(String str)
        {
            if (String.IsNullOrEmpty(currAlert))
                currAlert = str;
            else
                currAlert = str += " \r\n\r\n\r\n" + currAlert;
        }
        protected override void PrepareBreachReport(dynamic result)
        {
            try
            {
                Console.Clear();
                currAlert = "";
                TimeSpan window = TimeSpan.Parse(result.TimeWindow.ToString());
                String str = "High traffic generated an alert - hits =" + result.totalCount + " triggered at " + DateTime.Now.ToString() + "\r\n";
                str += "Top 3 Hits \r\n";
                Dictionary<String, int> dict = result.resourceHit;
                str += GetTopHits(dict, 3);
                AddLinesIfNecessary(str);
                Console.Clear();
                WriteAlert();
                Console.WriteLine(lastGoodReport);
            }
            catch (Exception)
            {

            }


        }

        protected override void PrepareBreachRecoveryReport(dynamic result)
        {
            try
            {
                Console.Clear();
                currAlert = "";
                TimeSpan window = TimeSpan.Parse(result.TimeWindow.ToString());
                String str = "High traffic Conditon recovered - hits =" + result.totalCount + " recovered at " + DateTime.Now.ToString() + "\r\n";
                str += "Top 3 Hits \r\n";
                Dictionary<String, int> dict = result.resourceHit;
                str += GetTopHits(dict, 3);
                Console.Clear();
                AddLinesIfNecessary(str);
                WriteAlert();
                Console.WriteLine(lastGoodReport);
            }
            catch(Exception)
            {

            }
            

        }
        protected override void PrepareReport(dynamic result)
        {
            try
            {
                TimeSpan window = TimeSpan.Parse(result.TimeWindow.ToString()); 
                String str = "Total Traffic for the last " + window.Seconds.ToString() + "Seconds " + result.totalCount + "\r\n";
                str += "Total Errors for the last " + window.Seconds.ToString() + "Seconds " + result.ErrorCount + "\r\n";
                str += "Average Response size for the last " + window.Seconds.ToString() + "Seconds " + result.AverageResponseSize + "\r\n";
                str += "Top 3 Hits for the last  " + window.Seconds.ToString() + "Seconds\r\n";
                Dictionary<String, int> dict = result.resourceHit;
                str += GetTopHits(dict, 3);
                Console.Clear();
                WriteAlert();
                Console.WriteLine(str);
                lastGoodReport = str;
            }
            catch(Exception)
            {

            }

        }
        
    }
}
