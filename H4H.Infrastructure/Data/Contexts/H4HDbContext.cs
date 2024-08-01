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

            // Configure polymorphic relationship for Item creation (User, Volunteer, Organization)
            //modelBuilder.Entity<Item>()
            //    .Property(i => i.CreatedByType)
            //    .HasConversion<string>();

            // Define relationships
            // Configure the TPH inheritance
            //modelBuilder.Entity<User>()
            //    .HasDiscriminator<string>("user_type")
            //    .HasValue<User>("user_base")
            //    .HasValue<Volunteer>("user_volunteer");

            //modelBuilder.Entity<User>()
            //    .Property("user_type")
            //    .HasMaxLength(200);

            //modelBuilder.Entity<Volunteer>()
            //    .Property(v => v.VolunteerId)
            //    .IsRequired();

            modelBuilder.Entity<User>()
                .HasMany(u => u.Addresses)
                .WithOne(a => a.User)
                .HasForeignKey(a => a.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Organization>()
                .HasMany(o => o.Addresses)
                .WithOne(a => a.Organization)
                .HasForeignKey(a => a.OrganizationId)
                .OnDelete(DeleteBehavior.Cascade);

            //modelBuilder.Entity<Organization>()
            //     .HasMany(i => i.Items)
            //    .WithOne(a => a.Organization)
            //    .HasForeignKey(a => a.OrganizationId)
            //    .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Item>()
                .HasMany(i => i.Addresses)
                .WithOne(a => a.Item)
                .HasForeignKey(i => i.ItemId)
                .OnDelete(DeleteBehavior.Cascade);

            //modelBuilder.Entity<User>()
            //    .HasMany(u => u.Orders)
            //    .WithOne(o => o.User)
            //    .HasForeignKey(o => o.UserId)
            //    .OnDelete(DeleteBehavior.);

           // modelBuilder.Entity<User>()
           //.HasMany(u => u.Items)
           //.WithMany(i => i.Users);

            modelBuilder.Entity<Order>()
                .HasMany(o => o.Items)
                .WithOne(i => i.Order)
                .HasForeignKey(i => i.OrderId);

            // Define many-to-many relationship between Volunteer and Order
            //modelBuilder.Entity<Volunteer>()
            //    .HasMany(v => v.Orders)
            //    .WithMany(o => o.Volunteers);
        }
    }
}