using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace ExelTask.Models
{
    public class Worksheet
    {
        public int Id { get; set; }
        public int WorkbookId { get; set; }
        [StringLength(50)]
        public string WorksheetName { get; set; }
        public virtual ICollection<Cell> Cells { get; set; }
        public virtual Workbook Workbook { get; set; }
    }
}