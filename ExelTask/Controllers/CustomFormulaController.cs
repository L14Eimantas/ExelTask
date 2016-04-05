using ExelTask.DAL;
using ExelTask.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Text.RegularExpressions;
using System.Web.Mvc;
using ExelTask.MyClass;
using SpreadsheetGear;
using ExelTask.Models;

namespace ExelTask.Controllers
{
    public class CustomFormulaController : Controller
    {
        // GET: CustomFormula
        private TaskContext db = new TaskContext();
        public ActionResult Index(int mId, string workbook, string worksheet, string cell, int whereCopy)
        {
            CustomFormulaData model = new CustomFormulaData();
            model.InputWorkbook = GetInputWorkbook(mId);
            model.AllWorksheet = GetRequireSheet(model.InputWorkbook);
            model.AllCell = GetRequiredCell(model.AllWorksheet);
            model.ExcelFormula = GetDefaultFormulas();
            ViewBag.Name = "Custom - " + workbook + ", " + worksheet + ", " + cell;
            ViewBag.Where = whereCopy;
            ViewBag.String = GetAllNames(model.AllCell);
            return View(model);
        }
    
        private dynamic GetAllNames(IList<Cell> list)
        {
            string ats = string.Empty;
            foreach (var a in list)
            {
                ats += (a.ColumnName + ",");
            }
            return ats;
        }

        private IList<Cell> GetRequiredCell(IList<Worksheet> list)
        {
            List<Cell> data = new List<Cell>();
            foreach(var a in list)
            {
                var data2 = from m in db.Cells
                            where m.WorksheetId == a.Id
                            select m;
                data.AddRange(data2);
            }
            return data;
        }

        private IList<Worksheet> GetRequireSheet(IList<Workbook> list)
        {
            List<Worksheet> data = new List<Worksheet>();
            foreach(var a in list)
            {
                var data2 = from m in db.Worksheets
                            where m.WorkbookId == a.Id
                            select m;
                data.AddRange(data2);
            }
            return data;
        }


        private IList<string> GetDefaultFormulas()
        {
            string[] defaultFormula = { "ACOS", "ASIN", "ATAN", "COS", "DOLLAR", "ROUND", "SIN", "SQRT", "SUM", "TAN" };
            return defaultFormula.ToList();
        }

        private IList<Models.Workbook> GetInputWorkbook(int mId)
        {
            var required = from m in db.Workbooks
                           where (mId == m.MappingId && m.IsInputOrOutput == true)
                           select m;
            return required.ToList();
        }
  /* /////////////////////////////////////////////////////////////// */
        private string GetColumnName(int columnId)
        {
            if (columnId != -1)
            {
                return db.Cells.Find(columnId).ColumnName;
            }
            else return "@#@!@#@#"; //then columnid is -1 
        }

        public ActionResult CheckCellId(string cellId, string  titles, string alltext)
        {
            if (string.IsNullOrEmpty(titles) || string.IsNullOrWhiteSpace(titles))
                return Json(new { answer = string.Empty }); // this mean that cellid string should be empty
            else
            {
                string[] newtitles = TitlesAdaptation(titles.Split(';')).Split('-');
                cellId = GetRequiredCellId(ref newtitles, cellId.Split(' '));
                alltext = GetNewTextAreaText(alltext,newtitles); // this need to delete wrong column 
                return Json(new { answer = cellId, newtext = alltext });
            }
        }

        private string GetNewTextAreaText(string alltext,string[] newtitles) // check and delete wrong column in textarea
        {
            for (int i=0; i<newtitles.Length-1; i++)
            {
                if (newtitles[i]!="")
                {
                    Regex regex = new Regex(newtitles[i]);
                    alltext = regex.Replace(alltext, "", 1);
                }
            }
            return alltext; // new textarea value
        }

        private string GetRequiredCellId(ref string[] newtitles, string[] p) //p is cellIds strings array
        {
            string answer = string.Empty;
            for(int i=0; i<newtitles.Length-1; i++ )
            {
                for (int j=i; j<p.Length-1; j++)
                {
                    if (newtitles[i]==GetColumnName(int.Parse(p[j])))
                    {
                        answer += (p[j]+" ");
                        newtitles[i] = "";  // "" if columns is exit in database
                        p[j] = "-1";
                        break;
                    }
                }
            }
            return answer; // return right columnId string that will be return to javascript
        }

        // this method return required columnsname string without number
        private string TitlesAdaptation(string[] p) //p is titles (ColumnName) 
        { 
            string interim = string.Empty;
            double sk;
            for (int i = 0; i < p.Length; i++)
            {
                if (p[i] != "" && !double.TryParse(p[i], out sk))
                {
                    interim += (p[i].Trim() + "-");
                }
            }
            return interim;// return required string 
        }

///////////////////////////////test formula//////////
        public ActionResult FormulaTest(string text, string titles, string cellId, int mId)
        {
            text = text.Replace(" ", "");
            titles = titles.Replace(" ", "");
            string[] newtitles = TitlesAdaptation(titles.Split(';')).Split('-');
            string[] realCellId = cellId.Split(' ');
            GetFirstData myData = new GetFirstData(mId);
            for (int i = 0; i < realCellId.Length - 1; i++)
            {
                Regex regex = new Regex(newtitles[i]);
                string data=myData.GetOneData(int.Parse(realCellId[i]));
                text = regex.Replace(text, data, 1);// this repclace 
            }

            return Json(new { TorF = FormulaEvalute(text) });
        }

        private bool FormulaEvalute(string text) // this method check if formula is right write
        {
            SpreadsheetGear.IWorkbook workbook = SpreadsheetGear.Factory.GetWorkbook();
            var val = workbook.Worksheets["Sheet1"].EvaluateValue(text);
            if (val != null && (val is double || val is string))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
/////////////////////////////
        public ActionResult GetTitles(string titles)
        {
            titles = titles.Replace(" ", "");
            titles = TitlesAdaptation(titles.Split(';'));
            titles = titles.Replace("-", " "); // this is required for RUN controler methods
            return Json(new { titles = titles });
        }
    }
}