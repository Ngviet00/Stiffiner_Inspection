namespace Stiffiner_Inspection.Models.DTO.Data
{
    public class DataCSV
    {
        public string? model { get; set; } = string.Empty;
        public DateTime? time { get; set; }
        public int index { get; set; }
        public string? result_area { get; set; }
        public string? result_line { get; set; }
        public string? image { get; set; }
        public string? errors { get; set; }
    }
}