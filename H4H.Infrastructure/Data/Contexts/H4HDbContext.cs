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
            modelBuilder.Entity<Item>()
                .Property(i => i.CreatedByType)
                .HasConversion<string>();

            // Define relationships
            modelBuilder.Entity<User>()
                .HasMany(u => u.Addresses)
                .WithOne()
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Volunteer>()
                .HasMany(v => v.Addresses)
                .WithOne()
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Organization>()
                .HasMany(o => o.Addresses)
                .WithOne()
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Item>()
                .HasOne(i => i.Address)
                .WithMany()
                .HasForeignKey(i => i.AddressId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Item>()
                .HasOne(i => i.Order)
                .WithMany(o => o.Items)
                .HasForeignKey(i => i.OrderId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Order>()
                .HasOne(o => o.User)
                .WithMany(u => u.Orders)
                .HasForeignKey(o => o.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            //// Many-to-Many relationships for Orders, Volunteers, and Organizations
            //modelBuilder.Entity<Order>()
            //    .HasMany(o => o.Volunteers)
            //    .WithMany(v => v.AssignedItems);

            //modelBuilder.Entity<Order>()
            //    .HasMany(o => o.Organizations)
            //    .WithMany(o => o.Items);

            // Additional configuration as needed
        }
    }
}
