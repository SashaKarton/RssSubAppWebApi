using RSS_Sub_App.Models;
using RSS_Subscription_Application.Interfaces;
using System.ServiceModel.Syndication;
using System.Xml;

namespace RSS_Sub_App.Services
{
    class PeriodicHostedService : BackgroundService
    {
        private readonly ILogger<PeriodicHostedService> _logger;
        private readonly IServiceProvider _serviceProvider;        
        private readonly TimeSpan _period = TimeSpan.FromHours(5);
        public PeriodicHostedService(ILogger<PeriodicHostedService> logger, IServiceProvider serviceProvider)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;            
        }

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            using var scope = _serviceProvider.CreateAsyncScope();
            var feedRepository= scope.ServiceProvider.GetRequiredService<IFeedRepository>();
            using PeriodicTimer timer = new PeriodicTimer(_period);
            while (
                !cancellationToken.IsCancellationRequested &&
                await timer.WaitForNextTickAsync(cancellationToken))
            {
                try
                {
                    var feeds = await feedRepository.GetAllFeedsAsync();
                    var ids = feeds.Select(i => i.FeedCheckId).ToList();
                    var links = feeds.Select(l => l.Link).ToList();

                    foreach (var link in links)
                    {
                        using XmlReader reader = XmlReader.Create(link);
                        var syndicationFeed = SyndicationFeed.Load(reader);

                        if (!ids.Contains(syndicationFeed.Id))
                        {
                            foreach (var item in syndicationFeed.Items)
                            {
                                var feed = await feedRepository.GetFeedByLinkAsync(link);
                                var users = await feedRepository.GetUsersByFeedAsync(feed);

                                var feedItem = new RssItem()
                                {
                                    Link = item.Links.ToString(),
                                    Title = item.Title.ToString(),
                                    Description = item.Content.ToString(),
                                    PublishDate = item.PublishDate,
                                    RssFeedId = feed.Id,
                                    AppUsers = users.ToList()
                                };
                                feedRepository.AddItem(feedItem);

                            }
                        }
                    }

                }
                catch (Exception ex)
                {
                    _logger.LogInformation(
                        $"Failed to execute PeriodicHostedService with exception message {ex.Message}.");
                }
            }

        }

    }
}
