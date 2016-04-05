using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace ExelTask.Models
{
    public class History
    {
        public int Id { get; set; }
        [Required]
        [StringLength(50)]
        public string Status { get; set; }
        [Required]
        [StringLength(50)]
        [Display(Name = "Ran By")]
        public string RanBy { get; set; }
        public int MappingId { get; set; }
        public DateTime Date { get; set; }
        public virtual Mapping Mapping { get; set; }
    }
}