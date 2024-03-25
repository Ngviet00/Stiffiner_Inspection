using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Stiffiner_Inspection.Models.Entity
{
    [Table("data")]
    public class Data
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("id", TypeName = "bigint")]
        public long Id { get; set; }

        [Column("time")]
        public DateTime? Time { get; set; }

        [Column("model"), MaxLength(255)]
        public string? Model { get; set; } = string.Empty;

        [Column("tray"), MaxLength(255)]
        public string? Tray { get; set; }

        [Column("client_id")]
        public int? ClientId { get; set; }

        [Column("side"), MaxLength(255)]
        public string? Side { get; set; } = string.Empty;

        [Column("index")]
        public int? Index { get; set; }

        [Column("camera"), MaxLength(255)]
        public string? Camera { get; set; } = string.Empty;

        [Column("result_area")]
        public int? ResultArea { get; set; }

        [Column("result_line")]
        public int? ResultLine { get; set; }

        [Column("target_id")]
        public int? TargetId { get; set; }
    }
}
