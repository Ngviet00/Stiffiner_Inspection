using Stiffiner_Inspection.Models.DTO.Data;

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

        public static int currentTray { get; set; } = 0;

        public static int currentTargetId { get; set; } = 1;

        public static List<DataDTO> currentTrayLeft = new List<DataDTO>();
        public static List<DataDTO> currentTrayRight = new List<DataDTO>();

        public static string directoryPath = @"D:\Export_Result";
        public static string fileNameCSV = "test.csv";      
    }
}
