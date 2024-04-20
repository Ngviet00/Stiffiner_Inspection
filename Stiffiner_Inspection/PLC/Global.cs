using Stiffiner_Inspection.Models.DTO.Data;

namespace Stiffiner_Inspection
{
    public static class Global
    {
        public static ControlPLC controlPLC = new ControlPLC();

        public static int resetClient { get; set; } = 0;
        public static int valuePLC { get; set; } = 4;

        //reset PLC
        public static int resetPLC1 { get; set; } = 0;
        public static int resetPLC2 { get; set; } = 0;
        public static int resetPLC3 { get; set; } = 0;
        public static int resetPLC4 { get; set; } = 0;

        public static int currentTray { get; set; } = 0;

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

        //string models
        public static string strModels = string.Empty;
        public static string _currentSelectedModel = string.Empty;
        public static List<string> ListModels = new List<string>();

        //check client is send model to server
        public static int Client1IsPostModel = 1;
        public static int Client2IsPostModel = 1;
        public static int Client3IsPostModel = 1;
        public static int Client4IsPostModel = 1;

        // client status
        public static int ClientStatus1 = 2; //1 running, 2 pause
        public static int ClientStatus2 = 2; //1 running, 2 pause
        public static int ClientStatus3 = 2; //1 running, 2 pause
        public static int ClientStatus4 = 2; //1 running, 2 pause
    }
}
