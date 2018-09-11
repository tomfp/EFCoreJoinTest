using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace EFCoreJoinTest
{
    class DesignTimeDbDataContextFactory : IDesignTimeDbContextFactory<CaseStudyContext>
    {
        private static string dbFilename = "CaseStudyDb";

        public CaseStudyContext CreateDbContext(string[] args)
        {
            var context = new CaseStudyContext(new DbContextOptionsBuilder<CaseStudyContext>()
                .UseSqlServer(LocalDb.ConnectionString)
                .Options);

            return context;
            //IConfigurationRoot configuration = new ConfigurationBuilder()
            //    .Build();
            //var builder = new DbContextOptionsBuilder<CaseStudyContext>();
            //var connectionString = configuration.GetConnectionString("FootprintCore");
            //builder.UseSqlServer(connectionString);
            //return new CaseStudyContext(builder.Options);
        }

        private static SqlConnectionStringBuilder LocalDb =>
            new SqlConnectionStringBuilder
            {
                DataSource = @"(LocalDB)\MSSQLLocalDB",
                InitialCatalog = dbFilename,
                IntegratedSecurity = true
            };

    }
}
