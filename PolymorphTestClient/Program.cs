using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using PolymorpTestClient.Models;
using Refit;

var builder = Host.CreateApplicationBuilder(args);
var shapeApiUrl = builder.Configuration["services:shapeapi:http:0"]
	?? throw new InvalidOperationException("Aspire did not provide the Shape API HTTP endpoint.");

builder.Services.AddRefitClient<IShapeApiClient>()
	.ConfigureHttpClient(client => client.BaseAddress = new Uri(shapeApiUrl));

using var host = builder.Build();
var shapeApiClient = host.Services.GetRequiredService<IShapeApiClient>();
var response = await shapeApiClient.GetShapesAsync();
var shapes = await DeserializeShapesAsync(response);

foreach (var shape in shapes)
{
	Console.WriteLine(shape switch
	{
		ShapeBaseCircle circle => $"Circle: {circle.Name}, radius {circle.Radius}",
		ShapeBaseRectangle rectangle => $"Rectangle: {rectangle.Name}, {rectangle.Width}x{rectangle.Height}",
		ShapeBaseBase baseShape => $"Shape: {baseShape.Name}",
		_ => "Unknown shape"
	});
}

static async Task<IReadOnlyList<object>> DeserializeShapesAsync(HttpResponseMessage response)
{
	response.EnsureSuccessStatusCode();
	await using var stream = await response.Content.ReadAsStreamAsync();
	using var document = await JsonDocument.ParseAsync(stream);
	var shapes = new List<object>();

	foreach (var element in document.RootElement.EnumerateArray())
	{
		var type = element.GetProperty("$type").GetString();
		shapes.Add(type switch
		{
			"Circle" => element.Deserialize<ShapeBaseCircle>()!,
			"Rectangle" => element.Deserialize<ShapeBaseRectangle>()!,
			"Base" => element.Deserialize<ShapeBaseBase>()!,
			_ => throw new JsonException($"Unknown shape discriminator '{type}'.")
		});
	}

	return shapes;
}

public interface IShapeApiClient
{
	[Get("/shapes")]
	Task<HttpResponseMessage> GetShapesAsync();
}
