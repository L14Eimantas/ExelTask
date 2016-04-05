using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace ExelTask.Models
{
    public class Mapping
    {
        public int Id { get; set; }
        [Required]
        [StringLength(50)]
        [Display(Name="Mapping Name")]
        public string MappingName { get; set; }
        [Required]
        [StringLength(50)]
        [Display(Name="Client")]
        public string ClieantName { get; set; }
        [Required]
        [StringLength(50)]
        [Display(Name="Module")]
        public string ModuleName { get; set; }
        [StringLength(50)]
        public string Tags { get; set; }

        public virtual ICollection<Configuration> Configurations { get; set; }
        public virtual ICollection<History> Histories { get; set; }
        public virtual ICollection<OutputFile> OutputFiles { get; set; }
        public virtual ICollection<Workbook> Workbooks { get; set; }
    }
}