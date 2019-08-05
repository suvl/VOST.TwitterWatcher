using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using System;
using System.Threading;
using System.Threading.Tasks;
using VOST.TwitterWatcher.Core.Interfaces;
using Xunit;

namespace VOST.TwitterWatcher.Background.Tests
{
    public class TwitterBackgroundWatcherTests
    {
        [Fact, Trait("Category", "Integration")]
        public async Task Test1()
        {
            var repoMock = new Mock<ITweetRepository>(MockBehavior.Strict);
            var keywordRepoMock = new Mock<IKeywordRepository>(MockBehavior.Strict);

            ITwitterBackgroundWatcher sut = new TwitterBackgroundWatcher(
                Options.Create(new Core.Configuration.TwitterApiConfiguration
                {
                    ConsumerKey = "Yg6k8dnETccDDmUul0HY7E6or",
                    ConsumerSecret= "P1bVtg2f2TWaWtVAYOf155ZZmkeVoyUSs7TWewwySnLciJOhOe",
                    AccessToken = "187164937-e6U6ct5Vn1JQD2pn0cOyo3SE7BLpvgiqXXNLG9C6",
                    AccessTokenSecret = "MJxdxeiA4B4AL4HNQOkwq3yqOJGdjyQO4rXh7xP0r57qt"
                }),
                repoMock.Object,
                keywordRepoMock.Object,
                new LoggerStub<TwitterBackgroundWatcher>());

            await sut.Subscribe();
            await Task.Delay(TimeSpan.FromMinutes(5));
        }

        private class LoggerStub<T> : ILogger<T>
        {
            public IDisposable BeginScope<TState>(TState state)
            {
                return default;
            }

            public bool IsEnabled(LogLevel logLevel) => true;

            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
            {
                Console.WriteLine(formatter(state, exception));
            }
        }
    }
}
