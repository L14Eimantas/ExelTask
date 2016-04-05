using ExelTask.DAL;
using ExelTask.Models;
using Ionic.Zip;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ExelTask.Controllers
{
    //its work in the future a litle bit change
    public class ZipController : Controller
    {
        // GET: Zip
        private TaskContext db = new TaskContext();
        public void InputFiles(int mId)
        {
            Response.Clear();
            Response.BufferOutput = false;
            Response.ContentType = "application/zip";
            Response.AddHeader("content-disposition", "attachment; filename=InputFiles.zip");
            using(ZipFile zip = new ZipFile() )
            {
                AddInputFiles(zip, mId);
                zip.Save(Response.OutputStream);
            }
       
        }
        public void OutputFiles(int mId)
        {
            Response.Clear();
            Response.BufferOutput = false;
            Response.ContentType = "application/zip";
            Response.AddHeader("content-disposition", "attachment; filename=OutputFiles.zip");
            using (ZipFile zip = new ZipFile())
            {
                AddOutputFiles(zip, mId);
                zip.Save(Response.OutputStream);
            }
            
        }
        private void AddInputFiles(ZipFile zip, int mId)
        {
            var Files = GetInputFiles(mId);
            int i = 1;
            foreach (var a in Files)
            {
                try
                {
                    zip.AddEntry(a.WorkbookName, a.WorkbookFile);
                }
                catch
                {
                    string[] names = a.WorkbookName.Split('.');
                    string fileName = names[0] + "(" + i.ToString() + ")." + names[1];
                    zip.AddEntry(fileName, a.WorkbookFile);
                    i++;
                }

            }
        }

        private void AddOutputFiles(ZipFile zip, int mId)
        {
            var Files = GetOutputFiles(mId);
            int i = 1;
            foreach (var a in Files)
            {
                try
                {
                    zip.AddEntry(a.FileName, a.FileOutput);
                }
                catch
                {
                    string[] names = a.FileName.Split('.');
                    string fileName = names[0] + "(" + i.ToString() + ")." + names[1];
                    zip.AddEntry(fileName, a.FileOutput);
                    i++;
                }
            }
        }



        private IEnumerable<Workbook> GetInputFiles(int mId)
        {
            return from m in db.Workbooks
                   where (m.MappingId == mId && m.IsInputOrOutput ==true)
                   select m;
        }


        private IEnumerable<OutputFile> GetOutputFiles(int mId)
        {
            return from m in db.OutputFiles
                   where (m.MappingId == mId)
                   select m;
        }
    }
}