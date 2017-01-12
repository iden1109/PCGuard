using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PCGuardDB.DBModel
{
    public class PCGuardDBContext : DbContext
    {
        public PCGuardDBContext() : base("name=pc_guard")
        {   
        }

        public virtual DbSet<history> history { get; set; }
        

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema("pc_guard");
            base.OnModelCreating(modelBuilder);
        }
    }
}
