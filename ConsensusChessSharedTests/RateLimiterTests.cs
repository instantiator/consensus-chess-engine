using System;
using ConsensusChessShared.Social;
using Microsoft.Extensions.Logging;
using Moq;

namespace ConsensusChessSharedTests
{
	[TestClass]
	public class RateLimiterTests
	{
        private Mock<ILogger> mockLogger;

		[TestInitialize]
		public void Init()
		{
            mockLogger = new Mock<ILogger>();
        }

        [TestMethod]
		public async Task RateLimitingOccurs_WhenPermittedUsageExceeded()
		{
			var limiter = new RateLimiter(mockLogger.Object, 300, TimeSpan.FromSeconds(2));

			var start = DateTime.Now;
			for (int i = 0; i < 300; i++)
			{
				await limiter.RateLimitAsync();
			}
			var fin1 = DateTime.Now;
			var duration1 = fin1 - start;
			Assert.IsTrue(duration1.TotalSeconds < 1);

            await limiter.RateLimitAsync(); // 301st hit

			var fin2 = DateTime.Now;
            var duration2 = fin2 - start;
            Assert.IsTrue(duration2.TotalSeconds > 2);
        }
	}
}

