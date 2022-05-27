using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DocumentGenerator3
{
    public static class RetryHelper
    {
        public static async Task RetryOnExceptionAsync(int maxRetryAttempts, Func<Task> operation)
        {
            await RetryOnExceptionAsync<Exception>(maxRetryAttempts, operation);
        }

        public static async Task RetryOnExceptionAsync<TException>(int maxRetryAttempts, Func<Task> operation) where TException : Exception
        {
            if (maxRetryAttempts <= 0)
                throw new ArgumentOutOfRangeException(nameof(maxRetryAttempts));

            var retryattempts = 0;
            do
            {
                try
                {
                    retryattempts++;
                    await operation();
                    break;
                }
                catch (TException ex)
                {
                    if (retryattempts == maxRetryAttempts)
                        throw;

                    await CreateRetryDelayForException(maxRetryAttempts, retryattempts, ex);
                }
            } while (true);
        }

        private static Task CreateRetryDelayForException(int maxRetryAttempts, int attempts, Exception ex)
        {
            int delay = IncreasingDelayInSeconds(attempts);
            Console.WriteLine("Attempt {0} of {1} failed. New retry after {2} seconds.", attempts.ToString(), maxRetryAttempts.ToString(), delay.ToString());
            return Task.Delay(delay);
        }

        internal static int[] DelayPerAttemptInSeconds =
        {
            (int) TimeSpan.FromSeconds(5).TotalSeconds,
            (int) TimeSpan.FromSeconds(30).TotalSeconds,
            (int) TimeSpan.FromMinutes(3).TotalSeconds,
            (int) TimeSpan.FromMinutes(10).TotalSeconds,
            (int) TimeSpan.FromMinutes(30).TotalSeconds
        };

        static int IncreasingDelayInSeconds(int failedAttempts)
        {
            if (failedAttempts <= 0) throw new ArgumentOutOfRangeException();

            return failedAttempts >= DelayPerAttemptInSeconds.Length ? DelayPerAttemptInSeconds.Last() : DelayPerAttemptInSeconds[failedAttempts];
        }
    }
}
