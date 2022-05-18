using Ingredients.Protos;
using Orders.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddGrpc();

if (Environment.OSVersion.Platform == PlatformID.MacOSX)
{
    AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);
}

var ingredientsUri = builder.Configuration.GetServiceUri("Ingredients", "https")
    ?? new Uri("https://localhost:5003");

builder.Services.AddGrpcClient<IngredientsService.IngredientsServiceClient>(o =>
{
    o.Address = ingredientsUri;
});

var app = builder.Build();

app.MapGrpcService<OrderImpl>();

app.Run();
