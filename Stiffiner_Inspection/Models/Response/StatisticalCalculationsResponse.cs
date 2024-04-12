namespace Stiffiner_Inspection.Models.Response
{
    public class StatisticalCalculationsResponse
    {
        public int TotalTray { get; set; }
        public double Total { get; set; }
        public int TotalOK { get; set; }
        public int TotalNG { get; set; }
        public int TotalEmpty { get; set; }
        public double PercentChartOk { get; set; }
        public double percentChartNG { get; set; }
        public double percentChartEmpty { get; set; }
    }
}
