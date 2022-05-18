using Orders.Protos;
using ShopConsole;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices(services =>
    {
        services.AddGrpcClient<OrderService.OrderServiceClient>(o =>
        {
            o.Address = new Uri("https://localhost:5005");
        });
        services.AddHostedService<Worker>();
    })
    .Build();

await host.RunAsync();
