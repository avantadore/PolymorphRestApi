using Microsoft.Extensions.Hosting;

var builder = DistributedApplication.CreateBuilder(args);

var shapeApi = builder.AddProject<Projects.PolymorphRestApi_ShapeApi>("shapeapi");
builder.AddProject<Projects.PolymorphTestClient>("testclient")
    .WithReference(shapeApi);
if (builder.Environment.IsDevelopment())
{
    builder.Configuration["ASPIRE_DASHBOARD_UNSECURED_ALLOW_ANONYMOUS"] = "true";
}

builder.Build().Run();
