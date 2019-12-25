using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.EntityFrameworkCore.ValueGeneration;
using Microsoft.EntityFrameworkCore.ValueGeneration.Internal;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using System;
using System.Configuration;

namespace LinkyLink.Models
{
    public class LinksContext : DbContext
    {
        private string cosmosContainerName;
        public LinksContext(DbContextOptions<LinksContext> options, IConfiguration configuration) : base(options)
        {
            cosmosContainerName = configuration.GetSection("ContainerName").ToString();
        }

        public DbSet<LinkBundle> LinkBundle { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<LinkBundle>().Property(p => p.Id).ValueGeneratedOnAdd();
            modelBuilder.Entity<LinkBundle>().OwnsMany<Link>(p => p.Links);
            modelBuilder.Entity<LinkBundle>().HasPartitionKey(o => o.UserId);
            modelBuilder.HasDefaultContainer(cosmosContainerName);
            modelBuilder.Entity<LinkBundle>().HasNoDiscriminator();
        }
    }
}
