using Grpc.Net.Client;
using Ingredients.Tests.Protos;
using Microsoft.AspNetCore.Mvc.Testing;

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

        Assert.NotEmpty(response.Toppings);
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
}