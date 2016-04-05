using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using SpreadsheetGear;
using ExelTask.Models;
using ExelTask.DAL;
using System.Data.Entity;

namespace ExelTask.MyClass
{
    public class ChangeHeadersNamesInFile
    {
        private int workbookID;
        private int worksheetID;
        private string worksheetName;
        private Workbook requiredWorkbook;
        private List<Cell> newData;
        private TaskContext db = new TaskContext();
        private List<Cell> oldNames;

        public ChangeHeadersNamesInFile(int workbookID, int worksheetID, List<Cell> newData)
        {
            this.workbookID = workbookID;
            this.worksheetName = db.Worksheets.Find(worksheetID).WorksheetName;
            this.worksheetID = worksheetID;
            this.newData = newData;
            this.requiredWorkbook = db.Workbooks.Find(workbookID);
        }

        public void ChangeNamesInWorkbook()
        {
            oldNames = GetOldData(worksheetID).ToList();
            IWorkbook excelWorkbook = GetWorkbookFile(requiredWorkbook.WorkbookFile);
            for (int i=0; i<newData.Count; i++)
            {
                ChangeHeaderName(ref excelWorkbook, oldNames[i].ColumnName, newData[i].ColumnName);
            }
            SaveModifyData(excelWorkbook);
        }

        private void SaveModifyData(IWorkbook excelWorkbook)
        {
            requiredWorkbook.WorkbookFile = excelWorkbook.SaveToMemory(SpreadsheetGear.FileFormat.OpenXMLWorkbook);
            db.Entry(requiredWorkbook).State = EntityState.Modified;
            db.SaveChanges();
        }

        private IWorkbook GetWorkbookFile(byte[] p) // p - excel workbook bibary file 
        {
            SpreadsheetGear.IWorkbookSet workbookset = SpreadsheetGear.Factory.GetWorkbookSet();
            SpreadsheetGear.IWorkbook workbook = workbookset.Workbooks.OpenFromMemory(p); // workbook is required excel file, not binary
            return workbook;
        }

        private IEnumerable<Cell> GetOldData(int worksheetID) // Get old data bu worksheetID
        {
            return from m in db.Cells
                   where m.WorksheetId == worksheetID
                   select m;
        }

        private void ChangeHeaderName(ref IWorkbook workbook, string oldName, string newName)
        {
            //address is oldName address (for example "A1") 
            string address = workbook.Worksheets[worksheetName].Cells.Find(oldName, null, SpreadsheetGear.FindLookIn.Values, SpreadsheetGear.LookAt.Part, SpreadsheetGear.SearchOrder.ByRows, SpreadsheetGear.SearchDirection.Next, true).Address;
            workbook.Worksheets[worksheetName].Cells[address].Formula = newName;
        }
    }
}