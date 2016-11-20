using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomWebAPI.DAL
{
    public class DBContext : DbContext
    {
        public DBContext() : base("CustomWebApi")
        {

        }
        public DbSet<Token> Token { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }

        public class AppDbInitializer : DropCreateDatabaseIfModelChanges<DBContext>
        {

        }
    }
}
