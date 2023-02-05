using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using RSS_Sub_App.Models;


namespace RSS_Sub_App.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public DbSet<RssFeed> RssFeeds { get; set; }
        public DbSet<RssItem> RssItems { get; set; }
        public DbSet<AppUser> AppUsers { get; set; }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        
    }
}
