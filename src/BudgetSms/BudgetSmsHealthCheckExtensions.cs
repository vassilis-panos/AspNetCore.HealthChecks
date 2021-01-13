using AspNetCore.HealthChecks.BudgetSms;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System;
using System.Collections.Generic;
using System.Net.Http;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class BudgetSmsHealthCheckExtensions
	{
		private const string DefaultName = "budgetsms";

		public static IHealthChecksBuilder AddBudgetSms(this IHealthChecksBuilder builder,
			Action<BudgetSmsHealthCheckOptions> setup, string name = default,
			HealthStatus? failureStatus = default, IEnumerable<string> tags = default, TimeSpan? timeout = default)
		{
			var registrationName = name ?? DefaultName;
			var budgetSmsOptions = new BudgetSmsHealthCheckOptions();

			builder.Services.AddHttpClient(registrationName);
			setup?.Invoke(budgetSmsOptions);

			return builder.Add(new HealthCheckRegistration(
			   registrationName,
			   sp => new BudgetSmsHealthCheck(sp.GetRequiredService<IHttpClientFactory>(), budgetSmsOptions),
			   failureStatus,
			   tags,
			   timeout));
		}
	}
}
