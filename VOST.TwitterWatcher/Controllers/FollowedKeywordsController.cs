using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace VOST.TwitterWatcher.Controllers
{
    /// <summary>
    /// The Followed Keywords controller
    /// </summary>
    /// <seealso cref="Microsoft.AspNetCore.Mvc.Controller" />
    [Route("api/v1/keywords")]
    public class FollowedKeywordsController : Controller
    {
        private readonly Core.Interfaces.IKeywordRepository _repository;
        private readonly Core.Interfaces.ITwitterBackgroundWatcher _watcher;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="FollowedKeywordsController"/> class.
        /// </summary>
        /// <param name="repository">The repository.</param>
        /// <param name="watcher">The watcher.</param>
        /// <param name="logger">The logger.</param>
        /// <exception cref="ArgumentNullException">
        /// repository
        /// or
        /// watcher
        /// or
        /// logger
        /// </exception>
        public FollowedKeywordsController(
            Core.Interfaces.IKeywordRepository repository,
            Core.Interfaces.ITwitterBackgroundWatcher watcher,
            ILogger<FollowedKeywordsController> logger)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _watcher = watcher ?? throw new ArgumentNullException(nameof(watcher));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Gets the followed keywords.
        /// </summary>
        /// <returns>All the followed keywords, whether enabled or disabled.</returns>
        /// <response code="200">A list of followed keywords.</response>
        [HttpGet]
        [Route("")]
        [ProducesResponseType(200, Type = typeof(IList<Repo.FollowedKeyword>))]
        public async Task<IActionResult> GetFollowedKeywords()
        {
            _logger.LogInformation("GetFollowedKeywords");
            var keywords = await _repository.GetAllFollowedKeywords();
            _logger.LogDebug("GetFollowedKeywords results={0}", keywords.Count);
            return Ok(keywords);
        }

        /// <summary>
        /// Creates a followed keyword.
        /// </summary>
        /// <param name="keyword">The keyword.</param>
        /// <returns>The domain model of the keyword</returns>
        /// <response code="201">The created keyword.</response>
        /// <response code="400">.Word must not be empty.</response>
        [HttpPut]
        [Route("new")]
        [ProducesResponseType(400, Type = typeof(string))]
        [ProducesResponseType(201, Type = typeof(Repo.FollowedKeyword))]
        public async Task<IActionResult> CreateFollowedKeyword([FromBody] Core.DTO.Keyword keyword)
        {
            if (string.IsNullOrWhiteSpace(keyword.Word))
            {
                return BadRequest(".Word must not be empty");
            }

            var domainKeyword = new Repo.FollowedKeyword
            {
                Id = keyword.Word,
                Keyword = keyword.Word,
                Enabled = keyword.Enabled
            };

            await _repository.InsertAsync(domainKeyword);
            TriggerNewSubscribe();
            return StatusCode(201, domainKeyword);
        }

        /// <summary>
        /// Toggles the enabled status of a certain keyword.
        /// </summary>
        /// <param name="keyword">The keyword.</param>
        /// <returns></returns>
        [HttpPatch]
        [Route("toggle")]
        [ProducesResponseType(404)]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> ToggleEnabledStatus([FromQuery] string keyword)
        {
            if (string.IsNullOrWhiteSpace(keyword))
            {
                return BadRequest("keyword must not be empty.");
            }

            try
            {
                var followedKeyword = await _repository.GetAsync(keyword);
                followedKeyword.Enabled = !followedKeyword.Enabled;
                await _repository.UpdateAsync(followedKeyword);
                TriggerNewSubscribe();
                return Ok(followedKeyword);
            }
            catch (Repo.EntityNotFoundException)
            {
                return NotFound();
            }
        }

        private void TriggerNewSubscribe()
        {
            System.Threading.ThreadPool.QueueUserWorkItem(state => _watcher.Subscribe());
        }
    }
}
