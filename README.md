# AspNetCore.HealthChecks.SystemMemory
Cross-platform system memory health check

## Installation

```
> dotnet add package AspNetCore.HealthChecks.SystemMemory
```

```
PM> Install-Package AspNetCore.HealthChecks.SystemMemory
```
## Usage

```csharp

public class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddHealthChecks()
            .AddSystemMemory(500);
    }
}
```


# AspNetCore.HealthChecks.BudgetSms
.NET Health check for [Badget SMS](https://www.budgetsms.net/) API connectivity and low credit

## Installation

```
> dotnet add package AspNetCore.HealthChecks.BudgetSms
```

```
PM> Install-Package AspNetCore.HealthChecks.BudgetSms
```

## Usage

```csharp

public class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddHealthChecks()
            .AddBudgetSms(options =>
	    {
	    	options.Username = Configuration["BudgetSms:Username"];
		options.UserId = Configuration["BudgetSms:UserId"];
		options.Handle = Configuration["BudgetSms:Handle"];
		options.MinimumCredit = 30;
	    });
    }
}
```
