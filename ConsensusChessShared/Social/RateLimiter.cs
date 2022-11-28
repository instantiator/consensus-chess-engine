using System;
using System.Threading;
using Microsoft.Extensions.Logging;

namespace ConsensusChessShared.Social
{
	public class RateLimiter
	{
		private ILogger log;

		public RateLimiter(ILogger log, int permitted, TimeSpan period)
		{
			this.log = log;
			Permitted = permitted;
			Period = period;
			History = new List<DateTime>();
		}

        private static readonly SemaphoreSlim semaphore = new SemaphoreSlim(1, 1);

        public int Permitted { get; private set; }
		public TimeSpan Period { get; private set; }
		public List<DateTime> History { get; private set; }

		public async Task RateLimitAsync()
		{
			// rate limiting should happen across all threads
            await semaphore.WaitAsync();
			try
			{
				var rollingPeriodStart = DateTime.Now.Subtract(Period);
				var hitsInPeriod = History.Where(d => d >= rollingPeriodStart).Order();
				if (hitsInPeriod.Count() >= Permitted)
				{
					log.LogWarning("Rate limit exceeded... Introducing a delay.");
					var mostRecentCluster = History.Skip(Math.Max(0, History.Count() - Permitted)).Order();
					var earliestOfCluster = mostRecentCluster.First();
					var newPeriod = earliestOfCluster.Add(Period);
					var delay = newPeriod.Subtract(DateTime.Now);
					await Task.Delay(delay);
				}
				History.RemoveAll(d => d < rollingPeriodStart);
				History.Add(DateTime.Now);
            }
            finally
            {
                semaphore.Release();
            }
        }

	}
}

