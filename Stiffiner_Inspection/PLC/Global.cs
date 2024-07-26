using OfficeOpenXml;
using Stiffiner_Inspection.Commons;
using Stiffiner_Inspection.Models.DTO.Data;
using System.Collections.Concurrent;
using Stiffiner_Inspection.Models.Response;

namespace Stiffiner_Inspection
{
    public static class Global
    {
        public static ControlPLC controlPLC = new ControlPLC();

        public static readonly Random random = new();

        public static int resetClient { get; set; } = 0;
        public static int valuePLC { get; set; } = 4;

        //reset PLC
        public static int resetPLC1 { get; set; } = 0;
        public static int resetPLC2 { get; set; } = 0;
        public static int resetPLC3 { get; set; } = 0;
        public static int resetPLC4 { get; set; } = 0;

        public static int Total { get; set; } = 0;
        public static int TotalOK{ get; set; } = 0;
        public static int TotalNG { get; set; } = 0;
        public static int TotalEmpty { get; set; } = 0;
        public static int currentTray { get; set; } = 0;
        public static int HiddenSetting = 0;    //1 hidden, 0 not hidden

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

        public static string PathFileListModel = @"D:\Projects\Stiffiner_Inspection\Stiffiner_Inspection\ClientModel\ListModels.txt";

        public static string PathFileSetting = @"D:\Projects\Stiffiner_Inspection\Stiffiner_Inspection\ClientModel\Setting.txt";

        public static int Mode = 1; //1 master, 2 normal

        public static string? TimeLine = DateTime.Now.ToString("yyyyMMddHHmmss");

        public static int ResetCamClient1 = 0;
        public static int ResetCamClient2 = 0;
        public static int ResetCamClient3 = 0;
        public static int ResetCamClient4 = 0;

        public static int AUTO_DELETE_IMAGE = 2; //day

        public static int AUTO_DELETE_EXCEL = 120; //day

        public static string PATH_SAVE_EXCEL = @"D:\Export_Result";

        public static string PATH_SAVE_IMAGE = @"D:\publish_image\images\";

        public static string PATH_EXPORT_EXCEL = @"D:\Export_Excel";

        public static void WriteFileToTxt(string filePath, Dictionary<string, string> values)
        {
            try
            {
                var lines = File.ReadAllLines(filePath).ToList();
                var keysToUpdate = values.Keys.ToList();

                // Track which keys have been updated
                var updatedKeys = new HashSet<string>();

                // Iterate through lines to find and update the specific keys
                for (int i = 0; i < lines.Count; i++)
                {
                    var parts = lines[i].Split(new[] { ':' }, 2);
                    if (parts.Length == 2)
                    {
                        string key = parts[0].Trim();
                        if (values.ContainsKey(key))
                        {
                            lines[i] = $"{key}: {values[key]}";
                            updatedKeys.Add(key);
                        }
                    }
                }

                // If some keys were not found, add them as new lines
                foreach (var key in keysToUpdate)
                {
                    if (!updatedKeys.Contains(key))
                    {
                        lines.Add($"{key}: {values[key]}");
                    }
                }

                // Write all lines back to the file
                File.WriteAllLines(filePath, lines);
            }
            catch (Exception ex)
            {
                Log.Error($"Error can not write value to file txt: {ex.Message}");
            }
        }

