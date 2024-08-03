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
            // Ignore Volunteers property in Order
          

            base.OnModelCreating(modelBuilder);
            //this.ChangeTracker.LazyLoadingEnabled = false;

   

            //modelBuilder.Entity<User>()
            //    .HasMany(u => u.Addresses)
            //    .WithOne(a => a.User)
            //    .HasForeignKey(a => a.UserId)
            //    .OnDelete(DeleteBehavior.SetNull);


            //modelBuilder.Entity<Organization>()
            //    .HasMany(o => o.Addresses)
            //    .WithOne(a => a.Organization)
            //    .HasForeignKey(a => a.OrganizationId)
            //    .OnDelete(DeleteBehavior.SetNull);



            //modelBuilder.Entity<Item>()
            //    .HasMany(i => i.Addresses)
            //    .WithOne(a => a.Item)
            //    .HasForeignKey(i => i.ItemId)
            //    .OnDelete(DeleteBehavior.SetNull);



            //modelBuilder.Entity<Order>()
            //    .HasMany(i => i.Items)
            //    .WithOne(o => o.Order)
            //    .HasForeignKey(i => i.OrderId);

            //modelBuilder.Entity<Order>()
            //    .HasMany(u => u.Users)
            //    .WithOne(o => o.Order)
            //    .HasForeignKey(u => u.UserId);

            //modelBuilder.Entity<Order>()
            //    .HasMany(a => a.Addresses)
            //    .WithOne(o => o.Order)
            //    .HasForeignKey(a => a.OrderId);

         
        }
    }
}