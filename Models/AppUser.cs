using Microsoft.AspNetCore.Identity;

namespace RSS_Sub_App.Models
{
    public class AppUser : IdentityUser
    {             
        public ICollection<RssFeed>? Feeds { get; set; }
        public ICollection<RssItem>? Items { get; set; }
    }
}
