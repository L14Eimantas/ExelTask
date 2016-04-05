using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using ExelTask.DAL;
using ExelTask.Models;
using PagedList;
using PagedList.Mvc; //this need do to page list

namespace ExelTask.Controllers
{
    public class MappingController : Controller
    {
        private TaskContext db = new TaskContext();
        private static string lastChoose = "";//lastchoose of pagesize
        // GET: Mapping
        public ActionResult Index(int? page,  string PageSize)
        {
            if (!MappingsExist())
                ResetAllTableID();
            if (String.IsNullOrEmpty(PageSize) && String.IsNullOrEmpty(lastChoose)) //if PageSize and lastChoose equil "", this means 
            {                                                              // that program  just start and default page size is 3 
                PageSize = "3"; //default pagesize is 3
                lastChoose = PageSize;
            }
            else if (!String.IsNullOrEmpty(PageSize)) //this means that user choose see new PageSize item per page
                lastChoose = PageSize; //and lastChoose save user request
            return View(db.Mappings.ToList().ToPagedList((page ?? 1),int.Parse(lastChoose)));
        }
        [NonAction]
        private void ResetAllTableID()
        {
            db.Database.ExecuteSqlCommand("DBCC CHECKIDENT('Mappings',RESEED,0);"); //all this needed to reset each other Id
            db.Database.ExecuteSqlCommand("DBCC CHECKIDENT('Workbooks',RESEED,0);");
            db.Database.ExecuteSqlCommand("DBCC CHECKIDENT('Worksheets',RESEED,0);");
            db.Database.ExecuteSqlCommand("DBCC CHECKIDENT('Cells',RESEED,0);");
            db.Database.ExecuteSqlCommand("DBCC CHECKIDENT('Configurations',RESEED,0);");
            db.Database.ExecuteSqlCommand("DBCC CHECKIDENT('OutputFiles',RESEED,0);");
            db.Database.ExecuteSqlCommand("DBCC CHECKIDENT('Histories',RESEED,0);");
            db.SaveChanges();
        }
        [NonAction]
        private bool MappingsExist()
        {
            if (db.Mappings.Any())
                return true;
            else return false;
        }

        // GET: Mapping/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Mapping/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,MappingName,ClieantName,ModuleName,Tags")] Mapping mapping) //uztenka ir Mapping mapping
        {
            if (ModelState.IsValid)
            {
                db.Mappings.Add(mapping);
                db.SaveChanges();
                return RedirectToAction("UploadIndex", "Upload", new { id = mapping.Id }); //i need send to controller (Upload) then i can save MappingId
            }
            return View(mapping);
        }

        // GET: Mapping/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Mapping mapping = db.Mappings.Find(id);
            // OR --- Mapping mapping = db.Mappings.Single(x => x.Id == id);
            if (mapping == null)
            {
                return HttpNotFound();
            }
            return View(mapping);
        }

        // POST: Mapping/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        // POST: Student/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,MappingName,ClieantName,ModuleName,Tags")] Mapping mapping)
        {
            if (ModelState.IsValid)
            {
                db.Entry(mapping).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("UploadIndex", "Upload", new {id= mapping.Id});
            }
            return View(mapping);
        }
        // GET: Mapping/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Mapping mapping = db.Mappings.Find(id);
            if (mapping == null)
            {
                return HttpNotFound();
            }
            return View(mapping);
        }

        // POST: Mapping/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Mapping mapping = db.Mappings.Find(id);
            db.Mappings.Remove(mapping);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
