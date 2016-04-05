using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using ExelTask.DAL;
using ExelTask.Models;
using System.IO;
using SpreadsheetGear;
using ExelTask.MyClass;
using System.Data;
using System;
using ExelTask.ViewModel;

namespace ExelTask.Controllers
{
    public class UploadController : Controller
    {
        // GET: Upload
        private static int mId = 0; //Mappind id
        private TaskContext db = new TaskContext();
        public ActionResult UploadIndex(int? id, int? WBid, int? WSid)//id-mappindid and WBid - Workbook id WSid=WorksheetId
        {
            mId = (id ?? mId); //if id==null then mId=mId, else mId=id
            CheckIsWorkbookEmpty(RequiredWorkbook());
            FileUploadModel FileUpload = new FileUploadModel();// create viewmodel (It have Workbook, Cell, Worksheet array) this need because uploadindex view uses 3 diferent model (Viewbag isnt strong type)
            FileUpload.requiredWorkbook = RequiredWorkbook();
            ViewBag.WorkbookSelect = WBid; // it is required, because program should now then Workbook is selected
            FileUpload.requiredWorksheet = Correctworksheet(WBid);
            ViewBag.WorksheetSelect = WSid; // it is required, because program should now then Worksheet is selected (in View button color is change)
            var cell = from m in db.Cells               //finds necessaries cells by worksheetId (WSid)
                               where m.WorksheetId == WSid
                               select m;
            FileUpload.requiredCell = cell.ToList();
            return View(FileUpload);
        }

        private void CheckIsWorkbookEmpty(IEnumerable<Workbook> dynamic)  // delete empty workbook, which dont have any worksheets
        {
            foreach(var a in dynamic)
            {
                var data = from m in db.Worksheets
                           where a.Id == m.WorkbookId
                           select m;
                if(!data.Any())
                {
                    db.Workbooks.Remove(a);
                }
            }
            db.SaveChanges();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]  //this methon edit and validate cell tables
        public ActionResult UploadIndex(FileUploadModel model, int WBid, int WSid) //fix all this
        {
            if (ModelState.IsValid) //check if all this isn't empty
            {
                ChangeHeadersNamesInFile Save = new ChangeHeadersNamesInFile(WBid, WSid, model.requiredCell);
                Save.ChangeNamesInWorkbook();
                foreach (var Change in model.requiredCell.Where(m => m.WorksheetId==WSid))
                {
                    db.Entry(Change).State = EntityState.Modified;
                    db.SaveChanges();
                }
                return RedirectToAction("Index", "TemplateUpload", new { id = mId });//temporarily change to Upload template send mId
            }
            FileUploadModel FileUpload = new FileUploadModel();
            FileUpload.requiredWorkbook = RequiredWorkbook();
            ViewBag.WorkbookSelect = WBid; // it is required, because program should now then Workbook is selected
            FileUpload.requiredWorksheet = Correctworksheet(WBid);
            ViewBag.WorksheetSelect = WSid;
            FileUpload.requiredCell = model.requiredCell.Where(m => m.WorksheetId == WSid).ToList();//find required  cell from model
            return View(FileUpload);
        }

        [NonAction]
        private dynamic RequiredWorkbook() // finds necessaries Workbooks by MappingId and by Input or output(template)
        {
            var requiredWorkbook = from m in db.Workbooks
                                   where (m.MappingId == mId && m.IsInputOrOutput == true)
                                   select m;
            return requiredWorkbook;
        }

        [NonAction]
        private dynamic Correctworksheet(int? WBid) //find necessaries Worksheets by workbookID
        {
            var correctworksheet = from m in db.Worksheets
                                  where (m.WorkbookId == (WBid ?? 0))
                                  select m;
            return correctworksheet;
        }

        [HttpPost]
        public ActionResult Upload(HttpPostedFileBase file)
        {
            
            if (file != null && mId!=0)
            {
                string ats = file.FileName.Remove(0, file.FileName.LastIndexOf('.') + 1); //find upload file ending
                if (!ats.Equals("xls") && !ats.Equals("xlsx")) // by ats(file ending) check if upload file is MS EXCEL file
                {
                    ViewBag.Path = "Error: You can only upload Microsoft Excel File";
                    return View();
                }
                var workbook = new Workbook();
                WorkbookInitialization(ref workbook,file);
                db.Workbooks.Add(workbook); //temporary data add to Workbooks model
                db.SaveChanges(); //save all change in sql database
                WorksheetInitialization(workbook.Id, workbook.WorkbookFile);
                return RedirectToAction("UploadIndex", new { id = mId});
            }
            else ViewBag.Path = "Error: You didn't select a File";
            return View();
        }
        [NonAction]
        private void WorksheetInitialization(int p1, byte[] p2) //initialization worksheet and call class who initialization cell
        {                            // p1 is worksheet id p2 is file 
            SpreadsheetGear.IWorkbookSet workbookSet = SpreadsheetGear.Factory.GetWorkbookSet();
            SpreadsheetGear.IWorkbook workbook = workbookSet.Workbooks.OpenFromMemory(p2);
            foreach(IWorksheet worksheet in workbook.Sheets)
            {
                var workSheet = new Worksheet(); 
                workSheet.WorkbookId = p1;
                workSheet.WorksheetName = worksheet.Name;
                db.Worksheets.Add(workSheet);
                db.SaveChanges();
                var SaveCells = new OutputCellFill(worksheet,workSheet.Id); //Saving Cell to database
                SaveCells.SaveToDatabase();
            }
        }

        [NonAction]
        private void WorkbookInitialization(ref Workbook workbook, HttpPostedFileBase file) //It is method that add new Initialization Workbook
        {
            workbook.MappingId = mId;
            workbook.IsInputOrOutput = true;
            workbook.WorkbookName = file.FileName;
            using (var reader = new BinaryReader(file.InputStream))
            {
                workbook.WorkbookFile = reader.ReadBytes(file.ContentLength); // save binary file to temporary data workbook
            }
        }
        // GET: Upload/UploadDelete/5
        public ActionResult DeleteUpload(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Workbook workbook = db.Workbooks.Find(id);
            if (workbook == null)
            {
                return HttpNotFound();
            }
            return View(workbook);
        }

        // POST: Upload/UploadDelete/5
        [HttpPost, ActionName("DeleteUpload")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Workbook workbook = db.Workbooks.Find(id);
            db.Workbooks.Remove(workbook);
            var Config = GetConfigurations();
            db.Configurations.RemoveRange(Config);
            db.SaveChanges();
            return RedirectToAction("UploadIndex"); //if works bad add , new {id= workbook.MappingId}
        }

        private IEnumerable<Configuration> GetConfigurations() // need delete all Configuration if one file has delete
        {
            return from m in db.Configurations
                   where m.MappingId == mId
                   select m;
        }
 
       

        public void ViewWorksheet(int sheetID) // show worksheet view like image in browser
        {
            string worksheetName= db.Worksheets.Find(sheetID).WorksheetName; //find worksheetname 
            var workbook = db.Workbooks.Find(db.Worksheets.Find(sheetID).WorkbookId);
            var Image = new PreparedImage(workbook,worksheetName);
            Image.FormatTitleStyle(14,25,100,250);
            using (System.Drawing.Bitmap bitmap = Image.GetImage().GetBitmap())
            {
                Response.Clear();
                Response.ContentType = "image/png";
                System.IO.MemoryStream memoryStream = new System.IO.MemoryStream();
                bitmap.Save(memoryStream, System.Drawing.Imaging.ImageFormat.Png);
                memoryStream.WriteTo(Response.OutputStream);
            }
        }
    }
}