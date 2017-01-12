using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PCGuardDB.DBModel
{
    [Table("history")]
    public class history
    {
        public history()
        {
        }

        [Key]
        [Column(Order = 3)]
        [StringLength(500)]
        public string url { get; set; }

        [Key]
        [Column(Order = 2)]
        [StringLength(500)]
        public string title { get; set; }

        [Key]
        [Column(Order = 1)]
        [StringLength(20)]
        public string browser_type { get; set; }

        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:yyyy/MM/dd hh:mm:ss}")]
        [DataType(DataType.Date)]
        public DateTime? last_visit_time { get; set; }

        [StringLength(50)]
        public string emp_no { get; set; }

        [StringLength(50)]
        public string dept_no { get; set; }
    }
}
