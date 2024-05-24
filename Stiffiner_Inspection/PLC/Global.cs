using Stiffiner_Inspection.Models.DTO.Data;
using System.Collections.Concurrent;

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

        public static ConcurrentQueue<DataDTO> CurrentTrayDataV2 = new ConcurrentQueue<DataDTO>();

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
        public static int Client1IsPostModel = 0;
        public static int Client2IsPostModel = 0;
        public static int Client3IsPostModel = 0;
        public static int Client4IsPostModel = 0;

        // client clear data, 1 - clear
        public static int ClearClient1 = 0;
        public static int ClearClient2 = 0;
        public static int ClearClient3 = 0;
        public static int ClearClient4 = 0;

        public static string PathFileCurrentModel = @"D:\Projects\Stiffiner_Inspection\Stiffiner_Inspection\ClientModel\CurrentModel.txt";
        public static string PathFileListModel = @"D:\Projects\Stiffiner_Inspection\Stiffiner_Inspection\ClientModel\ListModels.txt";
        public static string PathFileMode = @"D:\Projects\Stiffiner_Inspection\Stiffiner_Inspection\ClientModel\Mode.txt";
        public static string PathFileLogProgram = @"D:\LogProgram\LogProgram.txt";
        public static string PathFileTimeLine = @"D:\Projects\Stiffiner_Inspection\Stiffiner_Inspection\ClientModel\TimeLine.txt";

        public static int Mode = 1; //1 master, 2 normal

        public static string? TimeLine = DateTime.Now.ToString("yyyyMMddHHmmss");

        public static int ResetCamClient1 = 0;
        public static int ResetCamClient2 = 0;
        public static int ResetCamClient3 = 0;
        public static int ResetCamClient4 = 0;
    }
}
