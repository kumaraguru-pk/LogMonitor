using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogMonitor
{
    public enum Operations
    {
        AvgOf,
        CountOf
    }
    public enum DataPoints
    {
        DPLOverallTraffic,
        DPLResponseSize,
        DPHTTPErrors,
        DPHTTPResource
    }

    public class Intentions
    {
        public Intentions(DataPoints interest, Operations todo)
        {
            InterestedField = interest;
            WhatToDo = todo;
        }
        protected DataPoints interestedField;
        protected Operations whatToDo;
        private dynamic result;
        public DataPoints InterestedField { get => interestedField; set => interestedField = value; }
        public Operations WhatToDo { get => whatToDo; set => whatToDo = value; }
        public dynamic Result { get => result; set => result = value; }
    }

    public class WorrySomeIntentions : Intentions
    {
        public WorrySomeIntentions(DataPoints interest, Operations todo, int limit) : base(interest, todo)
        {
            threshold = limit;
        }
        int threshold;
        public int Threshold { get => threshold; set => threshold = value; }
    }
}
