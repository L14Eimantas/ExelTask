using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ExelTask.Models;
using ExelTask.DAL;
using SpreadsheetGear;
using System.Data.Entity;
using System.Web.Mvc;
using System.Text.RegularExpressions;


namespace ExelTask.MyClass
{
    public class FillOutputFile:OutputRequiredData
    {
        private TaskContext db = new TaskContext();
        private string[] actions; // get * + - / * 
        private string [] columns; // get  columns ID  from string
        private string[] customcolumns;
        private string[] customnames;
        private int number; //number customformula data index(split columnsname)
        public FillOutputFile (Configuration config, int mId):base (config, mId)
        {
            actions = config.Action.Split(' ');
            columns = config.Columns.Split(' ');
            number = 0;
            if (!string.IsNullOrEmpty(config.CustomCulumns) && !string.IsNullOrWhiteSpace(config.CustomCulumns))
            {
                customcolumns = config.CustomCulumns.Split(';');
                customnames = config.CustomCulumnsName.Split(';');
            }
        }

        public void GetOutputFile()
        {
            OutputFile file = new OutputFile();
            file.MappingId = mId;
            file.FileName = GetWorkbookName(config.Column2);
            file.TemplateFileId = GetTemplateFileId(config.Column2);
            file.FileOutput = GetOutFile(config.Column2);
            db.OutputFiles.Add(file);
            db.SaveChanges();
        }

        private byte[] GetOutFile(int columnWhere) 
        {
            SpreadsheetGear.IWorkbookSet workbookset = SpreadsheetGear.Factory.GetWorkbookSet();
            SpreadsheetGear.IWorkbook interim = workbookset.Workbooks.Add();
            SpreadsheetGear.IWorkbook workbookWhere = workbookset.Workbooks.OpenFromMemory(GetWhereToCopy(columnWhere));
            Cell requiredcell = GetRequiredCell(columnWhere); // this 3 data until number need to workbookWhere
            string name = GetCellWorksheetName(columnWhere);
            string address = FindColumnWhereCopy(workbookWhere, requiredcell.ColumnName, name);
            PutemporaryData(ref interim);
            int number = interim.Worksheets[0].Cells["A1"].CurrentRegion.RowCount; // number to need for cycle
            if (actions.Length == 2) // because last element is ""
                return JustCopyOperation(requiredcell, interim, workbookWhere, name, address, number);
            return MathActions(interim, workbookWhere, number, requiredcell, name, address);
        }
/*                                 */
        private void PutemporaryData(ref IWorkbook interim) //this prepared interim workbook data
        {
            int sk;
            for (int i =0; i<actions.Length-1; i++)
            {
                string cell =columnChar[i] + (1).ToString();
                if (int.TryParse(columns[i], out sk))
                    CopyToNew(ref interim, int.Parse(columns[i]), cell);
                else
                    CostumFormulaToNew(ref interim, columns[i], cell);
            }
        }

        //this method calculate custom formula to required interim workbook column
        private void CostumFormulaToNew(ref IWorkbook interim, string p, string cell) // p is customformula
        {
            SpreadsheetGear.IWorkbookSet workbookset = SpreadsheetGear.Factory.GetWorkbookSet();
            SpreadsheetGear.IWorkbook costumFormula = workbookset.Workbooks.Add();
            if (customcolumns[number]!="#")
            {
                string[] cColumns = customcolumns[number].Split(' ');
                for(int i=0; i<cColumns.Length-1; i++)
                {
                    string customCell = columnChar[i] + 1.ToString();
                    CopyToNew(ref costumFormula, int.Parse(cColumns[i]), customCell);// prepare costum formula required columns
                }
                int quantity = costumFormula.Worksheets[0].Cells["A1"].CurrentRegion.RowCount;
                PerformCustomFormula(ref costumFormula, p, cColumns.Length - 1, customnames[number].Split(' '), quantity);
                costumFormula.Worksheets[0].Cells["AA1:" + "AA" + quantity.ToString()].Copy(interim.Worksheets[0].Cells[cell]);
            }
            else
            {
                interim.Worksheets[0].Cells[cell].Value = costumFormula.Worksheets[0].EvaluateValue(p);
            }
            number++;
        }

