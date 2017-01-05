using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomWebAPI.DAL
{
    /// <exclude />
    public class DBContext : DbContext
    {
        /// <exclude />
        public DBContext() : base("CustomWebApi")
        {

        }
        /// <exclude />
        public DbSet<Token> Token { get; set; }

        /// <exclude />
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }
        
        /// <exclude />
        public class AppDbInitializer : DropCreateDatabaseIfModelChanges<DBContext>
        {

        }
    }
}
