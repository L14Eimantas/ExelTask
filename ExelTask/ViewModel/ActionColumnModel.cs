using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ExelTask.Models;

namespace ExelTask.ViewModel
{
    public class ActionColumnModel
    {
        public IList<Workbook> InputWorkbook { get; set; }
        public IList<Workbook> TemplateWorkbook { get; set; }
        public IList<Worksheet> InputSheet { get; set; }
        public IList<Worksheet> TemplateSheet { get; set; }
        public IList<Cell> InputCell { get; set; }
        public IList<Cell> TemplateCell { get; set; }
    }
}