        // this calculate customformula
        private void PerformCustomFormula(ref IWorkbook costumFormula, string p1, int p2, string[] p3, int number) // p1 customformula p2 columns length, p3 - columns name
        {
           
            for (int i=1; i<number; i++)
            {
                string formulaText = p1;
                for (int j=0; j<p2; j++)
                {
                    string columncell=columnChar[j]+i.ToString();
                    Regex regex = new Regex(p3[j]);
                    formulaText = regex.Replace(formulaText,columncell, 1);// this repclace 
                }
                costumFormula.Worksheets[0].Cells["AA" + i.ToString()].Value = costumFormula.Worksheets[0].EvaluateValue(formulaText);
            
            }
        }
   /*////////////////////////////////////////////*/
        private byte[] MathActions(IWorkbook interim, IWorkbook workbookWhere, int number, Cell requiredcell, string name, string address)
        {
            for (int i = 1; i <= number; i++)
            {
                string ats = "=";
                for (int j = 0; j<actions.Length-1; j++)
                {
                    ats += columnChar[j] + i.ToString() + actions[j];
                }
                ats = ats.Remove(ats.Length - 1, 1);
                interim.Worksheets[0].Cells["AA" + i.ToString()].Formula = ats;
            }
            interim.Worksheets[0].Cells["AA1:AA" + number.ToString()].Value = interim.Worksheets[0].Cells["AA1:AA" + number.ToString()].Value;
            interim.Worksheets[0].Cells["AA1:" + "AA" + number.ToString()].Copy(workbookWhere.Worksheets[name].Cells[address]);
            return workbookWhere.SaveToMemory(SpreadsheetGear.FileFormat.OpenXMLWorkbook);
        }

        private byte[] JustCopyOperation(Cell requiredcell, IWorkbook interim, IWorkbook workbookWhere, string name, string address, int number)
        {
            interim.Worksheets[0].Cells["A1:" + "A" + number.ToString()].Copy(workbookWhere.Worksheets[name].Cells[address]);
            return workbookWhere.SaveToMemory(SpreadsheetGear.FileFormat.OpenXMLWorkbook);
        }

        private void CopyToNew(ref IWorkbook interim, int columnId, string cell) // this method copy one column to interrim column worksheet
        {
            SpreadsheetGear.IWorkbookSet workbookset = SpreadsheetGear.Factory.GetWorkbookSet();
            SpreadsheetGear.IWorkbook workbook = workbookset.Workbooks.OpenFromMemory(GetWorkbookFile(columnId));
            string columnName = GetRequiredCell(columnId).ColumnName;
            string worksheetName = GetCellWorksheetName(columnId);
            string address = FindAddressByName(workbook.Worksheets[worksheetName], columnName);
            string columnLast = FindLastRow(address, workbook.Worksheets[worksheetName], GetRequiredCell(columnId).StartRow);
            int diference = GetRequiredCell(columnId).StartRow - GetRequiredCell(columnId).HeaderRow;
            address = Modify(diference, address);
            workbook.Worksheets[worksheetName].Cells[address + ":" + columnLast].Copy(interim.Worksheets[0].Cells[cell]);

        }

        private string FindLastRow(string address, IWorksheet worksheet, int starRow)
        {
            string newAddress = address.Remove(address.LastIndexOf('$') + 1, address.Length - address.LastIndexOf('$') - 1)+starRow.ToString();
            int number = worksheet.Cells[newAddress].CurrentRegion.RowCount; // calculate how many rows have one column
            string data = Modify(number, newAddress);
            return data;
        }

        private string Modify(int number, string address) // change column number for example we have $A$11 after this will be $A$12 if number is 1
        {
            int oldNumbet = Convert.ToInt32(address.Remove(0, address.LastIndexOf('$') + 1));
            string data = address.Remove(address.LastIndexOf('$') + 1, address.Length - address.LastIndexOf('$') - 1);
            data += (oldNumbet + number).ToString();
            return data;
        }

        private string FindAddressByName(IWorksheet worksheet, string p) // p - columnName
        {
            string address = worksheet.Cells.Find(p, null, SpreadsheetGear.FindLookIn.Values, SpreadsheetGear.LookAt.Part, SpreadsheetGear.SearchOrder.ByRows, SpreadsheetGear.SearchDirection.Next, true).Address;
            return address;
        }

        private string FindColumnWhereCopy(IWorkbook workbook, string columnName, string nameWorksheet) // find column (A,B,C..) where will be write answer
        {
            string column = "";
            foreach (IRange cell in workbook.Worksheets[nameWorksheet].UsedRange)
            {
                try
                {
                    if (cell.Text == columnName)
                    {
                        column = cell.Address; // Get column (A,B,C)
                        column = Modify(1, column);
                        break;
                    }
                }
                catch { }
            }
            return column;
        }
        
    }
}