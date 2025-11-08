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

    }
}
