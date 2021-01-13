using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace AspNetCore.HealthChecks.BudgetSms
{
	internal sealed class BudgetSmsHealthCheck : IHealthCheck
	{
		private readonly IHttpClientFactory _httpClientFactory;
		private readonly BudgetSmsHealthCheckOptions _budgetSmsOptions;
		private const string ApiBaseAddress = "https://api.budgetsms.net";

		public BudgetSmsHealthCheck(
			IHttpClientFactory httpClientFactory,
			BudgetSmsHealthCheckOptions budgetSmsOptions)
		{
			_httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
			_budgetSmsOptions = budgetSmsOptions ?? throw new ArgumentNullException(nameof(budgetSmsOptions));
		}

		public async Task<HealthCheckResult> CheckHealthAsync(
			HealthCheckContext context,
			CancellationToken cancellationToken = default)
		{
			try
			{
				var registrationName = context.Registration.Name;
				var httpClient = _httpClientFactory.CreateClient(registrationName);
				httpClient.BaseAddress = new Uri(ApiBaseAddress);

				var queryParams = new Dictionary<string, string>
				{
					{ "username", _budgetSmsOptions.Username },
					{ "userid", _budgetSmsOptions.UserId },
					{ "handle", _budgetSmsOptions.Handle }
				};

				var response = await httpClient.GetAsync(
					QueryHelpers.AddQueryString("checkcredit", queryParams), cancellationToken);

				response.EnsureSuccessStatusCode();

				var responseText = await response.Content.ReadAsStringAsync();

				if (responseText.StartsWith("ERR"))
				{
					return new HealthCheckResult(context.Registration.FailureStatus,
						description: $"Budget SMS API returned an error: {responseText}");
				}

				if (!responseText.StartsWith("OK"))
				{
					return new HealthCheckResult(context.Registration.FailureStatus,
						description: "Budget SMS API returned an unexpected response");
				}

				if (_budgetSmsOptions.MinimumCredit.HasValue)
				{
					var credit = responseText.Replace("OK ", string.Empty);

					if (double.TryParse(credit, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture,
						out var numValue) && numValue < _budgetSmsOptions.MinimumCredit)
					{
						return HealthCheckResult.Degraded(
							description: $"Budget SMS credit is low: {credit}");
					}
				}

				return HealthCheckResult.Healthy();
			}
			catch (Exception ex)
			{
				return new HealthCheckResult(context.Registration.FailureStatus, exception: ex);
			}
		}
	}
}
