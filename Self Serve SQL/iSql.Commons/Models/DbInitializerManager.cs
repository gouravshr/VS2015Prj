using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Entity;
using System.Configuration;

namespace iSql.Commons.Models
{
    public class DbInitializerManager  {
        //CreateDatabaseOnlyIfNotExists
        public static void SetInitializer( string policy = null )  {
            if (policy == null) {
                policy = ConfigurationManager.AppSettings["seed-data-policy"];
            }

            switch (policy) {
                case "always":
                    Database.SetInitializer<EntDbContext>(new AlwaysRecreateDb());
                    break; 
                case "non-exist":
                    Database.SetInitializer<EntDbContext>(new CreateNonExistDb() ); 
                    break; 
                case "modified":
                    Database.SetInitializer<EntDbContext>(new CreateIfModified() );
                    break;
                default:
                    // no need to initialize  (for older EF)
                    //NOTE: after upgrade to EF5.0, we seems have to init again
                    Database.SetInitializer<EntDbContext>(null);
                    break;
            }
        }

        public static void ApplySeedData( EntDbContext context) { 
            // for now, just populate unit testing data 
            PopulateUnitTestingData(context);
        }

        public static void PopulateUnitTestingData( EntDbContext context) {

            var users = new List<User>() { 
                                             new User { UserId = "g.shrivastava" , SystemRole="Admin" } , 
                                             new User { UserId = "raghavan.p.kumar", SystemRole="DBA" }, 
                                         };

            users.ForEach(u => context.Users.Add(u));

            // change testing project name to avoid future conflict.
            var projects = new List<Project>() {
                                                   new Project { Name = "Test 001", Description="Actual sql query plan analysis tool.", ContactInfo="Owner: Albrecht, Scott A. and Davidson, Josh" , IsActive=true, StagingHost = ".", StagingPort = 1433, StagingDatabase = "Northwind_Stage", ProductionHost = ".", ProductionPort = 1433, ProductionDatabase = "Northwind"},
                                                   new Project { Name = "Test 002", Description="Global Time & Expense managemnt applicaiton", ContactInfo="Owner: Domsky, Oron H." , IsActive=true , StagingHost = ".", StagingPort = 1433, StagingDatabase = "Northwind_Stage", ProductionHost = ".", ProductionPort = 1433, ProductionDatabase = "Northwind"},
                                                   new Project { Name = "Test 003", Description="MME finanical manangement project", ContactInfo="Owner: Klee, Elizabeth C. ; Main Contact: Rehal, Ryan M." , IsActive=true , StagingHost = ".", StagingPort = 1433, StagingDatabase = "Northwind_Stage", ProductionHost = ".", ProductionPort = 1433, ProductionDatabase = "Northwind"},
                                                   new Project { Name = "Test 004", Description="MyPerformance HR management tool.", ContactInfo="Owner: Vinkler, Jeffrey T." , IsActive=true , StagingHost = ".", StagingPort = 1433, StagingDatabase = "Northwind_Stage", ProductionHost = ".", ProductionPort = 1433, ProductionDatabase = "Northwind"},
                                                   new Project { Name = "Test 005", Description="Opportunity Manangement project.", ContactInfo="Owner: Chapman, Paul S." , IsActive=true , StagingHost = ".", StagingPort = 1433, StagingDatabase = "Northwind_Stage", ProductionHost = ".", ProductionPort = 1433, ProductionDatabase = "Northwind"}, 
                                               };

            projects.ForEach( p => context.Projects.Add(p));

            var accesses = new List<Access>() {
                                                  new Access { UserId = "g.shrivastava", ProjectId = 1  , Role = "Approver"}, 
                                                  new Access { UserId = "g.shrivastava", ProjectId = 2  , Role = "Approver"}, 
                                                  new Access { UserId = "g.shrivastava", ProjectId = 3  , Role = "Approver"}, 
                                                  new Access { UserId = "g.shrivastava", ProjectId = 4  , Role = "Requester"}, 
                                                  new Access { UserId = "g.shrivastava", ProjectId = 5  , Role = "Requester"}, 

                                                  new Access { UserId = "raghavan.p.kumar", ProjectId = 1, Role = "Requester"}, 
                                                  new Access { UserId = "raghavan.p.kumar", ProjectId = 2, Role = "Requester"}, 
                                                  new Access { UserId = "raghavan.p.kumar", ProjectId = 3, Role = "Requester"}, 
                                                  new Access { UserId = "raghavan.p.kumar", ProjectId = 4, Role = "Approver"}, 
                                                  new Access { UserId = "raghavan.p.kumar", ProjectId = 5, Role = "Approver"}, 
                                              };

            //NOTE: for some reason, EF will populate Access first, which should be a bug and will generate foreign key violation exception, now just comment it out, need further investigation. 
//            accesses.ForEach( a => context.Accesses.Add(a));

            // no need to create tickets now... the workflow has evovled and it is really hard to mimic the uploaded file, etc, without quite some efforts
            /*
            var tickets = new List<Ticket>() {
                                                 new Ticket { UserId="josh.davidson", Description="Testing script, add one column to  tbl_people table.", ProjectId=1, CreatedAt= DateTime.Now } , 
                                                 new Ticket { UserId="josh.davidson", Description="Testing script, index defragmentation.", ProjectId=2, CreatedAt= DateTime.Now } , 
                                                 new Ticket { UserId="josh.davidson", Description="Testing script, create stored procedure.", ProjectId=2, CreatedAt= DateTime.Now } , 
                                                 new Ticket { UserId="eugene.khazin", Description="Testing script, clean statistics.", ProjectId=1, CreatedAt= DateTime.Now } , 
                                             };

            tickets.ForEach(t => context.Tickets.Add(t)); 
            */
        }

        // ------------------------------------------------------------------------
        // At different dev stage, I need different data initializers, when go with 
        // EF code-first approach. Those dynamically generate tables will enventually
        // being refined or discarded, just for faster prototype anyway. 
        // ------------------------------------------------------------------------

        private class CreateNonExistDb : CreateDatabaseIfNotExists<EntDbContext>{ 
            protected override void  Seed(EntDbContext context) { 
                DbInitializerManager.ApplySeedData( context ); 
            }
        }

        private class AlwaysRecreateDb : DropCreateDatabaseAlways<EntDbContext> { 
            protected override void  Seed(EntDbContext context) { 
                DbInitializerManager.ApplySeedData( context ); 
            }
        }

        private class CreateIfModified : DropCreateDatabaseIfModelChanges<EntDbContext>{ 
            protected override void  Seed(EntDbContext context) { 
                DbInitializerManager.ApplySeedData( context ); 
            }
        }        
    }
}