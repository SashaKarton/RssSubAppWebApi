using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RSS_Sub_App.Models
{
    public class RssItem
    {
        [Key]
        public int Id { get; set; }        
        public string Link { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTimeOffset PublishDate { get; set; }

        [ForeignKey("RssFeed")]
        public int RssFeedId { get; set; }
        public RssFeed Feed { get; set; }
        public ICollection<AppUser> AppUsers { get; set; }
        public bool ReadStatus { get; set; } = false;



    }
}
