using Microsoft.Extensions.Diagnostics.HealthChecks;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace AspNetCore.HealthChecks.SystemMemory
{
    public class SystemMemoryHealthCheck : IHealthCheck
    {
        private class MemoryMetrics
        {
            public double Total;
            public double Used;
            public double Free;
        }

        private readonly int _minimumFreeMegabytes;
        private static bool IsUnix => RuntimeInformation.IsOSPlatform(OSPlatform.Linux) ||
                                      RuntimeInformation.IsOSPlatform(OSPlatform.OSX);

        public SystemMemoryHealthCheck(int minimumUsedMegabytes)
        {
            _minimumFreeMegabytes = minimumUsedMegabytes;
        }

        public Task<HealthCheckResult> CheckHealthAsync(
            HealthCheckContext context,
            CancellationToken cancellationToken = default)
        {
            var metrics = new MemoryMetrics();

            if (IsUnix)
            {
                var info = new ProcessStartInfo("free -m")
                {
                    FileName = "/bin/bash",
                    Arguments = "-c \"free -m\"",
                    RedirectStandardOutput = true
                };

                using var process = Process.Start(info);
                string output = process.StandardOutput.ReadToEnd();

                var lines = output.Split("\n");
                var memory = lines[1].Split(" ", StringSplitOptions.RemoveEmptyEntries);

                metrics.Total = double.Parse(memory[1]);
                metrics.Used = double.Parse(memory[2]);
                metrics.Free = double.Parse(memory[3]);
            }
            else
            {
                var info = new ProcessStartInfo
                {
                    FileName = "wmic",
                    Arguments = "OS get FreePhysicalMemory,TotalVisibleMemorySize /Value",
                    RedirectStandardOutput = true
                };

                using var process = Process.Start(info);
                string output = process.StandardOutput.ReadToEnd();

                var lines = output.Trim().Split("\n");
                var freeMemoryParts = lines[0].Split("=", StringSplitOptions.RemoveEmptyEntries);
                var totalMemoryParts = lines[1].Split("=", StringSplitOptions.RemoveEmptyEntries);

                metrics.Total = Math.Round(double.Parse(totalMemoryParts[1]) / 1024, 0);
                metrics.Free = Math.Round(double.Parse(freeMemoryParts[1]) / 1024, 0);
                metrics.Used = metrics.Total - metrics.Free;
            }

            if (metrics.Free <= _minimumFreeMegabytes)
            {
                return Task.FromResult(
                    new HealthCheckResult(
                        context.Registration.FailureStatus,
                        description: $"The system is low on memory: {metrics.Free} mb of {metrics.Total} mb"));
            }

            return Task.FromResult(HealthCheckResult.Healthy(description:
                $"Free megabytes in memory: {metrics.Free} mb of {metrics.Total} mb"));
        }
    }
}
