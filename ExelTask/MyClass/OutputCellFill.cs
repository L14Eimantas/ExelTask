using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using SpreadsheetGear;
using ExelTask.Models;
using ExelTask.DAL;

namespace ExelTask.MyClass
{
    public class OutputCellFill
    {
        private TaskContext db = new TaskContext();
        private IWorksheet worksheet;
        private int worksheetId;
        public OutputCellFill(IWorksheet worksheet, int worksheetId)
        {
            this.worksheet = worksheet;
            this.worksheetId = worksheetId;
        }

        public void SaveToDatabase()
        {
            string range = GetCorrectRange();
            if (range != "") // chech if row range exist
            {
                foreach (IRange cell in worksheet.Cells[range])
                {
                    int startRow = 0;
                    if ((cell.MergeCells || cell.Text != "") && cell.Columns.Hidden == false && DataExist(cell, worksheet,ref startRow))
                    {
                        var SaveCell = new Cell();
                        if (cell.MergeCells)
                        {
                            foreach (IRange cc in cell.MergeArea.Range)
                            {
                                SaveCell.HeaderRow = GetHeaderRow(cc);
                                SaveCell.ColumnName = cc.Text;
                                break; // because if cell is merge all information is saved to first cell
                            }
                        }
                        else
                        {
                            SaveCell.HeaderRow = GetHeaderRow(cell);
                            SaveCell.ColumnName = cell.Text;
                        }
                        SaveCell.WorksheetId = worksheetId;
                        SaveCell.StartRow = startRow;
                        SaveData(SaveCell);
                    }
                }
            }
            else
                DeleteWorksheetFromDatabase();
        }

        private void SaveData(Cell SaveCell)  // save cell data to database
        {
            if (SaveCell.ColumnName != "")
            {
                db.Cells.Add(SaveCell);
                db.SaveChanges();
            }
        }

        private int GetHeaderRow(IRange cell) // get header row number 
        {
            string address = cell.Address;
            return Convert.ToInt32(address.Remove(0, address.LastIndexOf('$') + 1));
        }

        private bool DataExist(IRange cell, IWorksheet worksheet, ref int startRow)
        {                                       //chech if in column exist any data, columns isn't empty (have just column name)
            bool data = false;
            for (int i = 1; i < 20; i++)
            {
                string aaa = Modify(i, cell.Address);
                if (worksheet.Cells[aaa].Text != "") 
                {
                    startRow = Convert.ToInt32(aaa.Remove(0, aaa.LastIndexOf('$') + 1)); // get startrow number
                    data = true;
                    break;
                }
            }
            return data;
        }

        private string Modify(int number, string address)// return new address (to the old address number add number that is indicated
        {
            int oldNumbet = Convert.ToInt32(address.Remove(0, address.LastIndexOf('$') + 1));
            string data = address.Remove(address.LastIndexOf('$') + 1, address.Length - address.LastIndexOf('$') - 1);
            data += (oldNumbet + number).ToString();
            return data;
        }

        private void DeleteWorksheetFromDatabase() // if worksheet is empty, then it is delete
        {
            db.Worksheets.Remove(db.Worksheets.Find(worksheetId));
            db.SaveChanges();
        }

        private string GetCorrectRange() // this return worksheet row range where column name is
        {
            string rowRange = "";
            try
            {
                if (worksheet.AutoFilter.Filters.Count > 2)
                    rowRange = GetRangeByAutoFilters(worksheet);
                else rowRange = GetRangeByRowsAndColumns(worksheet);
            }
            catch
            {
                rowRange = GetRangeByRowsAndColumns(worksheet);
            }
            return rowRange;
        }

        private string GetRangeByRowsAndColumns(IWorksheet worksheet) // get row range then worksheet don't have autofilters
        {
            string lastAddres = "";
            if (worksheet.UsedRange.Address.Split(':').Length != 1) // need because worksheet can be empty
                lastAddres = worksheet.UsedRange.Address.Split(':')[1]; // get last usedrange address for example $AE$322
            string num = "0";
            string range = "";
            foreach (IRange cell in worksheet.UsedRange)
            {
                if (IsEqualRowNumber(num, cell) && (cell.CurrentRegion.ColumnCount > 3 || cell.CurrentRegion.RowCount > 5) && cell.Text != "" && ChechMerge(cell.Address, lastAddres, worksheet, ref num))
                {
                    range = cell.Address;
                    break;
                }

            }
            if (range != "")
                range = GetNewRange(range, lastAddres);
            return range;
        }

        private bool ChechMerge(string p, string lastAddres, IWorksheet worksheet, ref string num) // chech if cell is merge (cells columns)
        {
            string range = string.Empty;
            num = p.Remove(0, p.LastIndexOf('$') + 1);
            range = GetNewRange(p, lastAddres);
            bool t = true;
            foreach (IRange cell in worksheet.Cells[range])
            {
                if (cell.MergeArea.ColumnCount > 1)
                {
                    t = false;
                    break;
                }
            }
            return t;
        }

        private bool IsEqualRowNumber(string answer, IRange cell)
        {
            string incomingRow = cell.Address.Remove(0, cell.Address.LastIndexOf('$') + 1);
            if (answer != incomingRow)
                return true;
            else return false;
        }

        private string GetRangeByAutoFilters(IWorksheet worksheet) // this return get range where autofilters is, because when filter exist this row is required
        {
            string range = string.Empty;
            string[] autoRange = worksheet.AutoFilter.Range.Address.Split(':');
            string[] usedRange = worksheet.UsedRange.Address.Split(':');
            range = GetNewRange(autoRange[0], usedRange[1]);
            return range;
        }

        private string GetNewRange(string start, string end) // this method return correct range from start position ($A$1 to the end position 
        {                                                  // $Z$33, the end position is change to $A$33, because is need one row range
            string range = string.Empty;
            if (end.Split('$').Length == 3)
                end = end.Remove(end.LastIndexOf('$'), end.Length - end.LastIndexOf('$'));
            end += start.Remove(0, start.LastIndexOf('$'));
            range = start + ":" + end;
            return range;
        }
    }
}