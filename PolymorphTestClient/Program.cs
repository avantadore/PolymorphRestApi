using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using PolymorphTestClient;
using PolymorpTestClient.Models;
using Refit;

var builder = Host.CreateApplicationBuilder(args);
var shapeApiUrl = builder.Configuration["services:shapeapi:http:0"]
	?? throw new InvalidOperationException("Aspire did not provide the Shape API HTTP endpoint.");

var jsonOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web);
jsonOptions.Converters.Add(new ShapeBaseFallbackConverter());
var refitSettings = new RefitSettings(new SystemTextJsonContentSerializer(jsonOptions));

builder.Services.AddRefitClient<IShapeApiClient>(refitSettings)
	.ConfigureHttpClient(client => client.BaseAddress = new Uri(shapeApiUrl));

using var host = builder.Build();
var shapeApiClient = host.Services.GetRequiredService<IShapeApiClient>();
var shapes = await shapeApiClient.GetShapesAsync();

foreach (var shape in shapes)
{
	Console.WriteLine(shape switch
	{
		ShapeBaseCircle circle => $"Circle: {circle.Name}, radius {circle.Radius}",
		ShapeBaseRectangle rectangle => $"Rectangle: {rectangle.Name}, {rectangle.Width}x{rectangle.Height}",
		_ => $"Shape: {shape.Name}"
	});
}

public interface IShapeApiClient
{
	[Get("/shapes")]
	Task<IReadOnlyList<ShapeBase>> GetShapesAsync();
}
