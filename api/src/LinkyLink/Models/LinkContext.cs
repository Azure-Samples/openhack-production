using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.EntityFrameworkCore.ValueGeneration;
using Microsoft.EntityFrameworkCore.ValueGeneration.Internal;
using System;

namespace LinkyLink.Models
{
    public class linkbundles1 : DbContext
    {
        public linkbundles1(DbContextOptions<linkbundles1> options)
            : base(options) { }

        public DbSet<LinkBundle> LinkBundle { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<LinkBundle>().Property(p => p.Id).ValueGeneratedOnAdd();
            modelBuilder.Entity<LinkBundle>().OwnsMany<Link>(p => p.Links);
            modelBuilder.Entity<LinkBundle>().HasPartitionKey(o => o.VanityUrl);
            modelBuilder.Entity<LinkBundle>().HasNoDiscriminator();
        }
    }
}
