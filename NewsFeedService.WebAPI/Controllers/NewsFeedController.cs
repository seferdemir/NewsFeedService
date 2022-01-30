using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using NewsFeedService.WebAPI.Data;
using NewsFeedService.WebAPI.Services;

namespace NewsFeedService.WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class NewsFeedController : ControllerBase
    {
        private readonly INewsFeedService _newsFeedService;
        private readonly IMemoryCache _memoryCache;

        public NewsFeedController(INewsFeedService newsFeedService, IMemoryCache memoryCache)
        {
            _newsFeedService = newsFeedService;
            _memoryCache = memoryCache;

        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var user = (await _newsFeedService.Get(new[] { id }, null)).FirstOrDefault();
            if (user == null)
                return NotFound();

            return Ok(user);
        }

        [HttpGet("")]
        public async Task<IActionResult> GetAll([FromQuery] Filters filters)
        {
            IEnumerable<NewsFeedItem> cachedItems = null;

            //bool isExist = _memoryCache.TryGetValue("CacheItems", out cachedItems);
            //if (!isExist)
            //{
            //    cachedItems = await _newsFeedService.Get(null, filters);

            //    var chacheEntryOptions = new MemoryCacheEntryOptions().SetSlidingExpiration(System.TimeSpan.FromSeconds(2));
            //    _memoryCache.Set("CacheItems", cachedItems, chacheEntryOptions);
            //}

            cachedItems = await _memoryCache.GetOrCreateAsync("CacheItems", entry =>
            {
                entry.SlidingExpiration = System.TimeSpan.FromSeconds(2);
                return _newsFeedService.Get(null, filters);
            });

            return Ok(cachedItems);
        }

        [HttpPost]
        public async Task<IActionResult> Add(NewsFeedItem newsFeedItem)
        {
            await _newsFeedService.Add(newsFeedItem);
            return Ok(newsFeedItem);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var user = (await _newsFeedService.Get(new[] { id }, null)).FirstOrDefault();
            if (user == null)
                return NotFound();


            await _newsFeedService.Delete(user);
            return NoContent();
        }
    }
}
