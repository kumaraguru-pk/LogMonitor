using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Collections.Concurrent;
namespace LogMonitor
{

    public class LogData
    {
        // Possible operations on Data        

        DateTime time;
        String serverAddress;
        String originatingIP;
        String httpVerb;
        String httpResource;
        String queryString;
        int port;
        String destinationIP;
        String ClientID;
        int httpErrorCode;
        int responseSize;
        String rawLog;

        public DateTime Time { get => time; set => time = value; }
        public string ServerAddress { get => serverAddress; set => serverAddress = value; }
        public string OriginatingIP { get => originatingIP; set => originatingIP = value; }
        public string HttpVerb { get => httpVerb; set => httpVerb = value; }
        public string HttpResource { get => httpResource; set => httpResource = value; }
        public string QueryString { get => queryString; set => queryString = value; }
        public int Port { get => port; set => port = value; }
        public string DestinationIP { get => destinationIP; set => destinationIP = value; }
        public string ClientID1 { get => ClientID; set => ClientID = value; }
        public int HttpErrorCode { get => httpErrorCode; set => httpErrorCode = value; }
        public int ResponseSize { get => responseSize; set => responseSize = value; }
        public string RawLog { get => rawLog; set => rawLog = value; }
    }

    public interface IDataStore<T, Y>
    {
        T DataStore { get; }

        void InsertData(List<Y> events);

        List<Y> GetViewOnTimeLine(TimeSpan ts);


    }


    //Specialized Store 
    public class DataStoreDictionary : IDataStore<ConcurrentDictionary<DateTime, List<LogData>>, LogData>
    {
        public DataStoreDictionary()
        {
            timedStore = new ConcurrentDictionary<DateTime, List<LogData>>();
        }
        private ConcurrentDictionary<DateTime, List<LogData>> timedStore;
        public ConcurrentDictionary<DateTime, List<LogData>> DataStore
        {
            get
            {
                return timedStore;
            }

        }

        public void InsertData(List<LogData> dataList)
        {
            foreach (LogData ld in dataList)
            {
                List<LogData> ldList;

                if (!timedStore.TryGetValue(ld.Time, out ldList))
                    ldList = new List<LogData>();
                ldList.Add(ld);
                timedStore[ld.Time] = ldList;
            }
        }
        // Go back upto TS from current time and pull the data for it. 
        public List<LogData> GetViewOnTimeLine(TimeSpan ts)
        {
            /// Do a reverse iteration and accumulate all the logs which is within the time window. 
            /// IF this is sorted it will be easy search on a range. 
            List<LogData> timedView = new List<LogData>();
            DateTime upto = DateTime.Now.Subtract(ts);
            foreach (var element in timedStore.OrderByDescending(x => x.Key))
            {
                if (element.Key > upto)
                    timedView.AddRange(element.Value);
                else
                    break;
            }
            return timedView;
        }

        // Go back upto TS from current time and pull the data for it. 

    }
}
