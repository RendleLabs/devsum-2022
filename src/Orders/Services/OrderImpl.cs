using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Ingredients.Protos;
using Microsoft.AspNetCore.Authorization;
using Orders.Protos;
using Orders.PubSub;

namespace Orders.Services
{
    public class OrderImpl : OrderService.OrderServiceBase
    {
        private readonly IngredientsService.IngredientsServiceClient _ingredients;
        private readonly IOrderPublisher _orderPublisher;
        private readonly IOrderMessages _orderMessages;
        private readonly ILogger<OrderImpl> _logger;

        public OrderImpl(IngredientsService.IngredientsServiceClient ingredients,
            IOrderPublisher orderPublisher,
            IOrderMessages orderMessages,
            ILogger<OrderImpl> logger)
        {
            _ingredients = ingredients;
            _orderPublisher = orderPublisher;
            _orderMessages = orderMessages;
            _logger = logger;
        }

        [Authorize]
        public override async Task<PlaceOrderResponse> PlaceOrder(PlaceOrderRequest request, ServerCallContext context)
        {
            var decrementToppingsRequest = new DecrementToppingsRequest
            {
                ToppingIds = {request.ToppingIds}
            };

            await _ingredients.DecrementToppingsAsync(decrementToppingsRequest);

            var dueBy = DateTimeOffset.UtcNow.AddMinutes(30);

            await _orderPublisher.PublishOrder(request.CrustId, request.ToppingIds, dueBy);

            var response = new PlaceOrderResponse
            {
                DueBy = dueBy.ToTimestamp()
            };
            return response;
        }

        public override async Task Subscribe(SubscribeRequest request,
            IServerStreamWriter<OrderNotification> responseStream,
            ServerCallContext context)
        {
            var token = context.CancellationToken;

            while (!token.IsCancellationRequested)
            {
                try
                {
                    var message = await _orderMessages.ReadAsync(token);
                    var notification = new OrderNotification
                    {
                        CrustId = message.CrustId,
                        ToppingIds = {message.ToppingIds},
                        DueBy = message.Time.ToTimestamp()
                    };
                    await responseStream.WriteAsync(notification, token);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception e)
                {
                    _logger.LogError(e, e.Message);
                }
            }
        }
    }
}
