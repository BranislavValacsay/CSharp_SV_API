﻿using Microsoft.EntityFrameworkCore;
using sp_api.Models;

namespace sp_api.Data
{
    public class API_DbContext : DbContext
    {
        public API_DbContext(DbContextOptions<API_DbContext> options)
            : base(options)
        {
        }

        public DbSet<RequestServer> RequestServers { get; set; }
        public DbSet<VMMNetwork> VMMNetworks { get; set; }
        public DbSet<AdDomain> AdDomains { get; set; }
        public DbSet<AdminList> AdminList { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<RequestServer>().ToTable("RequestServer");
            modelBuilder.Entity<VMMNetwork>().ToTable("VMMNetwork");
            modelBuilder.Entity<AdDomain>().ToTable("AdDomain");
            modelBuilder.Entity<AdminList>().ToTable("AdminList");

            base.OnModelCreating(modelBuilder);
        }
    }
}
