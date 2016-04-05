using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ExelTask.Models;
using ExelTask.DAL;
using SpreadsheetGear;

namespace ExelTask.MyClass
{
    public class CellFilling
    {
        private TaskContext db = new TaskContext();
        private IWorksheet worksheet;
        private int worksheetId;
        public CellFilling(IWorksheet worksheet, int worksheetId)
        {
            this.worksheet = worksheet;
            this.worksheetId = worksheetId;
        }

        public void SaveWorksheetCellsToDatabase()
        {
            string answer = RowGuess(); //HeaderRow 
            foreach(IRange cell in worksheet.UsedRange)
            {
                if (IsEqualRowNumber(answer, cell) && !CellIsNumber(cell) && !IsCellEmpty(cell))
                {
                    var SaveCell = new Cell();
                    SaveCell.HeaderRow = int.Parse(answer);
                    SaveCell.StartRow = int.Parse(answer) + 1;
                    SaveCell.WorksheetId = worksheetId;
                    SaveCell.ColumnName = cell.Text;
                    db.Cells.Add(SaveCell);
                    db.SaveChanges();
                }
            }
        }
        private string RowGuess() //Get HeaderRow (Works with template files, because he find last number row)
        {
            string answer = "1"; 
            string column = "A"; 
            int counter = 0;
            SpreadsheetGear.IRange cel = worksheet.Cells;// need check if cell after it is empty
            foreach (IRange cell in worksheet.UsedRange)
            {
                CountNotEmptyCells(ref counter, cell);// counter is use check worksheet is worksheet is empty (counter =0) and then delete worksheet from database
                answer = cell.Address.Remove(0, cell.Address.LastIndexOf('$') + 1);// 
                column = cell.Address.Remove(0, 1);
                column = column.Remove(column.IndexOf('$'), column.Length - column.IndexOf('$'));
                if (IsNextCellEmpty(cel, column, answer) && !CellIsNumber(cell) && !IsCellEmpty(cell))
                    break;
            }
            if (counter == 0)
                DeleteWorksheetFromDatabase();
            return answer; //return Row number
        }

        private void CountNotEmptyCells(ref int counter, IRange cell)
        {
            if (cell.ValueType != SpreadsheetGear.ValueType.Empty)
                counter++;
        }

        private void DeleteWorksheetFromDatabase()
        {
            db.Worksheets.Remove(db.Worksheets.Find(worksheetId));
            db.SaveChanges();
        }

        private bool IsCellEmpty(IRange cell)
        {
            if (cell.Text == "")
                return true;
            else return false;
        }

        private static bool IsNextCellEmpty(IRange cel, string column, string answer) //check if cell after or (after after) it is empty
        {
            if (cel[column + (int.Parse(answer) + 1).ToString()].Text != "" ||
                 cel[column + (int.Parse(answer) + 2).ToString()].Text != "")
                return true;
            else return false;
        }
        private static bool IsEqualRowNumber(string answer, IRange cell)
        {
            string incomingRow = cell.Address.Remove(0, cell.Address.LastIndexOf('$') + 1);
            if (answer == incomingRow)
                return true;
            else return false;
        }
        private static bool CellIsNumber(IRange cel)
        {
            int sk;//temporary data
            if (int.TryParse(cel.Text, out sk))
                return true;
            return false;
        }

    }
}