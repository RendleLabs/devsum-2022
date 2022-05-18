using Grpc.Net.Client;
using Ingredients.Data;
using Ingredients.Tests.Protos;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using NSubstitute;

namespace Ingredients.Tests;

public class IngredientsServiceTests : IClassFixture<IngredientsApplicationFactory>
{
    private readonly IngredientsApplicationFactory _factory;

    public IngredientsServiceTests(IngredientsApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task GetsToppings()
    {
        var client = _factory.CreateGrpcClient();

        var response = await client.GetToppingsAsync(new GetToppingsRequest());

        Assert.Collection(response.Toppings,
            t =>
            {
                Assert.Equal("cheese", t.Id);
                Assert.Equal(0.5d, t.Price);
            },
            t =>
            {
                Assert.Equal("tomato", t.Id);
            });
    }
}

public class IngredientsApplicationFactory
    : WebApplicationFactory<Ingredients.Protos.IngredientsService.IngredientsServiceBase>
{
    public IngredientsService.IngredientsServiceClient CreateGrpcClient()
    {
        var httpClient = CreateDefaultClient();
        var channel = GrpcChannel.ForAddress(httpClient.BaseAddress!,
            new GrpcChannelOptions
            {
                HttpClient = httpClient
            });
        return new IngredientsService.IngredientsServiceClient(channel);
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            services.RemoveAll<IToppingData>();
            services.AddSingleton(SubToppingData());
        });
    }

    private static IToppingData SubToppingData()
    {
        var toppings = new List<ToppingEntity>
        {
            new("cheese", "Cheese", 0.5d, 10),
            new("tomato", "Tomato", 0.5d, 10),
        };

        var sub = Substitute.For<IToppingData>();

        sub.GetAsync(Arg.Any<CancellationToken>())
            .Returns(toppings);

        return sub;
    }
}