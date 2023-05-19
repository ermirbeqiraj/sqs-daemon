using Daemon;
using Serilog;
using Serilog.Formatting.Compact;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console(new RenderedCompactJsonFormatter())
    .CreateLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog();

    // setup
    builder.AddConfigurations();
    // application services
    builder.ConfigureQueueServices();
    builder.ConfigureApiService();
    builder.AddHostedServices();

    // run
    var app = builder.Build();
    app.MapGet("/", () => "App Started!");

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}