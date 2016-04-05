using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ExelTask.Models;
using ExelTask.DAL;
using SpreadsheetGear;

namespace ExelTask.MyClass
{
    public class OutputRequiredData
    {
       
        private TaskContext db = new TaskContext();
        protected string[] columnChar = { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z" }; //need this that copy columns in one sheet
        protected Configuration config;
        protected int mId;
        public OutputRequiredData (Configuration config, int mId)
        {
            this.config = config;
            this.mId = mId;
        }

        public OutputRequiredData(int mId)
        {
            this.mId = mId;
        }

        protected Cell GetRequiredCell(int cellId)
        {
            var required = from m in db.Cells
                           where m.Id == cellId
                           select m;
            return required.First();
        }

        protected byte[] GetWhereToCopy(int cellId)
        {
            var data = findWhereToCopy(cellId);
            if (data.Any())  // chech if any data exist 
            {
                return data.Last().FileOutput; //  data.Last because last data is new one (if it was first then not all change will be saved
            }
            else
            {
                return GetWorkbookFile(cellId);
            }
        }

        protected IEnumerable<OutputFile> findWhereToCopy(int cellId) // find outputfiles by cellId
        {
            int workbookId = GetTemplateFileId(cellId);
            return from m in db.OutputFiles
                   where m.TemplateFileId == workbookId && mId == m.MappingId
                   select m;
        }


        protected byte[] GetWorkbookFile(int cellId)
        {
            int worksheetId = GetRequiredCell(cellId).WorksheetId;
            Worksheet required = GetWorksheet(worksheetId);
            int workbookId = required.WorkbookId;
            Workbook workbook = GetWorkbook(workbookId);
            return workbook.WorkbookFile;
        }

        protected int GetTemplateFileId(int cellId)// find template by CellId
        {
            int worksheetId = GetRequiredCell(cellId).WorksheetId;
            Worksheet required = GetWorksheet(worksheetId);
            int workbookId = required.WorkbookId;
            Workbook workbook = GetWorkbook(workbookId);
            return workbook.Id;
        }

        private Workbook GetWorkbook(int workbookId) // find required workbook by workbook id
        {
            var data = from m in db.Workbooks
                       where m.Id == workbookId
                       select m;
            return data.First();
        }

        private Worksheet GetWorksheet(int WorksheetId) // find required worksheet by WorksheetId
        {
            var data = from m in db.Worksheets
                       where m.Id == WorksheetId
                       select m;
            return data.First();
        }

        protected string GetWorkbookName(int cellId) 
        {
            int worksheetId = GetRequiredCell(cellId).WorksheetId;
            Worksheet required = GetWorksheet(worksheetId);
            int workbookId = required.WorkbookId;
            Workbook workbook = GetWorkbook(workbookId);
            return workbook.WorkbookName;
        }

        protected string GetCellWorksheetName(int cellId) // get worksheetName
        {
            int worksheetId = GetRequiredCell(cellId).WorksheetId;
            Worksheet required = GetWorksheet(worksheetId);
            return required.WorksheetName;
        }
    }
}