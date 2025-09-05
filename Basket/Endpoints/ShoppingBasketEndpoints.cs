using Basket.Models;
using Basket.Services;

namespace Basket.Endpoints
{
  internal static class ShoppingBasketEndpoints
  {
    internal static readonly string[] value = ["The Username field is required."];

    internal static void MapShoppingBasketEndpoints(this IEndpointRouteBuilder app)
    {
      ArgumentNullException.ThrowIfNull(app);

      var group = app.MapGroup("/basket");

      group
        .MapGet(
          "/{username:minlength(1):maxlength(50)}",
          async (string username, ShoppingBasketService service, HttpContext httpContext) =>
          {
            var basket = await service
              .GetBasketAsync(username, httpContext.RequestAborted)
              .ConfigureAwait(false);

            return basket is null ? Results.NotFound() : Results.Ok(basket);
          }
        )
        .WithName("GetBasket")
        .Produces<ShoppingBasket>()
        .Produces(StatusCodes.Status404NotFound);

      group
        .MapPost(
          "/",
          async (ShoppingBasket basket, ShoppingBasketService service, HttpContext httpContext) =>
          {
            if (string.IsNullOrEmpty(basket.Username))
            {
              return Results.ValidationProblem(
                new Dictionary<string, string[]> { { "Username", value } }
              );
            }

            await service
              .UpdateBasketAsync(basket, httpContext.RequestAborted)
              .ConfigureAwait(false);

            return Results.CreatedAtRoute("GetBasket", new { username = basket.Username });
          }
        )
        .WithName("CreateBasket")
        .Produces<ShoppingBasket>(StatusCodes.Status201Created)
        .ProducesValidationProblem();

      group
        .MapPut(
          "/",
          async (ShoppingBasket basket, ShoppingBasketService service, HttpContext httpContext) =>
          {
            if (string.IsNullOrEmpty(basket.Username))
            {
              return Results.ValidationProblem(
                new Dictionary<string, string[]> { { "Username", value } }
              );
            }

            await service
              .UpdateBasketAsync(basket, httpContext.RequestAborted)
              .ConfigureAwait(false);

            return Results.NoContent();
          }
        )
        .WithName("UpdateBasket")
        .Produces(StatusCodes.Status204NoContent)
        .ProducesValidationProblem();

      group
        .MapDelete(
          "/{username:minlength(1):maxlength(50)}",
          async (string username, ShoppingBasketService service, HttpContext httpContext) =>
          {
            await service
              .DeleteBasketAsync(username, httpContext.RequestAborted)
              .ConfigureAwait(false);

            return Results.NoContent();
          }
        )
        .WithName("DeleteBasket")
        .Produces(StatusCodes.Status204NoContent);
    }
  }
}
