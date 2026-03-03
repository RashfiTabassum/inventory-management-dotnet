using InventoryApp.Data.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace InventoryApp.Data.Context
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Inventory> Inventories { get; set; }
        public DbSet<Item> Items { get; set; }
        public DbSet<Tag> Tags { get; set; }
        public DbSet<CustomField> CustomFields { get; set; }
        public DbSet<CustomFieldValue> CustomFieldValues { get; set; }
        public DbSet<InventoryAccess> InventoryAccesses { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<Like> Likes { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Fix multiple cascade paths
            modelBuilder.Entity<Comment>()
                .HasOne(c => c.Author) // a comment has one author
                .WithMany() // an author can have many comments, but we don't need to define the navigation property on the ApplicationUser side
                .HasForeignKey(c => c.AuthorId) // the foreign key in the Comment table that references the ApplicationUser
                .OnDelete(DeleteBehavior.Restrict); // prevent cascade delete to avoid multiple cascade paths. This means that if a user is deleted, their comments will not be automatically deleted, and the foreign key will not be set to null. You may want to handle this manually in your application logic (e.g., by reassigning comments to a "deleted user" account or by deleting comments when a user is deleted).

            modelBuilder.Entity<Inventory>()
                .HasOne(i => i.Owner)
                .WithMany()
                .HasForeignKey(i => i.OwnerId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<InventoryAccess>()
                .HasOne(a => a.User)
                .WithMany()
                .HasForeignKey(a => a.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Item>()
                .HasOne(i => i.CreatedBy)
                .WithMany()
                .HasForeignKey(i => i.CreatedById)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Like>()
                .HasOne(l => l.User)
                .WithMany()
                .HasForeignKey(l => l.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // Composite unique indexes
            modelBuilder.Entity<Item>()
                .HasIndex(i => new { i.InventoryId, i.CustomId })
                .IsUnique();

            modelBuilder.Entity<Like>()
                .HasIndex(l => new { l.ItemId, l.UserId })
                .IsUnique();

            modelBuilder.Entity<InventoryAccess>()
                .HasIndex(a => new { a.InventoryId, a.UserId })
                .IsUnique();

            modelBuilder.Entity<CustomFieldValue>()
                .HasOne(v => v.Item)
                .WithMany(i => i.CustomFieldValues)
                .HasForeignKey(v => v.ItemId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Like>()
                .HasOne(l => l.Item)
                .WithMany(i => i.Likes)
                .HasForeignKey(l => l.ItemId)
                .OnDelete(DeleteBehavior.Restrict);
        }
        
    }
}