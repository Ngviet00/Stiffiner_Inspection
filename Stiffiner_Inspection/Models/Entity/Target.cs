using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Stiffiner_Inspection.Models.Entity
{
    [Table("targets")]
    public class Target
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("target_id", TypeName = "bigint")]
        public long TargetId { get; set; }

        [Column("target_qty", TypeName = "int")]
        public int Target_qty { get; set; }

        [Column("created_date", TypeName = "DateTime")]
        public DateTime Created_date { get; set; }

        [Column("updated_date", TypeName = "DateTime")]
        public DateTime Updated_date { get; set; }
    }
}
