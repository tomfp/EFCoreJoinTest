using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Reflection;

namespace EFCoreJoinTest
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello Case study");

            var context = GetInMemoryDataContext();

            PopuplateContext(context);
            var caseStudyId = context.CaseStudy.First().Id;
            // queries - 1) as lambda

            var extRecords = QueryLambda(context, caseStudyId);
            PrintQueryResults(extRecords, "Lambda Query");

            var extRecs2 = QueryFromSyntax(context, caseStudyId);
            PrintQueryResults(extRecs2, "From Syntax");

            // Relational methods require an Sql Server database 

            context.Dispose();

            DestroyDatabase();
            var context2 = GetLocalDbDataContext();
            CreateDatabase(context2);
            PopuplateContext(context2);

            AddRecordsBySql(context2, caseStudyId);

            var extRecs3 = QueryLambda(context2, caseStudyId);
            PrintQueryResults(extRecs3, "Post SQL ADD Lambda");

            var extRecs4 = QueryFromSyntax(context2, caseStudyId);
            PrintQueryResults(extRecs4, "Post SQL ADD From Syntax");

            context2.Dispose();
            Console.ReadLine();

        }

        private static string dbFilename => "CaseStudyDb";

        static void PrintQueryResults(IEnumerable<V1Extended> extRecords, string resultTitle)
        {
            Console.WriteLine($"** {resultTitle} ***");
            foreach (var v1Extended in extRecords)
            {
                Console.WriteLine($"Ext Record: {v1Extended.ExtendedData}");
            }
        }

        static IEnumerable<V1Extended> QueryLambda(CaseStudyContext context, int caseStudyId)
        {
            var extRecords = context.CommonData
                .Join(context.V1Extended,
                    n => n.Id,
                    v => v.CommonDataId,
                    (n, v) => new { n, v })
                .Where(x => x.n.CaseStudy.Id == caseStudyId)
                .Select(y => y.v).ToList();
            return extRecords;
        }

        static IEnumerable<V1Extended> QueryFromSyntax(CaseStudyContext context, int caseStudyId)
        {
            var extRecords = from n in context.CommonData
                             join v in context.V1Extended
                                 on n.Id equals v.CommonDataId
                             select v;
            return extRecords;
        }


        static void PopuplateContext(CaseStudyContext context)
        {
            context.CaseStudy.Add(new CaseStudy { CaseStudyName = "Case Study" });
            context.SaveChanges();
            var caseStudy = context.CaseStudy.First();

            context.CommonData.Add(new CommonData { CaseStudy = caseStudy, CommonText = "First Common Data" });
            context.CommonData.Add(new CommonData { CaseStudy = caseStudy, CommonText = "Second Common Data" });
            context.SaveChanges();
            var commonData1 = context.CommonData.First(x => x.CommonText == "First Common Data");
            var commonData2 = context.CommonData.First(x => x.CommonText == "Second Common Data");

            context.V1Extended.Add(new V1Extended { CommonData = commonData1, ExtendedData = "First Extended Data" });
            context.V1Extended.Add(new V1Extended { CommonData = commonData2, ExtendedData = "Second Extended Data" });
            context.SaveChanges();
        }

        static void AddRecordsBySql(CaseStudyContext context, int caseStudyId)
        {
            var sql = $@"INSERT INTO CommonData (CaseStudyId, CommonText)
                SELECT {caseStudyId}, 'THIRD BY SQL' UNION ALL
                SELECT {caseStudyId}, 'FOURTH BY SQL';";

            var param = new Object[0];
            context.Database.ExecuteSqlCommand(sql, param);

            // get the new Ids
            var commonData3Id = context.CommonData.First(x => x.CommonText == "THIRD BY SQL").Id;
            var commonData4Id = context.CommonData.First(y => y.CommonText == "FOURTH BY SQL").Id;

            var sql2 = $@"INSERT INTO V1Extended (CommonDataId, ExtendedData)
                SELECT {commonData3Id}, 'THIRD EXTENDED' UNION ALL
                SELECT {commonData4Id}, 'FOURTH EXTENDED';";

            context.Database.ExecuteSqlCommand(sql2, param);
        }

        static CaseStudyContext GetLocalDbDataContext()
        {
            var context = new CaseStudyContext(new DbContextOptionsBuilder<CaseStudyContext>()
                .UseSqlServer(LocalDb.ConnectionString)
                .Options);
            return context;
        }

        static CaseStudyContext GetInMemoryDataContext()
        {
            var options = new DbContextOptionsBuilder<CaseStudyContext>()
                .UseInMemoryDatabase(databaseName: dbFilename)
                .Options;
            return new CaseStudyContext(options);
        }

        private static SqlConnectionStringBuilder LocalDb =>
            new SqlConnectionStringBuilder
            {
                DataSource = @"(LocalDB)\MSSQLLocalDB",
                InitialCatalog = dbFilename,
                IntegratedSecurity = true
            };

        private static SqlConnectionStringBuilder Master =>
            new SqlConnectionStringBuilder
            {
                DataSource = @"(LocalDB)\MSSQLLocalDB",
                InitialCatalog = "master",
                IntegratedSecurity = true
            };
        private static string Filename => Path.Combine(
            Path.GetDirectoryName(
                Assembly.GetExecutingAssembly().Location),
            dbFilename + ".mdf");


        private static void CreateDatabase(CaseStudyContext caseStudyContext)
        {
            // default language British 
            ExecuteSqlCommand(Master,
                $@"CREATE DATABASE [{dbFilename}]
                   ON ( NAME = '{dbFilename}',
                   FILENAME = '{Filename}' )");
            var context = caseStudyContext;
            context.Database.Migrate();
        }
        private static void DestroyDatabase()
        {
            var fileNames = ExecuteSqlQuery(Master,
                $@"SELECT [physical_name] FROM [sys].[master_files]
                    WHERE [database_id] = DB_ID('{dbFilename}')",
                row => (string)row["physical_name"]);
            if (fileNames.Any())
            {
                ExecuteSqlCommand(Master, $@"
                ALTER DATABASE [{dbFilename}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE; 
                EXEC sp_detach_db '{dbFilename}'");

                fileNames.ForEach(File.Delete);
            }
        }

        private static void ExecuteSqlCommand(
            SqlConnectionStringBuilder connectionStringBuilder,
            string commandText)
        {
            using (var connection = new SqlConnection(connectionStringBuilder.ConnectionString))
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = commandText;
                    command.ExecuteNonQuery();
                }
            }
        }

        private static List<T> ExecuteSqlQuery<T>(
            SqlConnectionStringBuilder conectionStringBuilder,
            string queryText,
            Func<SqlDataReader, T> read)
        {
            var result = new List<T>();
            using (var connection = new SqlConnection(conectionStringBuilder.ConnectionString))
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = queryText;
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            result.Add(read(reader));
                        }
                    }
                }
            }
            return result;
        }
    }
}

