using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Entity;
using ExelTask.Models;

namespace ExelTask.DAL
{
    public class TaskContext :DbContext
    {
        public TaskContext()
            : base("TaskContext")
        {

        }
        public virtual DbSet<Cell> Cells { get; set; }
        public virtual DbSet<Mapping> Mappings { get; set; }
        public virtual DbSet<History> Histories { get; set; }
        public virtual DbSet<Configuration> Configurations { get; set; }
        public virtual DbSet<OutputFile> OutputFiles { get; set; }
        public virtual DbSet<Workbook> Workbooks { get; set; }
        public virtual DbSet<Worksheet> Worksheets { get; set; }
    }
}