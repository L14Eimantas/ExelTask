using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ExelTask.Models;
using ExelTask.DAL;
using SpreadsheetGear;
using System.Data.Entity;

namespace ExelTask.MyClass
{
    public class GetFirstData:OutputRequiredData
    {
        public GetFirstData(int mId):base(mId)
        {}

        public string GetOneData(int cellId) //this method gives us one data
        {
            SpreadsheetGear.IWorkbookSet workbookset = SpreadsheetGear.Factory.GetWorkbookSet();
            SpreadsheetGear.IWorkbook workbook = workbookset.Workbooks.OpenFromMemory(GetWorkbookFile(cellId));
            string columnName = GetRequiredCell(cellId).ColumnName;
            string worksheetName = GetCellWorksheetName(cellId);
            string address = FindAddressByName(workbook.Worksheets[worksheetName], columnName);
            int diference = GetRequiredCell(cellId).StartRow - GetRequiredCell(cellId).HeaderRow;
            address = Modify(diference, address);
            string data = workbook.Worksheets[worksheetName].Cells[address].Value.ToString();
            return data;
        }

        private string FindAddressByName(IWorksheet worksheet, string p) // p - columnName
        {
            string address = worksheet.Cells.Find(p, null, SpreadsheetGear.FindLookIn.Values, SpreadsheetGear.LookAt.Part, SpreadsheetGear.SearchOrder.ByRows, SpreadsheetGear.SearchDirection.Next, true).Address;
            return address;
        }

        private string Modify(int number, string address) // change column number for example we have $A$11 after this will be $A$12 if number is 1
        {
            int oldNumbet = Convert.ToInt32(address.Remove(0, address.LastIndexOf('$') + 1));
            string data = address.Remove(address.LastIndexOf('$') + 1, address.Length - address.LastIndexOf('$') - 1);
            data += (oldNumbet + number).ToString();
            return data;
        }
    }
}