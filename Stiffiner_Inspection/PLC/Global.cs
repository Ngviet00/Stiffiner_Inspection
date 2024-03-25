using Stiffiner_Inspection.Models.DTO.Data;
using System.Collections;

namespace Stiffiner_Inspection
{
    public static class Global
    {
        public static ControlPLC controlPLC = new ControlPLC();

        public enum eSampleStatus
        {
            OK = 1,
            NG = 2,
            EMPTY = 3
        }

        public static int resetClient { get; set; } = 0;

        public static int valuePLC { get; set; } = 4;

        public static int resetPLC1 { get; set; } = 0;

        public static int resetPLC2 { get; set; } = 0;

        public static int resetPLC3 { get; set; } = 0;

        public static int resetPLC4 { get; set; } = 0;

        public static int currentTray = 1;
    }
}
