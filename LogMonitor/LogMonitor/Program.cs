using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace LogMonitor
{
    class Program 
    {

        static void Main(string[] args)
        {
            Console.WriteLine("Please Enter the logfile path to monitor");
           String path = Console.ReadLine();

            int reportTimeSpan = 10;
            int alertTimeSpan = 120;
            int recoveryTimeSpan = 10;
            int alertTrafficCount = 1000;
            Console.WriteLine("Please Enter Report Job frequency in Seconds");
            Int32.TryParse(Console.ReadLine(), out reportTimeSpan);

            JobConfig.ReportDuration = new TimeSpan(0, 0, reportTimeSpan);

            Console.WriteLine("Please Enter Alert frequency in Seconds");
            Int32.TryParse(Console.ReadLine(), out alertTimeSpan);

            JobConfig.AlertDuration = new TimeSpan(0, 0, alertTimeSpan);


            Console.WriteLine("Please Enter Recovery Check frequency in Seconds");
            Int32.TryParse(Console.ReadLine(), out recoveryTimeSpan);

            JobConfig.RecoveryDuration = new TimeSpan(0, 0, recoveryTimeSpan);



            Console.WriteLine("Please Enter High traffic threshold");
            Int32.TryParse(Console.ReadLine(), out alertTrafficCount);
            JobConfig.AlertThreshold = alertTrafficCount;



            DataPipelineController dpController = new DataPipelineController(path);
            dpController.Init();
            while (Console.ReadLine() != "Quit")
            {

            }
        }

       
    }
}
