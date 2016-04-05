using ExelTask.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ExelTask.ViewModel
{
    public class CustomFormulaData
    {
        public IList<Workbook> InputWorkbook{get; set;}
        public IList<Worksheet> AllWorksheet { get; set; }
        public IList<Cell> AllCell { get; set; }
        public IList<string> ExcelFormula { get; set; }
    }
}