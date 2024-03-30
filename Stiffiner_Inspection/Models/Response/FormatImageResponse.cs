namespace Stiffiner_Inspection.Models.Response
{
    public class FormatImageResponse
    {
        public int id { get; set; }
        public int dataId { get; set; }
        public string path { get; set; } = string.Empty;
        public int clientId { get; set; }
    }
}
