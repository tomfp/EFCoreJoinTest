using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using Microsoft.Extensions.Configuration;

namespace EFCoreJoinTest
{
    class CaseStudy
    {
        public int Id { get; set; }
        public string CaseStudyName { get; set; }
    }

    class CommonData
    {
        public int Id { get; set; }
        public int CaseStudyId { get; set; }
        public CaseStudy CaseStudy { get; set; }
        public string CommonText { get; set; }
    }

    class V1Extended
    {
        public int Id { get; set; }
        public int CommonDataId { get; set; }
        public CommonData CommonData { get; set; }
        public string ExtendedData { get; set; }
    }

    class CaseStudyContext : DbContext
    {
        public DbSet<CaseStudy> CaseStudy { get; set; }
        public DbSet<CommonData> CommonData { get; set; }
        public DbSet<V1Extended> V1Extended { get; set; }

        public CaseStudyContext(DbContextOptions<CaseStudyContext> options)
            :base(options)
        { }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseInMemoryDatabase("CaseStudyDatabase");                    
            }
            base.OnConfiguring(optionsBuilder);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<CaseStudy>()
                .HasIndex(u => u.CaseStudyName)
                .IsUnique();
        }
    }


}
