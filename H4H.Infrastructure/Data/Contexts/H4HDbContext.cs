using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using H4H.Domain.Entities;

namespace H4H.Infrastructure.Data.Contexts
{
    public class H4HDbContext : DbContext
    {
        public H4HDbContext(DbContextOptions<H4HDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Volunteer> Volunteers { get; set; }
        public DbSet<Organization> Organizations { get; set; }
        public DbSet<Item> Items { get; set; }
        public DbSet<Address> Addresses { get; set; }
        public DbSet<Order> Orders { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure polymorphic relationship for Item creation (User, Volunteer, Organization)
            //modelBuilder.Entity<Item>()
            //    .Property(i => i.CreatedByType)
            //    .HasConversion<string>();

            // Define relationships
            modelBuilder.Entity<User>()
                .HasMany(u => u.Addresses)
                .WithOne(a => a.User)
                .HasForeignKey(a => a.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            //modelBuilder.Entity<Volunteer>()
            //    .HasMany(v => v.Addresses)
            //    .WithOne(a => a.Volunteer)
            //    .HasForeignKey(a => a.VolunteerId)
            //    .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Organization>()
                .HasMany(o => o.Addresses)
                .WithOne(a => a.Organization)
                .HasForeignKey(a => a.OrganizationId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Item>()
                .HasMany(i => i.Addresses)
                .WithOne(a => a.Item)
                .HasForeignKey(i => i.AddressId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Item>()
                .HasOne(i => i.Order)
                .WithMany(o => o.Items)
                .HasForeignKey(i => i.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Order>()
                .HasOne(o => o.User)
                .WithMany(u => u.Orders)
                .HasForeignKey(o => o.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Many-to-Many relationships for Orders, Volunteers, and Organizations
            modelBuilder.Entity<Order>()
                .HasMany(o => o.Items)
                .WithOne(i => i.Order)
                .HasForeignKey(i => i.OrderId);

            //modelBuilder.Entity<Order>()
            //    .HasMany(o => o.Organizations)
            //    .WithMany(o => o.Orders);

            // Additional configuration as needed
        }
    }
}
