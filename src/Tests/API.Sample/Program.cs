
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

var app = builder.Build();

app.MapGet("/configuration", () =>
{
    var data = new
    {
        QueueUrl = "https://sqs.us-east-1.amazonaws.com/087855136007/DIQ-Tracking-Errors",
        ApiMaxConcurrency = 30,
        VisibilityTimeout = 60,
        ErrorVisibilityTimeout = 60
    };

    return Results.Ok(data);
});

app.MapPost("/", async ([FromBody] JsonElement model) =>
{
    if (!builder.Environment.IsDevelopment())
        throw new InvalidOperationException("This POST location is only for testing.");

    var payload = model.GetRawText();

    await Task.Delay(TimeSpan.FromMinutes(1));

    return Results.Ok();
});

app.Run();