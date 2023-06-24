using System;
using System.Threading;
using System.Threading.Tasks;

namespace ProiectLicenta.Model
{
    internal class DataInsertionThreads
    {
        private CancellationTokenSource _cancellationTokenSource;
        private int delayVar = 5000;
        public void Start()
        {
            // Create a cancellation token source to allow graceful cancellation
            _cancellationTokenSource = new CancellationTokenSource();

            // Start the data insertion tasks
            Task liveCpuDataTask = InsertLiveCpuDataAsync(_cancellationTokenSource.Token);
            Task hourlyCpuDataTask = InsertHourlyCpuDataAsync(_cancellationTokenSource.Token);
            Task liveRamDataTask = InsertLiveRamDataAsync(_cancellationTokenSource.Token);
            Task hourlyRamDataTask = InsertHourlyRamDataAsync(_cancellationTokenSource.Token);
            Task liveNetworkDataTask = InsertLiveNetworkDataAsync(_cancellationTokenSource.Token);
            Task hourlyNetworkDataTask = InsertHourlyNetworkDataAsync(_cancellationTokenSource.Token);
            Task liveDiskDataTask = InsertLiveDiskDataAsync(_cancellationTokenSource.Token);

            // Wait for all tasks to complete
            Task.WaitAll(liveCpuDataTask, hourlyCpuDataTask, liveRamDataTask, hourlyRamDataTask, liveNetworkDataTask, hourlyNetworkDataTask, liveDiskDataTask);
        }

        public void Stop()
        {
            // Cancel the tasks and wait for their completion
            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource = null;
        }
        private async Task InsertLiveCpuDataAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                CpuDataInserter.InsertCpuUsageInDb("CPULiveUsageData");
                await Task.Delay(delayVar, cancellationToken);
            }
        }
        private async Task InsertHourlyCpuDataAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                DateTime now = DateTime.Now;
                DateTime nextHour = now.AddHours(1).Date.AddHours(now.Hour + 1);
                // Calculate the delay until the next hour
                TimeSpan delay = nextHour - now;
                // Wait until the start of the next hour
                await Task.Delay(delay, cancellationToken);
                // Insert the CPU usage data at the start of the hour
                CpuDataInserter.InsertCpuUsageInDb("CPUHourlyUsageData");
            }
        }
        private async Task InsertLiveRamDataAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                RamDataInserter.InsertRamUsageInDb("RamLiveUsageData");
                await Task.Delay(delayVar + 5000, cancellationToken);
            }
        }
        private async Task InsertHourlyRamDataAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                DateTime now = DateTime.Now;
                DateTime nextHour = now.AddHours(1).Date.AddHours(now.Hour + 1);
                // Calculate the delay until the next hour
                TimeSpan delay = nextHour - now;
                // Wait until the start of the next hour
                await Task.Delay(delay, cancellationToken);
                // Insert the CPU usage data at the start of the hour
                RamDataInserter.InsertRamUsageInDb("RamHourlyUsageData");
            }
        }

        private async Task InsertLiveNetworkDataAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                NetworkDataInserter.InsertNetworkUsageInDb("NetworkLiveUsageData");
                await Task.Delay(delayVar + 10000, cancellationToken);
            }
        }
        private async Task InsertHourlyNetworkDataAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                DateTime now = DateTime.Now;
                DateTime nextHour = now.AddHours(1).Date.AddHours(now.Hour + 1);
                // Calculate the delay until the next hour
                TimeSpan delay = nextHour - now;
                // Wait until the start of the next hour
                await Task.Delay(delay, cancellationToken);
                // Insert the CPU usage data at the start of the hour
                NetworkDataInserter.InsertNetworkUsageInDb("NetworkHourlyUsageData");
            }
        }
        private async Task InsertLiveDiskDataAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                DiskDataInserter.InsertDiskUsageInDb("DiskLiveUsageData");
                await Task.Delay(delayVar + 15000, cancellationToken);
            }
        }
    }
}
