namespace Stiffiner_Inspection.Models.Response
{
    public class ExportDataResponse
    {
        public string? Model { get; set; }
        public string? Date { get; set; }
        public int? Inspection { get; set; }
        public int? OK { get; set; }
        public int? NG { get; set; }
        public double? NG_Percent { get; set; }
        public int? Particle { get; set; }
        public int? NGTapePosition { get; set; }
        public int? Deform { get; set; }
        public int? Scratch { get; set; }
        public int? Dirty { get; set; }
    }
}
