using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace ExelTask.Models
{
    public class OutputFile
    {
        public int Id { get; set; }
        public int MappingId { get; set; }
        public int TemplateFileId { get; set; }
        [Required]
        public byte[] FileOutput { get; set; }
        [Required]
        [StringLength(50)]
        public string FileName { get; set; }
        public virtual Mapping Mapping { get; set; }
    }
}