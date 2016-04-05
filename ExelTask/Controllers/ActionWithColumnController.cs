using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ExelTask.ViewModel;
using ExelTask.DAL;
using ExelTask.Models;
using System.Data.Entity;

namespace ExelTask.Controllers
{
    public class ActionWithColumnController : Controller
    {
        // GET: ActionWithColumn
        private TaskContext db = new TaskContext();
        private static int mId = 0;
        public ActionResult MainPageColumn(int ?Id) // this method find all required data and send them to view
        {
            mId = (Id ?? mId);
            ActionColumnModel ActionModel = new ActionColumnModel();
            ActionModel.InputWorkbook = InputWorkbook();//find all one mapping inputfile workbook
            ActionModel.TemplateWorkbook = TemplateWorkbook();// find all one mapping templatefile workbook
            ViewBag.num = ActionModel.TemplateWorkbook.Count;
            ActionModel.InputSheet = GetSheet(ActionModel.InputWorkbook);
            ActionModel.TemplateSheet = GetSheet(ActionModel.TemplateWorkbook);
            ActionModel.InputCell = GetCell(ActionModel.InputSheet);
            ActionModel.TemplateCell = GetCell(ActionModel.TemplateSheet);
            return View(ActionModel);
        }

        private IList<Cell> GetCell(IList<Worksheet> list) 
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

        private List<Worksheet> GetSheet(IList<Workbook> list)
        {
            List<Worksheet> data = new List<Worksheet>();
            foreach (var a in list)
            {
                var vv = from m in db.Worksheets
                         where m.WorkbookId == a.Id
                         select m;
                data.AddRange(vv);
            }
            return data;
        }

        public ActionResult CheckAndFill(string ColumnWhat, int ColumnWhere)
        {
         
            if (IsExist(ColumnWhere)) // IsExist return bool
            {
                var NewConfig = FindRequiredConfig(ColumnWhere).First();
                NewConfig.Columns += (ColumnWhat+ " ");
                NewConfig.Action += ("+"+" "); // default add + sign
                ModifyConfiguration(NewConfig);
                return Json("");
            }
            else
            {
                SaveNewConfigToDatabase(ColumnWhat, ColumnWhere);
                return Json("");
            }

        }

        public ActionResult ChangeActionSign(int columnWhere, string sign, int where)  // this method modify defult sign(+) to new that select user
        {
            var NewConfig = FindRequiredConfig(columnWhere).First();
            NewConfig.Action = GetSignChanges(NewConfig.Action, where, sign);
            ModifyConfiguration(NewConfig); //save modify data in database
            return Json("");
        }

        private string GetSignChanges(string p, int where, string sign) // return new Action to Configuration table
        {
            string[] allSign = p.Split(' ');
            allSign[where] = sign; // change one sign
            string newString=string.Empty;
            for (int i = 0; i < allSign.Length; i++)
            {
                if (allSign[i] != "")
                    newString += (allSign[i] + " "); //this put together new Action (+ - + * - +)
            }
            return newString;
        }

        private IList<Models.Workbook> TemplateWorkbook()
        {
            var required = from m in db.Workbooks
                           where (mId == m.MappingId && m.IsInputOrOutput == false) //chech that Workbooks mapping id was equil mId(one mapping Id) and IsInputOrOutput was equil false
                           select m;
            return required.ToList();
        }

        private IList<Models.Workbook> InputWorkbook()
        {
            var required = from m in db.Workbooks
                           where (mId == m.MappingId && m.IsInputOrOutput == true) 
                           select m;
            return required.ToList();
        }
       

        private void ModifyConfiguration(Configuration NewConfig) //this method save changes data in database (Configuration Table)
        {
            if (ModelState.IsValid)
            {
                db.Entry(NewConfig).State = EntityState.Modified;
                db.SaveChanges();
            }
        }


        private void SaveNewConfigToDatabase(string ColumnWhat, int ColumnWhere) //this method is doing then totaly new data comes to this controler (from ajax)
        {
            var Config = new Configuration();
            Config.MappingId = mId;
            Config.Columns = ColumnWhat+ " ";
            Config.Column2 = ColumnWhere;
            Config.Action = "+ ";//is default sign
            db.Configurations.Add(Config);
            db.SaveChanges();
        }

        private bool IsExist(int ColumnWhere) // chech if data already exist in Configuration table
        {
            var  Exist = FindRequiredConfig(ColumnWhere); 
            if (Exist.Any())
                return true;
            else
                return false;
        }

