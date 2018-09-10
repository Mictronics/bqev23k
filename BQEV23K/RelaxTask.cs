using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BQEV23K
{
    /// <summary>
    /// This class handles a single relaxing task.
    /// </summary>
    public class RelaxTask : GenericTask
    {
        private bool isInitialized = false;
        private int duration = 0;
        private DateTime startTime;
        private DateTime endTime;

        #region Properties
        /// <summary>
        /// Get name of task.
        /// </summary>
        override public string Name
        {
            get
            {
                return "Relax";
            }
        }

        /// <summary>
        /// Get description of task.
        /// </summary>
        override public string Description
        {
            get
            {
                return Name + " - (Duration = up to " + TimeSpan.FromMinutes(duration).ToString(@"hh\:mm\:ss") + ")";
            }
        }

        /// <summary>
        /// Get expected end time of task.
        /// </summary>
        public DateTime EndTime
        {
            get
            {
                return endTime;
            }
        }

        /// <summary>
        /// Get start time of task.
        /// </summary>
        override public DateTime StartTime
        {
            get
            {
                return startTime;
            }
        }
        #endregion

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="d">Duration of task.</param>
        public RelaxTask(int d)
        {
            duration = d;
        }

        /// <summary>
        /// Initialize charge task.
        /// </summary>
        /// <returns>Initialization status.</returns>
        override public bool InitializeTask()
        {
            startTime = endTime = DateTime.Now;
            endTime = endTime.AddMinutes(duration);
            isInitialized = true;
            return true;
        }

        /// <summary>
        /// Check and return task completation status.
        /// </summary>
        /// <param name="gaugeInfo">Reference to device/gauge in use.</param>
        /// <returns>True when task completed, otherwise false.</returns>
        override public bool IsTaskComplete(GaugeInfo gaugeInfo)
        {
            if (isInitialized)
            {
                // Complete either by timeout
                if (DateTime.Compare(DateTime.Now, endTime) > 0)
                    return true;
                // or more accurate when VOK and RDIS are cleared (SLUA848, chapter 4.2.2)
                if (gaugeInfo.FlagVOK == false && gaugeInfo.FlagRDIS == false)
                    return true;
            }
            return false;
        }
    }
}
