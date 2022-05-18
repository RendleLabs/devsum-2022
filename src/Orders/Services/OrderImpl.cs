using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Ingredients.Protos;
using Orders.Protos;

namespace Orders.Services
{
    public class OrderImpl : OrderService.OrderServiceBase
    {
        private readonly IngredientsService.IngredientsServiceClient _ingredients;

        public OrderImpl(IngredientsService.IngredientsServiceClient ingredients)
        {
            _ingredients = ingredients;
        }

        public override async Task<PlaceOrderResponse> PlaceOrder(PlaceOrderRequest request, ServerCallContext context)
        {
            var decrementToppingsRequest = new DecrementToppingsRequest
            {
                ToppingIds = {request.ToppingIds}
            };

            await _ingredients.DecrementToppingsAsync(decrementToppingsRequest);

            var dueBy = DateTimeOffset.UtcNow.AddMinutes(30);
            var response = new PlaceOrderResponse
            {
                DueBy = dueBy.ToTimestamp()
            };
            return response;
        }
    }
}