        private  IEnumerable<Configuration> FindRequiredConfig(int ColumnWhere) //this method find reauired data from 
        {                       //from Configuartons table
            return from m in db.Configurations
                           where (ColumnWhere == m.Column2) //if ColumnWhere (this data like Id, because just in one column save information)
                           select m;
            
        }

////////////////////////////////////////////////////Delete item
        
        public ActionResult DeleteItem(int where, int position, bool t)
        {

            var NewConfig = FindRequiredConfig(where).First();
            if (t == true)
                DeleteAll(NewConfig);
            else
            {
                DeleteItemCustom(NewConfig, position);// Checking if exist CustomFormula column
            }
            return Json("");
        }

        private void DeleteItemCustom(Configuration NewConfig, int position)
        {
            string[] chech = NewConfig.Columns.Split(' ');
            int sk;
            if (!int.TryParse(chech[position],out sk)) // if is custom_formula then delete CustomFormula data
            {
                int customPosition = GetCustomPosition(chech, chech[position]);// get customformula data(CustomCulumns and CustomCulumnName position)
                NewConfig.CustomCulumns = GetNewColumns(NewConfig.CustomCulumns.Split(';'), customPosition,";");
                NewConfig.CustomCulumnsName = GetNewColumns(NewConfig.CustomCulumnsName.Split(';'), customPosition,";");
            }
            NewConfig.Columns = GetNewColumns(NewConfig.Columns.Split(' '), position, " ");
            NewConfig.Action = GetNewColumns(NewConfig.Action.Split(' '), position," ");
            ModifyConfiguration(NewConfig);
        }

        private int GetCustomPosition(string[] chech, string p) //p custom formula
        {
            string customFormula=string.Empty; //customFormula save customformulas 
            int interim;
            for (int i=0; i<chech.Length-1; i++)
            {
                if(!int.TryParse(chech[i], out interim))
                    customFormula += (chech[i] + " ");// " " is trademark
            }
            int answer=0;
            string[] interimstring = customFormula.Split(' '); // spilt customFormula by trademark
            for (int i=0; i<interimstring.Length-1; i++)
            {
                if (p == interimstring[i]) // checks if delele cuctomFormula equil one element from interimstring array
                {
                    answer = i; // answer is required position (which custom formula data is needed delete)
                    break;
                }
            }
            return answer;
        }

        private string GetNewColumns(string[] p, int position, string sign) // delete one element and sort out new string
        {
            string newString = string.Empty;
            p[position] = "";
            for (int i=0; i<p.Length; i++)
            {
                if (p[i] != "")
                    newString += (p[i] + sign);
            }
            return newString;
        }

        private void DeleteAll(Configuration NewConfig)
        {
            db.Configurations.Remove(NewConfig);
            db.SaveChanges();
        }


///////////////////////////////////////////////////////////// MAIN PAGE IS OPEN


        [HttpGet]
        public ActionResult PageLoad()
        {
            var data = GetConfig();
            bool t = new bool(); // true or false
            int sk = data.ToList().Count();
            if (data.Any())  // if data exist
                t = true;
            else t = false;
            return Json(new { check = t, num = sk }, JsonRequestBehavior.AllowGet);
        }
        
        public ActionResult GetData(int num)   // this method return where copy, whatColumns is needed and what signs is needed
        {
            var data1=GetConfig().ToList();
            int WhereCopy = data1[num].Column2;
            string whatColumns = data1[num].Columns;
            string whatSigns = data1[num].Action;
            return Json(new { Where = WhereCopy, signs = whatSigns, columns = whatColumns });
        }

        public ActionResult GetName(string num)  // this method return column name
        {
            int sk;
            if (int.TryParse(num, out sk))
            {
                string name = db.Cells.Find(sk).ColumnName;
                return Json(new { name = name });
            }
            else
                return Json(new { name = num });
        }

        private IEnumerable<Configuration> GetConfig() // get configuration table data
        {
            return from m in db.Configurations
                   where m.MappingId == mId
                   select m;
        }
 ///////////////////////////////////////////Custom Formula

        public ActionResult CustomaAddData(string culumns, string names, int where) // add data to configturation customformula data
        {
            var NewConfig = FindRequiredConfig(where).First();
            if (!string.IsNullOrEmpty(culumns) && !string.IsNullOrWhiteSpace(culumns))
            {
                NewConfig.CustomCulumns += (culumns + ";");
                NewConfig.CustomCulumnsName += (names + ";");
            }
            else // when culomns is empty the write # this sign
            {
                NewConfig.CustomCulumns += ("#" + ";");
                NewConfig.CustomCulumnsName += ("#" + ";");
            }
            ModifyConfiguration(NewConfig); 
            return Json("");
        }

    }
}