        public static Dictionary<string, string> ReadValueFileTxt(string filePath, List<string> keys)
        {
            Dictionary<string, string> values = new Dictionary<string, string>();

            try
            {
                string[] lines = File.ReadAllLines(filePath);
                foreach (string line in lines)
                {
                    string[] parts = line.Split(':');

                    if (parts.Length == 2)
                    {
                        string key = parts[0].Trim();

                        if (keys.Contains(key))
                        {
                            values[key] = parts[1].Trim();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error($"Error can not read value from file txt: {ex.Message}");
            }

            return values;
        }

        public static int GetRandom()
        {
            double probability = random.NextDouble();

            if (probability < 0.5)
                return 1;

            return random.Next(2, 6);
        }

        public static string GetNameErrorType(int? errorCode)
        {
            string value = string.Empty;

            switch (errorCode)
            {
                case 1:
                    value = "Particle";
                    break;
                case 2:
                    value = "NG Tape Position";
                    break;
                case 3:
                    value = "Deform";
                    break;
                case 4:
                    value = "Scratch";
                    break;
                case 5:
                    value = "Dirty";
                    break;   
            }

            return value;
        }

        private static string GetUniqueFilePath(string filePath)
        {
            try
            {
                string directory = Path.GetDirectoryName(filePath);
                string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(filePath);
                string extension = Path.GetExtension(filePath);
                int count = 1;

                while (File.Exists(filePath))
                {
                    string newFileName = string.Empty;
                    newFileName = $"{fileNameWithoutExtension}_({count}){extension}";
                    filePath = Path.Combine(directory, newFileName);
                    count++;
                }

                return filePath;
            }
            catch (Exception ex)
            {
                Log.Error($"Error can not get unique file path, error: {ex.Message}");
                return filePath;
            }
        }

        public static void ExportExcel(ExportDataResponse rs, string fromDate, string toDate)
        {
            try
            {
                if (!Directory.Exists(Global.PATH_EXPORT_EXCEL))
                {
                    Directory.CreateDirectory(Global.PATH_EXPORT_EXCEL);
                }

                string fileName = string.Empty;

                if (fromDate == toDate)
                {
                    fileName = $"{fromDate}.xlsx";
                }
                else
                {
                    fileName = $"{fromDate}_{toDate}.xlsx";
                }

                string filePath = Path.Combine(Global.PATH_EXPORT_EXCEL, fileName);
                filePath = GetUniqueFilePath(filePath);

                using (ExcelPackage package = new ExcelPackage(new FileInfo(filePath)))
                {
                    ExcelWorksheet worksheet = package.Workbook.Worksheets.Count == 0 ? package.Workbook.Worksheets.Add("Sheet1") : package.Workbook.Worksheets[0];

                    int row = worksheet.Dimension?.Rows + 1 ?? 1;

                    if (row == 1)
                    {
                        worksheet.Cells[1, 1].Value = "Model";
                        worksheet.Cells[1, 2].Value = "Date";

                        worksheet.Cells[1, 3].Value = "Inspection";
                        worksheet.Cells[1, 4].Value = "OK";

                        worksheet.Cells[1, 5].Value = "NG";
                        worksheet.Cells[1, 6].Value = "NG(%)";
                        worksheet.Cells[1, 7].Value = "Particle";
                        worksheet.Cells[1, 8].Value = "NG Tape Position";

                        worksheet.Cells[1, 9].Value = "Deform";
                        worksheet.Cells[1, 10].Value = "Scratch";
                        worksheet.Cells[1, 11].Value = "Dirty";

                        worksheet.Column(1).Width = 30;
                        worksheet.Column(2).Width = 20;
                        worksheet.Column(3).Width = 15;
                    }

                    if (row == 1)
                    {
                        row += 1;
                    }

                    worksheet.Cells[row, 1].Value = rs.Model;
                    worksheet.Cells[row, 2].Value = rs.DateSelect;
                    worksheet.Cells[row, 3].Value = rs.Ok + rs.Ng;
                    worksheet.Cells[row, 4].Value = rs.Ok;

                    worksheet.Cells[row, 5].Value = rs.Ng;
                    worksheet.Cells[row, 6].Value = Math.Round((double)rs.Ng / (double)(rs.Ok + rs.Ng) * 100, 2);
                    worksheet.Cells[row, 7].Value = rs.ErrorParticle;
                    worksheet.Cells[row, 8].Value = rs.ErrorNgTapePosition;

                    worksheet.Cells[row, 9].Value = rs.ErrorDeform;
                    worksheet.Cells[row, 10].Value = rs.ErrorScratch;
                    worksheet.Cells[row, 11].Value = rs.ErrorDirty;
                    
                    FileInfo fileInfo = new FileInfo(filePath);
                    package.SaveAs(fileInfo);
                }
            }
            catch (Exception ex)
            {
                Log.Error($"Error can not save file excel, error: {ex.Message}");
            }
        }
    }
}