using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RSS_Subscription_Application.Interfaces;
using System.ServiceModel.Syndication;
using System.Xml;
using RSS_Sub_App.Models;

namespace RSS_Sub_App.Controllers
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [Route("api/[controller]")]
    [ApiController]    
    public class FeedController : ControllerBase
    {       

        private readonly IFeedRepository _feedRepository;

        public FeedController(IFeedRepository feedRepository)
        {
            _feedRepository = feedRepository;
        }

        [HttpPost]
        [Route("add")]
        [ProducesResponseType(200)]
        public async Task<IActionResult> AddFeed(string feedLink)
        {          
            var user = await _feedRepository.GetUserAsync();

            using XmlReader reader = XmlReader.Create(feedLink);
            var syndicationFeed = SyndicationFeed.Load(reader);
                
            if(syndicationFeed == null)
                return NotFound();

            var feed = new RssFeed()
            {
                FeedCheckId = syndicationFeed.Id.ToString(),
                Link = feedLink,
                Title = syndicationFeed.Title.ToString(),
                Description = syndicationFeed.Description.ToString(),
                LastUpdate = syndicationFeed.LastUpdatedTime                    

            };                
            feed.AppUsers.Add(user);
                
            (int id, bool success) = _feedRepository.AddAndGetId(feed);
            if (success == false) return BadRequest(error: "Adding feed failed");
                
            if(syndicationFeed.Items != null)
            {
                foreach(var item in syndicationFeed.Items)
                {
                    var feedItem = new RssItem()
                    {
                        Link = item.Links.ToString(),
                        Title = item.Title.ToString(),
                        Description = item.Content.ToString(),
                        PublishDate = item.PublishDate,
                        RssFeedId = id,                            
                    };
                    feedItem.AppUsers.Add(user);
                    _feedRepository.AddItem(feedItem);
                }                    
            }          
                        
            return Ok("Feed was added successfully");
        }

        [HttpGet]
        [Route("feeds")]
        [ProducesResponseType(200, Type = typeof(IEnumerable<RssFeed>))]
        [ProducesResponseType(204)]
        public async Task<IActionResult> GetActiveFeeds()
        {
            var appUsers = await _feedRepository.GetUserWithFeedAsync();
            var feedIds = appUsers.Feeds.Select(i => i.Id).ToList();
            var feeds = _feedRepository.GetFreshFeeds(feedIds);

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            return Ok(feeds);           
                     
            
        }

        [HttpGet]
        [Route("news")]
        [ProducesResponseType(200, Type = typeof(IEnumerable<RssItem>))]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetUnreadNews(DateTime date)
        {
            if (date == null)
                return BadRequest();
            var news = _feedRepository.GetAllNewItemsAsync(date);

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            return Ok(news);         
                       
        }

        [HttpPost]
        [Route("setasread")]
        [ProducesResponseType(400)]
        public async Task<IActionResult> SetAsRead(RssItem news)
        {
            var status = news.ReadStatus;
            status = true;
            var success = _feedRepository.UpdateItem(news);

            if (success == false) return BadRequest(error: "Status change failed");
            return Ok("News was set as read");
        }
    }
}
