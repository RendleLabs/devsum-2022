﻿using System.Security.Claims;
using Grpc.Core;
using Ingredients.Data;
using Ingredients.Protos;
using Microsoft.AspNetCore.Authorization;

namespace Ingredients.Services
{
    public class IngredientsImpl : IngredientsService.IngredientsServiceBase
    {
        private readonly IToppingData _toppingData;
        private readonly ILogger<IngredientsImpl> _logger;

        public IngredientsImpl(IToppingData toppingData, ILogger<IngredientsImpl> logger)
        {
            _toppingData = toppingData;
            _logger = logger;
        }

        public override async Task<GetToppingsResponse> GetToppings(GetToppingsRequest request, ServerCallContext context)
        {
            try
            {
                var toppings = await _toppingData.GetAsync(context.CancellationToken);

                _logger.LogInformation("Found {Count} toppings", toppings.Count);

                var response = new GetToppingsResponse
                {
                    Toppings =
                    {
                        toppings.Select(t => new Topping
                        {
                            Id = t.Id,
                            Name = t.Name,
                            OldPrice = t.Price,
                            Price = Convert.ToDecimal(t.Price)
                        })
                    }
                };

                return response;
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception e)
            {
                _logger.LogError(e, e.Message);
                throw new RpcException(new Status(StatusCode.Internal, e.Message, e));
            }
        }

        public override async Task<DecrementToppingsResponse> DecrementToppings(DecrementToppingsRequest request, ServerCallContext context)
        {
            var user = context.GetHttpContext()
                .User.FindFirst(ClaimTypes.Name)?.Value;

            _logger.LogInformation("DecrementStock triggered by {User}", user);
            var tasks = request.ToppingIds
                .Select(id => _toppingData.DecrementStockAsync(id));
            await Task.WhenAll(tasks);

            return new DecrementToppingsResponse();
        }
    }
}
