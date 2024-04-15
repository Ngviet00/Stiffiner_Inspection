using Stiffiner_Inspection.Models.Entity;

namespace Stiffiner_Inspection.Models.Response
{
    public class SearchDataResponse
    {
        public double Total { get; set; }
        public int TotalOK { get; set; }
        public int TotalNG { get; set; }
        public int TotalEmpty { get; set; }
        public int TotalTray { get; set; }

        public double PercentOK { get; set; }
        public double PercentNG { get; set; }
        public double PercentEmpty { get; set; }

        public List<Data> results { get; set; }
    }
}
