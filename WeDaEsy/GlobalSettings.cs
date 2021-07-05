using System.Collections.Generic;
using System.Linq;

namespace WeDaESy
{
    internal static class GlobalSettings
    {
        #region Fields

        private static WorkloadCompValArray s_globalWorkloadCompValArray;

        #endregion Fields

        #region Properties

        public static int CurrentTaskId { get; internal set; } = 1;
        public static bool GlobalIsExperimentalGroup { get; internal set; }
        public static bool GlobalIsFindNextTask { get; set; }

        public static WorkloadCompValArray GlobalWorkloadCompValArray
        {
            get => s_globalWorkloadCompValArray;
            set
            {
                s_globalWorkloadCompValArray = value;
                GlobalWorkloadCompValList = value.WorkloadCompVals.ToList();
            }
        }

        public static List<WorkloadCompVal> GlobalWorkloadCompValList { get; set; }

        // 1 is expected to be the entry task //TODO UI-Erfassung oder config File Erfassung implementieren
        public static List<int> ListUsedTaskIds { get; set; } = new List<int>();

        #endregion Properties
    }
}