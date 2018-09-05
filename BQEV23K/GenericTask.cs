using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BQEV23K
{
    /// <summary>
    /// This class defines a generic cycle task.
    /// </summary>
    public class GenericTask
    {
        private DateTime startTime;
        private string name = "Task";
        private string description = "Generic Task";

        #region Properties
        /// <summary>
        /// Get start time of task.
        /// </summary>
        virtual public DateTime StartTime
        {
            get
            {
                return startTime;
            }
        }

        /// <summary>
        /// Get name of task.
        /// </summary>
        virtual public string Name
        {
            get
            {
                return name;
            }
        }

        /// <summary>
        /// Get description of task.
        /// </summary>
        virtual public string Description
        {
            get
            {
                return description;
            }
        }
        #endregion

        /// <summary>
        /// Initialize charge task.
        /// </summary>
        /// <returns>Initialization status.</returns>
        virtual public bool InitializeTask()
        {
            this.startTime = DateTime.Now;
            return true;
        }

        /// <summary>
        /// Check and return task completation status.
        /// </summary>
        /// <param name="gaugeInfo">Reference to device/gauge in use.</param>
        /// <returns>True when task completed, otherwise false.</returns>
        virtual public bool IsTaskComplete(GaugeInfo gaugeInfo)
        {
            return true;
        }
    }
}
