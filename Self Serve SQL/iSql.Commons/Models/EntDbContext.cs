using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Web;
using System.Data.Entity;

namespace iSql.Commons.Models {
    public class EntDbContext : DbContext {

        public DbSet<User> Users { get; set; }

        public DbSet<Project> Projects { get; set; }

        public DbSet<Access> Accesses { get; set; }

        public DbSet<Ticket> Tickets { get; set; }

        public DbSet<WorkState> Workstates { get; set; }

        public DbSet<Comment> Comments { get; set; }

        public DbSet<UserAccessRequest> UserAccessRequest { get; set; }

        //NOTE: after prototype stage, it is soemtimes annoying to generate the db from code-first approach every time, we can disable it here.
        protected override void OnModelCreating(DbModelBuilder modelBuilder) {
            modelBuilder.Conventions.Remove<IncludeMetadataConvention>();
            base.OnModelCreating(modelBuilder);
        }
    }
}