using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogMonitor
{
    public static class JobConfig
    {
        
        static TimeSpan reportDuration = new TimeSpan(0, 0, 10);
        static TimeSpan alertDuration =  new TimeSpan(0, 0, 15);
        static TimeSpan recoveryDuration = new TimeSpan(0, 0, 15);

        static int alertThreshold = 1000;
        static int recoveryThreshold = 1000;

        public static TimeSpan ReportDuration { get => reportDuration; set => reportDuration = value; }
        public static TimeSpan AlertDuration { get => alertDuration; set => alertDuration = value; }
        public static TimeSpan RecoveryDuration { get => recoveryDuration; set => recoveryDuration = value; }
        public static int AlertThreshold { get => alertThreshold; set => alertThreshold = value; }
        public static int RecoveryThreshold { get => recoveryThreshold; set => recoveryThreshold = value; }
    }
}
