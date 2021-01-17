using AspNetCore.HealthChecks.SystemMemory;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System;
using System.Collections.Generic;

namespace Microsoft.Extensions.DependencyInjection
{
	public static class SystemMemoryHealthCheckExtensions
	{
		private const string DefaultName = "sysmem";

		public static IHealthChecksBuilder AddSystemMemory(
			this IHealthChecksBuilder builder, int minimumFreeMegabytes, string name = default,
			HealthStatus? failureStatus = default, IEnumerable<string> tags = default, TimeSpan? timeout = default)
		{
			if (minimumFreeMegabytes <= 0)
			{
				throw new ArgumentException($"{nameof(minimumFreeMegabytes)} should be greater than zero");
			}

			var registrationName = name ?? DefaultName;

			return builder.Add(new HealthCheckRegistration(
			   registrationName,
			   sp => new SystemMemoryHealthCheck(minimumFreeMegabytes),
			   failureStatus,
			   tags,
			   timeout));
		}
	}
}