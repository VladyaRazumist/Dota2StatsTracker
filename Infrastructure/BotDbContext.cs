using System;
using System.Collections.Generic;
using Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure
{
    public class BotDbContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<DotaAccount> DotaAccounts { get; set; }
        public DbSet<DotaMatch> DotaMatches { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
            => options.UseSqlServer("data source=.\\MSSQLSERVER02;Initial Catalog=PudgeBot;Integrated Security=True;");

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>()
                .HasIndex(u => u.DiscordUserId)
                .IsUnique();
        }
    }
}
