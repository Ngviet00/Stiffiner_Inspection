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

        public static List<DataDTO> CurrentTrayData = new List<DataDTO>();

        //status CAM
        public static int StatusCam1 { get; set; } = 0;
        public static int StatusCam2 { get; set; } = 0;
        public static int StatusCam3 { get; set; } = 0;
        public static int StatusCam4 { get; set; } = 0;

        //connect
        public static int ConnectCam1 { get; set; } = 0;
        public static int ConnectCam2 { get; set; } = 0;
        public static int ConnectCam3 { get; set; } = 0;
        public static int ConnectCam4 { get; set; } = 0;

        //deep-learning
        public static int DeepLearningCam1 { get; set; } = 0;
        public static int DeepLearningCam2 { get; set; } = 0;
        public static int DeepLearningCam3 { get; set; } = 0;
        public static int DeepLearningCam4 { get; set; } = 0;

        public static int currentSelectedModel = 1;

        public static int StatusVisionChangeModel = 0; //1 busy
    }
}
