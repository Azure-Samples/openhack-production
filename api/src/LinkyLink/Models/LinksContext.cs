﻿using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Diagnostics.CodeAnalysis;

namespace LinkyLink.Models
{
    /// <summary>
    /// This class coordinates Entity Framework Core functionality (Create, Read, Update, Delete) for the LinkBundle data model.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class LinksContext : DbContext
    {
        private readonly string cosmosContainerName;
        public LinksContext(DbContextOptions<LinksContext> options, IConfiguration configuration) : base(options)
        {
            cosmosContainerName = configuration.GetSection("CosmosSettings")["ContainerName"];
        }

        public virtual DbSet<LinkBundle> LinkBundle { get; set; }

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
