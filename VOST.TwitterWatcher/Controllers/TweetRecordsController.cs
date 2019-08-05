using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using VOST.TwitterWatcher.Core.Interfaces;

namespace VOST.TwitterWatcher.Controllers
{
    /// <summary>
    /// The tweet records controller
    /// </summary>
    [Route("api/v1/tweets")]
    public class TweetRecordsController : Controller
    {
        private readonly ITweetRepository _repository;
        private readonly ILogger _logger;

        /// <summary>
        /// TweetRecordsController .ctor
        /// </summary>
        /// <param name="repository">The tweet repository.</param>
        /// <param name="logger">The logger.</param>
        public TweetRecordsController(ITweetRepository repository, ILogger<TweetRecordsController> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        }

        /// <summary>
        /// Get all tweet records, paged.
        /// </summary>
        /// <param name="before">Get tweets before date.</param>
        /// <param name="after">Get tweets after date.</param>
        /// <param name="page">The page number, starting at 0.</param>
        /// <param name="pageSize">The page size.</param>
        /// <returns></returns>
        [HttpGet]
        [Route("")]
        public async Task<IActionResult> GetTweetRecords(
            [FromQuery] DateTime? before,
            [FromQuery] DateTime? after,
            [FromQuery] int page = 0, 
            [FromQuery] int pageSize = 100)
        {
            if (page < 0) return StatusCode(400, "page must be 0 or higher");
            if (pageSize < 0) return StatusCode(400, "pageSize must be 0 or higher");
            if (before.HasValue && before.Value > DateTime.Now) return StatusCode(400, "before cannot be in the future");
            if (after.HasValue && after.Value > DateTime.Now) return StatusCode(400, "after cannot be in the future");

            _logger.LogInformation("GetTweetRecords page={0} pageSize={1} before={2} after={3}", page, pageSize, before, after);

            var results = await _repository.GetRecordsPaged(page, pageSize, before, after);

            _logger.LogDebug("GetTweetRecords result={0}", results.Count);

            return new OkObjectResult(results);
        }
    }
}
