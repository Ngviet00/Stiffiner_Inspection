using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Stiffiner_Inspection.Models.Entity
{
    [Table("status_cam")]
    public class StatusCAM
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("id", TypeName = "int")]
        public long Id { get; set; }

        [Column("status")]
        public int? Status { get; set; } = 0;
    }
}
