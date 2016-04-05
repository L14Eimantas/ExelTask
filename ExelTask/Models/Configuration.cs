using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ExelTask.Models
{
    public class Configuration
    {
        public int Id { get; set; }
        public string Columns { get; set; }
        public int Column2 { get; set; } //where to copy
        public string Action { get; set; }
        public string CustomCulumns { get; set; }
        public string CustomCulumnsName { get; set; }
        public int MappingId { get; set; }
        public virtual Mapping Mapping { get; set; }

    }
}