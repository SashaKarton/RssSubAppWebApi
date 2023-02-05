using RSS_Sub_App.Models;


namespace RSS_Subscription_Application.Interfaces
{
    public interface IFeedRepository
    {
        (int id, bool success) AddAndGetId(RssFeed feed);
        bool AddItem(RssItem feedItem);
        Task<AppUser> GetUserWithFeedAsync();
        Task<AppUser> GetUserAsync();
        Task<IEnumerable<RssFeed>> GetAllFeedsAsync();
        Task<IEnumerable<RssItem>> GetAllNewItemsAsync(DateTime date);
        Task<RssFeed> GetFeedByLinkAsync(string link);
        Task<IEnumerable<AppUser>> GetUsersByFeedAsync(RssFeed feed);        
        Task<IEnumerable<RssFeed>> GetFreshFeeds(List<int> ids);
        bool UpdateItem(RssItem news);
        bool Save();

    }
}
