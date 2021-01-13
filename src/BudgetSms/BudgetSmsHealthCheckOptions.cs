namespace AspNetCore.HealthChecks.BudgetSms
{
	public class BudgetSmsHealthCheckOptions
	{
		public string Username { get; set; }
		public string UserId { get; set; }
		public string Handle { get; set; }
		public double? MinimumCredit { get; set; }
	}
}
