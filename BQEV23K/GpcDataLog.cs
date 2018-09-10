using System;
using System.Globalization;
using System.IO;

namespace BQEV23K
{
    /// <summary>
    /// This class creates a new data log for GPC cycle.
    /// </summary>
    public class GpcDataLog
    {
        private DateTime startTime;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="cellCount">Configured battery cell count.</param>
        public GpcDataLog(int cellCount)
        {
            startTime = DateTime.Now;
            try
            {
                if(!Directory.Exists("GPC Results"))
                {
                    Directory.CreateDirectory("GPC Results");
                }

                if(!File.Exists(@"GPC Results\Config.txt"))
                {
                    using (System.IO.StreamWriter writer = new System.IO.StreamWriter(@"GPC Results\config.txt", false, System.Text.Encoding.UTF8))
                    {
                        writer.WriteLine("ProcessingType=2");
                        writer.WriteLine("NumCellSeries=" + cellCount.ToString());
                        writer.WriteLine("ElapsedTimeColumn=0");
                        writer.WriteLine("VoltageColumn=1");
                        writer.WriteLine("CurrentColumn=2");
                        writer.WriteLine("TemperatureColumn=3");
                    }
                }

                if (File.Exists(@"GPC Results\roomtemp_rel_dis_rel.csv"))
                {
                    File.Delete(@"GPC Results\roomtemp_rel_dis_rel.csv");
                }

                using (System.IO.StreamWriter writer = new System.IO.StreamWriter(@"GPC Results\roomtemp_rel_dis_rel.csv", false, System.Text.Encoding.UTF8))
                {
                    writer.WriteLine("ElapsedTime,Voltage,AvgCurrent,Temperature");
                }
            } catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        /// <summary>
        /// Write new data line to log file.
        /// </summary>
        /// <param name="voltage">Battery voltage</param>
        /// <param name="current">Battery current</param>
        /// <param name="temperature">Battery temperature</param>
        public void WriteLine(int voltage, int current, double temperature)
        {
            try
            {
                using (System.IO.StreamWriter writer = new System.IO.StreamWriter(@"GPC Results\roomtemp_rel_dis_rel.csv", true, System.Text.Encoding.UTF8))
                {
                    writer.WriteLine(DateTime.Now.Subtract(startTime).TotalSeconds.ToString("F1", CultureInfo.CreateSpecificCulture("en-US")) + "," + voltage.ToString() + "," + current.ToString() + "," + temperature.ToString("F1", CultureInfo.CreateSpecificCulture("en-US")));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}
