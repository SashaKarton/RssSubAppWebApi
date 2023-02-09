# RSS Feed Subscription App (WebApi)

# PeriodicHostedService
This background service every 5 hours updating the list of news from the using feed links and store them in the DB.
# Feed controller
## AddFeed method
Add RSS feed into DB using "feed url" parameter.
## GetActiveFeeds method
Get all active RSS feeds that belong to a current user.
## GetUnreadNews method
Get all unread news that belong to a current user from some date using date parameter.
## SetAsRead method
Set news as read.
