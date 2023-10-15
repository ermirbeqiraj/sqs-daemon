using Amazon;
using Amazon.Extensions.NETCore.Setup;
using Amazon.Runtime;
using Amazon.SQS;
using Daemon.ApplicationServices;
using Daemon.Workers;
using Infrastructure.HttpService;
using Infrastructure.QueueService;

namespace Daemon;

public static class ProgramExtensions
{
	public static void ConfigureApiService(this WebApplicationBuilder builder)
	{
		var apiBase = builder.Configuration["SQSD_APIBASE"] ?? throw new InvalidOperationException("SQSD_APIBASE variable is missing");

		builder.Services.AddHttpClient<IApiService, ApiService>((opts) =>
		{
			opts.BaseAddress = new Uri(apiBase);
			opts.Timeout = TimeSpan.FromHours(1);
		}).ConfigurePrimaryHttpMessageHandler(() => new SocketsHttpHandler
		{
			AllowAutoRedirect = false,
			MaxConnectionsPerServer = 50,
			UseCookies = false,
			UseProxy = false
		});
	}

	public static void ConfigureQueueServices(this WebApplicationBuilder builder)
	{
		var awsopts = builder.Configuration.GetAWSOptions().BuildBasicCredentials();
		builder.Services.AddDefaultAWSOptions(awsopts);
		builder.Services.AddSingleton<IQueueService, QueueService>();
		builder.Services.AddAWSService<IAmazonSQS>();
	}

	public static void AddHostedServices(this WebApplicationBuilder builder)
	{
		builder.Services.AddHostedService<ExecutorService>();
	}

	public static void AddConfigurations(this WebApplicationBuilder builder)
	{
		builder.Configuration.AddEnvironmentVariables();
		builder.Services.AddSingleton<IConfigurationService, ConfigurationService>();
		builder.Services.AddTransient<ConsumerService>();
	}

	private static ImmutableCredentials? GetBasicCredentials()
	{
		var accessKey = Environment.GetEnvironmentVariable("AWS_ACCESS_KEY_ID");
		var secretKey = Environment.GetEnvironmentVariable("AWS_SECRET_ACCESS_KEY");
		var sessionToken = Environment.GetEnvironmentVariable("AWS_SESSION_TOKEN");

		if (string.IsNullOrEmpty(accessKey) || string.IsNullOrEmpty(secretKey))
		{
			return null;
		}

		return new ImmutableCredentials(accessKey, secretKey, sessionToken);
	}

	private static AWSOptions BuildBasicCredentials(this AWSOptions awsOptions)
	{
		if (awsOptions.Credentials == null)
		{
			var basicCredentials = GetBasicCredentials();
			if (basicCredentials != null)
			{
				awsOptions.Credentials =
					new BasicAWSCredentials(basicCredentials.AccessKey, basicCredentials.SecretKey);
			}
		}

		if (awsOptions.Region == null)
		{
			var region = Environment.GetEnvironmentVariable("AWS_REGION");
			awsOptions.Region = !string.IsNullOrWhiteSpace(region) ? RegionEndpoint.GetBySystemName(region) : RegionEndpoint.USEast1;
		}

		return awsOptions;
	}
}
