
using Ingredients.Data;
using Microsoft.AspNetCore.Server.Kestrel.Core;

var builder = WebApplication.CreateBuilder(args);

// Only for macOS development
if (Environment.OSVersion.Platform == PlatformID.MacOSX)
{
    builder.WebHost.ConfigureKestrel(k =>
    {
        k.ConfigureEndpointDefaults(o => { o.Protocols = HttpProtocols.Http2; });
    });
}

// Additional configuration is required to successfully run gRPC on macOS.
// For instructions on how to configure Kestrel and gRPC clients on macOS, visit https://go.microsoft.com/fwlink/?linkid=2099682

// Add services to the container.
builder.Services.AddGrpc();

builder.Services.AddSingleton<IToppingData, ToppingData>();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");

app.Run();
