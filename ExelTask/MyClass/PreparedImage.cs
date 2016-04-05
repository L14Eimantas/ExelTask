using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ExelTask.Models;
using ExelTask.DAL;
using SpreadsheetGear;

namespace ExelTask.MyClass
{
    public class PreparedImage
    {
        private Workbook workbook; //is model data (not spreadsheethear IWorkbook)
        private SpreadsheetGear.IWorksheet spreadworksheet;
        private SpreadsheetGear.IRange cells;
        private string titleRange = "$A$100:$AE$100";
        private string allRange = "$A$1:$AE$100";
        public PreparedImage(Workbook workbook, string worksheetName)
        {
            this.workbook = workbook;
            spreadworksheet = InitializationSpreedsheetGearWorksheet(worksheetName);
            cells = spreadworksheet.Cells;
            ApplyDeaultTitle(worksheetName);
        }

        public void FormatTitleStyle(int fontSize, int redColorNumber, int greenColorNumber, int blueColorNumber) 
        {
            cells[titleRange].Font.Size = fontSize;
            cells[titleRange].Font.Color = SpreadsheetGear.Color.FromArgb(redColorNumber, greenColorNumber, blueColorNumber);
            cells[titleRange].Borders.LineStyle = SpreadsheetGear.LineStyle.Dash;
            cells[titleRange].Borders.Weight = SpreadsheetGear.BorderWeight.Thick;
        }

        public SpreadsheetGear.Drawing.Image GetImage()
        {
            SpreadsheetGear.Drawing.Image image = new SpreadsheetGear.Drawing.Image(cells[allRange]);
            return image;
        }

        private void ApplyDeaultTitle(string worksheetName)
        {
            cells[titleRange].Merge();
            cells[titleRange].Font.Bold = true;
            cells[titleRange].Formula = "PREVIEW: " + workbook.WorkbookName + " " + worksheetName;
            cells[titleRange].HorizontalAlignment = SpreadsheetGear.HAlign.Center;
            cells[titleRange].VerticalAlignment = SpreadsheetGear.VAlign.Center;
        }
    
        private IWorksheet InitializationSpreedsheetGearWorksheet(string worksheetName) // Create needed worksheet
        {
            SpreadsheetGear.IWorkbookSet workbookSet = SpreadsheetGear.Factory.GetWorkbookSet();
            SpreadsheetGear.IWorkbook spreadworkbook = workbookSet.Workbooks.OpenFromMemory(workbook.WorkbookFile);
            return spreadworkbook.Worksheets[worksheetName];
        }
    }
}