using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ExelTask.Models;
using ExelTask.DAL;
using ExelTask.MyClass;

namespace ExelTask.Controllers
{
    public class RunController : Controller
    {
        // GET: Run
        private TaskContext db = new TaskContext();
        public ActionResult Index(int mId)
        {
            var Config = GetRequiredConfig(mId);
            foreach (var config in Config)
            {
                FillOutputFile Output = new FillOutputFile(config, mId);
                Output.GetOutputFile();

            }
            DeleteSameDataFormDB(mId);
            return RedirectToAction("ChechHistory", "Report", new { mId = mId });
        }

        private void DeleteSameDataFormDB(int mId)
        {
            var Workbook = GetWorkbookTemplate(mId);
            foreach (var a in Workbook)
            {
                int requiredOutputId = GetRequiredOutputId(a.Id); // need that can find the same data (and can delete this data except the last one)
                if (requiredOutputId != 0)
                {
                    var reangeOfOutput = from m in db.OutputFiles
                                         where m.TemplateFileId == a.Id && m.Id < requiredOutputId
                                         select m;
                    db.OutputFiles.RemoveRange(reangeOfOutput);
                }
            }
            db.SaveChanges();
        }

        private int GetRequiredOutputId(int p) // p - workbookId
        {
            var Data = from m in db.OutputFiles
                     where m.TemplateFileId == p
                     orderby m.Id descending             
                     select m;
            if (Data.Any())
            {
                int required = Data.First().Id;
                return required;
            }
            else
                return 0;
        }

        private IEnumerable<Workbook> GetWorkbookTemplate(int mId)
        {
            return from m in db.Workbooks
                   where mId == m.MappingId && m.IsInputOrOutput == false
                   select m;
        }



        private IEnumerable<Configuration> GetRequiredConfig(int mId) //Get Configuration data by MappingID
        {
            return from m in db.Configurations
                   where m.MappingId == mId
                   select m;
        }
    }
}