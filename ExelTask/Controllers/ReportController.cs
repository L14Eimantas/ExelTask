using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ExelTask.Models;
using ExelTask.DAL;
using System.Data.Entity;

namespace ExelTask.Controllers
{
    public class ReportController : Controller
    {
        // GET: Report
        private TaskContext db = new TaskContext();
        public ActionResult Index()
        {
            
            return View(db.Histories.ToList());
        }


        public ActionResult ChechHistory(int mId)
        {
            var data = GetOldHistory(mId);
            if (!data.Any())
                AddNewReport(mId);
            else
            {
                data.First().Date = DateTime.Now;
                db.Entry(data.First()).State = EntityState.Modified;
                db.SaveChanges(); 
            }
            return RedirectToAction("Index","Report");
        }

        private void AddNewReport(int mId)
        {
            History newHistory = new History();
            newHistory.MappingId = mId;
            newHistory.Status = "Success";
            newHistory.RanBy = "Employee 1";
            newHistory.Date = DateTime.Now;
            db.Histories.Add(newHistory);
            db.SaveChanges();
        }

        private IEnumerable<History> GetOldHistory(int mId)
        {
            return from m in db.Histories
                       where m.MappingId == mId
                       select m;
            
        }

    }
}