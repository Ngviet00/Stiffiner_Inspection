using Microsoft.AspNetCore.SignalR;
using Stiffiner_Inspection.Hubs;

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

        public static int tempValuePLC { get; set; } = -1;

        public static int plcReset { get; set; } = -1;
    }
}
