using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NewsFeedService.WebAPI.Data;

namespace NewsFeedService.WebAPI.Services
{
    public class NewsFeedService : INewsFeedService
    {
        private readonly NewsFeedContext _newsFeedContext;

        public NewsFeedService(NewsFeedContext newsFeedContext)
        {
            _newsFeedContext = newsFeedContext;
        }

        public async Task<IEnumerable<NewsFeedItem>> Get(int[] ids, Filters filters)
        {
            var newsItems = _newsFeedContext.NewsFeedItems.AsQueryable();

            if (filters == null)
                filters = new Filters();

            if (filters.Body != null && filters.Body.Any())
                newsItems = newsItems.Where(x => filters.Body.Contains(x.Body));

            if (filters.AuthorNames != null && filters.AuthorNames.Any())
                newsItems = newsItems.Where(x => filters.AuthorNames.Contains(x.AuthorName));

            if (filters.Title != null && filters.Title.Any())
                newsItems = newsItems.Where(x => filters.Title.Contains(x.Title));

            if (ids != null && ids.Any())
                newsItems = newsItems.Where(x => ids.Contains(x.Id));

            await Task.Delay(2000);

            return await newsItems.ToListAsync();
        }

        public async Task<NewsFeedItem> Add(NewsFeedItem newsFeedItem)
        {
            await _newsFeedContext.NewsFeedItems.AddAsync(newsFeedItem);
            newsFeedItem.DateCreated = DateTime.UtcNow;

            await _newsFeedContext.SaveChangesAsync();
            return newsFeedItem;
        }

        public async Task<IEnumerable<NewsFeedItem>> AddRange(IEnumerable<NewsFeedItem> newsItems)
        {
            await _newsFeedContext.NewsFeedItems.AddRangeAsync(newsItems);
            await _newsFeedContext.SaveChangesAsync();
            return newsItems;
        }

        public async Task<NewsFeedItem> Update(NewsFeedItem newsFeedItem)
        {
            var newsItemForChanges = await _newsFeedContext.NewsFeedItems.SingleAsync(x => x.Id == newsFeedItem.Id);
            newsItemForChanges.Body = newsFeedItem.Body;
            newsItemForChanges.Title = newsFeedItem.Title;
            newsItemForChanges.AllowComments = newsFeedItem.AllowComments;

            _newsFeedContext.NewsFeedItems.Update(newsItemForChanges);
            await _newsFeedContext.SaveChangesAsync();
            return newsFeedItem;
        }

        public async Task<bool> Delete(NewsFeedItem newsFeedItem)
        {
            _newsFeedContext.NewsFeedItems.Remove(newsFeedItem);
            await _newsFeedContext.SaveChangesAsync();

            return true;
        }
    }

    public interface INewsFeedService
    {
        Task<IEnumerable<NewsFeedItem>> Get(int[] ids, Filters filters);

        Task<NewsFeedItem> Add(NewsFeedItem newsFeedItem);

        Task<IEnumerable<NewsFeedItem>> AddRange(IEnumerable<NewsFeedItem> newsItems);

        Task<NewsFeedItem> Update(NewsFeedItem newsFeedItem);

        Task<bool> Delete(NewsFeedItem newsFeedItem);
    }

    public class Filters
    {
        public string[] Body { get; set; }
        public string[] AuthorNames { get; set; }
        public string[] Title { get; set; }
    }
}
