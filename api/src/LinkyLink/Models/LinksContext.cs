using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.EntityFrameworkCore.ValueGeneration;
using Microsoft.EntityFrameworkCore.ValueGeneration.Internal;
using System;

namespace LinkyLink.Models
{
    public class LinksContext : DbContext
    {
        public LinksContext(DbContextOptions<LinksContext> options)
            : base(options) { }

        public DbSet<LinkBundle> LinkBundle { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<LinkBundle>().Property(p => p.Id).ValueGeneratedOnAdd();
            modelBuilder.Entity<LinkBundle>().OwnsMany<Link>(p => p.Links);
            modelBuilder.Entity<LinkBundle>().HasPartitionKey(o => o.UserId);
            modelBuilder.HasDefaultContainer("linkbundles");
            modelBuilder.Entity<LinkBundle>().HasNoDiscriminator();
        }
    }
}
