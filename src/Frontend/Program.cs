using AuthHelp;
using Grpc.Core;
using Ingredients.Protos;
using Microsoft.Extensions.Logging.Abstractions;
using Orders.Protos;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

if (Environment.OSVersion.Platform == PlatformID.MacOSX)
{
    AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);
}

var ingredientsUri = builder.Configuration.GetServiceUri("Ingredients", "https")
                     ?? new Uri("https://localhost:5003");

builder.Services.AddHttpClient("ingredients")
    .ConfigurePrimaryHttpMessageHandler(DevelopmentModeCertificateHelper.CreateClientHandler);

builder.Services.AddGrpcClient<IngredientsService.IngredientsServiceClient>(o => { o.Address = ingredientsUri; })
    .ConfigureChannel((provider, channel) =>
    {
        channel.HttpHandler = null;
        channel.HttpClient = provider.GetRequiredService<IHttpClientFactory>()
            .CreateClient("ingredients");
        channel.DisposeHttpClient = true;
    });

var ordersUri = builder.Configuration.GetServiceUri("Orders", "https")
                ?? new Uri("https://localhost:5005");

builder.Services.AddHttpContextAccessor();

builder.Services.AddGrpcClient<OrderService.OrderServiceClient>(o => { o.Address = ordersUri; })
    .ConfigureChannel((provider, channel) =>
    {
        var callCredentials = CallCredentials.FromInterceptor(async (ctx, metadata) =>
        {
            // To get the token from the Request header
            // var hc = provider.GetRequiredService<IHttpContextAccessor>().HttpContext;
            // var token = hc.Request.Headers.Authorization.FirstOrDefault();

            var token = JwtHelper.GenerateJwtToken("frontend");
            metadata.Add("Authorization", $"Bearer {token}");
        });

        channel.Credentials = ChannelCredentials.Create(ChannelCredentials.SecureSsl, callCredentials);
    });

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();