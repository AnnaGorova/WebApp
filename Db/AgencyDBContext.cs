using Microsoft.EntityFrameworkCore;
using WebApp.Entities;

namespace WebApp.Db
{
    public class AgencyDBContext : DbContext
    {
        public AgencyDBContext(DbContextOptions<AgencyDBContext> options)
            : base(options)
        {
            //Database.EnsureDeleted();
            Database.EnsureCreated();
        }

        public DbSet<Option> Options { get; set; }
        public DbSet<Navigate> Navigates { get; set; }
        public DbSet<ClientMessage> ClientMessages { get; set; }

        public DbSet<FooterQuickLinks> FooterQuickLinks { get; set; }
        public DbSet<FooterLink> FooterLinks { get; set; }
        public DbSet<Tag> Tags { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Post> Posts { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<PostTags> PostTags { get; set; }
        public DbSet<PostCategories> PostCategories { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Конфігурація для PostTags (багато-до-багатьох)
            modelBuilder.Entity<PostTags>()
                .HasKey(pt => new { pt.PostId, pt.TagId }); // Складений ключ

            modelBuilder.Entity<PostTags>()
                .HasOne(pt => pt.Post)           // PostTags має одного Post
                .WithMany(p => p.PostTags)       // Post має багато PostTags  
                .HasForeignKey(pt => pt.PostId); // Зовнішній ключ - PostId

            modelBuilder.Entity<PostTags>()
                .HasOne(pt => pt.Tag)
                .WithMany(t => t.PostTags)
                .HasForeignKey(pt => pt.TagId);

            // Конфігурація для PostCategories (багато-до-багатьох)
            modelBuilder.Entity<PostCategories>()
                .HasKey(pc => new { pc.PostId, pc.CategoryId }); // Складений ключ

            modelBuilder.Entity<PostCategories>()
                .HasOne(pc => pc.Post)
                .WithMany(p => p.PostCategories)
                .HasForeignKey(pc => pc.PostId);

            modelBuilder.Entity<PostCategories>()
                .HasOne(pc => pc.Category)
                .WithMany(c => c.PostCategories)
                .HasForeignKey(pc => pc.CategoryId);
        }
    }
}
