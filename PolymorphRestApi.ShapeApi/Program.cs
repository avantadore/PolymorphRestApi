using OpenTelemetry.Metrics;
using OpenTelemetry.Logs;
using OpenTelemetry.Trace;
using PolymorphRestApi.ShapeApi.Models;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer((document, _, _) =>
    {
        //options.AddSchemaTransformer<ShapeInheritanceSchemaTransformer>();
        document.Info = new()
        {
            Title = "Polymorph REST API",
            Version = "v1",
            Description = "REST API for Polymorph shapes."
        };
        
        return Task.CompletedTask;
    });
});
builder.Services.AddOpenTelemetry()
    .WithTracing(options => options
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddOtlpExporter())
    .WithMetrics(options => options
        .AddAspNetCoreInstrumentation()
        .AddRuntimeInstrumentation()
        .AddOtlpExporter());
builder.Logging.AddOpenTelemetry(options =>
{
    options.IncludeFormattedMessage = true;
    options.IncludeScopes = true;
    options.ParseStateValues = true;
    options.AddOtlpExporter();
});

var app = builder.Build();
app.Logger.LogInformation("Shape API started");

// Configure the HTTP request pipeline.
app.MapOpenApi();
app.MapScalarApiReference(options =>
{
    options.WithTitle("Polymorph REST API");
    options.WithOpenApiRoutePattern("/openapi/{documentName}.json");
});

app.UseHttpsRedirection();

app.MapGet("/shapes", () =>
{
    app.Logger.LogInformation("Shapes requested");

    ShapeBase[] shapes =
    [
        new Circle
        {
            Name = "Sample circle",
            Center = new Point { X = 0, Y = 0 },
            Radius = 10
        },
        new Rectangle
        {
            Name = "Sample rectangle",
            TopLeft = new Point { X = 0, Y = 0 },
            Width = 20,
            Height = 10
        },
        // new Triangle
        // {
        //     Name = "Sample triangle",
        //     Vertex1 = new Point { X = 0, Y = 0 },
        //     Vertex2 = new Point { X = 10, Y = 0 },
        //     Vertex3 = new Point { X = 5, Y = 10 }
        // }   
    ];

    return shapes;
})
.WithName("GetShapes")
.WithSummary("Get sample shapes")
.WithDescription("Returns one circle and one rectangle.")
.Produces<ShapeBase[]>(StatusCodes.Status200OK, "application/json");

app.Run();

