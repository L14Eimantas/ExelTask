using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace ExelTask.Models
{
    public class Workbook
    {
        //back required again in later
        public int Id { get; set; }
       // [Required]
        [StringLength(50)]
        [Display(Name="File Name")]
        public string WorkbookName { get; set; }
       // [Required]
        public byte[] WorkbookFile { get; set; }
        public int MappingId { get; set; }
        public bool IsInputOrOutput { get; set; }
        public virtual Mapping Mapping { get; set; }
        public virtual ICollection<Worksheet> Worksheets { get; set; }
    }
}