using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Stiffiner_Inspection.Models.Entity
{
    [Table("time_logs")]
    public class TimeLog
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("id", TypeName = "bigint")]
        public long Id { get; set; }

        [Column("time"), MaxLength(255)]
        public DateTime? Time { get; set; }

        [Column("type"), MaxLength(255)]
        public string? Type { get; set; } = string.Empty;

        [Column("message"), MaxLength(255)]
        public string? Message { get; set; } = string.Empty;
    }
}
