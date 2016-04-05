using ExelTask.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ExelTask.ViewModel
{
    public class FileUploadModel
    {
        public List<Cell> requiredCell { get; set; }
        public IEnumerable<Workbook> requiredWorkbook { get; set; }
        public IEnumerable<Worksheet> requiredWorksheet { get; set; }
    }
}