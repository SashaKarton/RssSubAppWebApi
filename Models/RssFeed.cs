using System.ComponentModel.DataAnnotations;

namespace RSS_Sub_App.Models
{
    public class RssFeed
    {
        [Key]
        public int Id { get; set; }
        public string FeedCheckId { get; set; }
        public string Link { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTimeOffset LastUpdate { get; set; }
        public ICollection<RssItem>? Items { get; set; }
        public ICollection<AppUser> AppUsers { get; set; }
        

    }
}
