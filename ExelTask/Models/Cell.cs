using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace ExelTask.Models
{
    public class Cell
    {
        public int Id { get; set; }
        [Display(Name = "Header Row")]
        [Range(1, int.MaxValue, ErrorMessage = "The value must be greater than 0")]
        public int HeaderRow { get; set; }
        [Display(Name = "Start Row")]
        [Range(1, int.MaxValue, ErrorMessage = "The value must be greater than 0")]
        public int StartRow { get; set; }
        [StringLength(100)]
        [Required]
        [Display(Name = "Column Name")]
        public string ColumnName { get; set; }
        [ForeignKey("Worksheet")]
        public int WorksheetId { get; set; }
        public virtual Worksheet Worksheet { get; set; }
    }
}