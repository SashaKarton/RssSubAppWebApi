using Microsoft.EntityFrameworkCore;
using RSS_Sub_App.Data;
using RSS_Sub_App.Models;
using RSS_Subscription_Application.Interfaces;

namespace RSS_Subscription_Application.Repository
{
    public class FeedRepository : IFeedRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;
        public FeedRepository(ApplicationDbContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<IEnumerable<RssFeed>> GetAllFeedsAsync()
        {
            var feeds = await _context.RssFeeds.ToListAsync();
            return feeds;
        }

        public async Task<IEnumerable<RssItem>> GetAllNewItemsAsync(DateTime date)
        {
            var curUser = _httpContextAccessor.HttpContext?.User.GetUserId();
            var users = await _context.AppUsers.Where(u => u.Id == curUser).FirstOrDefaultAsync();
            var userItems = await _context.RssItems.Where(u => u.AppUsers.Contains(users)).Where(d => d.PublishDate >= date).ToListAsync();                        
            return userItems;
        }

        public async Task<RssFeed> GetFeedByLinkAsync(string link)
        {
            var feed = await _context.RssFeeds.Where(l => l.Link == link).FirstOrDefaultAsync();
            return feed;
        }
        
        public async Task<IEnumerable<AppUser>> GetUsersByFeedAsync(RssFeed feed)
        {
            var users = await _context.AppUsers.Where(f => f.Feeds == feed).ToListAsync();                     
            return users;
        }
        public async Task<AppUser> GetUserWithFeedAsync()
        {
            var curUser = _httpContextAccessor.HttpContext?.User.GetUserId();
            var appUser = await _context.AppUsers.Where(x => x.Id == curUser).Include(f => f.Feeds).FirstOrDefaultAsync();
            return appUser;
        }

        public async Task<AppUser> GetUserAsync()
        {
            var curUser = _httpContextAccessor.HttpContext?.User.GetUserId();
            var appUser = await _context.AppUsers.Where(x => x.Id == curUser).FirstOrDefaultAsync();
            return appUser;
        }
        public (int id, bool success) AddAndGetId(RssFeed feed)
        {
            _context.Add(feed);

            bool success = Save();
            int id = feed.Id;
            return (id, success);        }

        public async Task<IEnumerable<RssFeed>> GetFreshFeeds(List<int> ids)
        {
            var oneWeek = TimeSpan.FromDays(7);

            return await _context.RssFeeds.Where(t => t.LastUpdate >= DateTime.Now.Date.Subtract(oneWeek)).Where(i => ids.Contains(i.Id)).ToListAsync();

        }
        public bool UpdateItem(RssItem news)
        {
            _context.Update(news);            
            return Save();
        }

        public bool AddItem(RssItem feedItem)
        {
            _context.Add(feedItem);
            return Save();
        }

        public bool Save()
        {
            var saved = _context.SaveChanges();
            return saved > 0;
        }

    }
}
