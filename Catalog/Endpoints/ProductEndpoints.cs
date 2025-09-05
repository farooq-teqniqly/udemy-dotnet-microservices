using Catalog.Models;
using Catalog.Services;

namespace Catalog.Endpoints
{
  internal static class ProductEndpoints
  {
    internal static void MapProductEndpoints(this IEndpointRouteBuilder app)
    {
      var group = app.MapGroup("/products");

      group
        .MapGet(
          "/",
          async (ProductService service, HttpContext httpContext) =>
          {
            var products = await service
              .GetProductsAsync(httpContext.RequestAborted)
              .ConfigureAwait(false);
            return Results.Ok(products);
          }
        )
        .WithName("GetAllProducts")
        .Produces<IEnumerable<Product>>();

      group
        .MapGet(
          "/{id}",
          async (int id, ProductService service, HttpContext httpContext) =>
          {
            var product = await service
              .GetProductByIdAsync(id, httpContext.RequestAborted)
              .ConfigureAwait(false);

            return product is null ? Results.NotFound() : Results.Ok(product);
          }
        )
        .WithName("GetProductById")
        .Produces<Product>()
        .Produces(StatusCodes.Status404NotFound);

      group
        .MapPost(
          "/",
          async (Product product, ProductService service, HttpContext httpContext) =>
          {
            await service
              .CreateProductAsync(product, httpContext.RequestAborted)
              .ConfigureAwait(false);

            return Results.CreatedAtRoute("GetProductById", new { id = product.Id });
          }
        )
        .WithName("CreateProduct")
        .Produces<Product>(StatusCodes.Status201Created);

      group
        .MapPut(
          "/{id}",
          async (int id, Product inputProduct, ProductService service, HttpContext httpContext) =>
          {
            var cancellationToken = httpContext.RequestAborted;

            var updatedProduct = await service
              .GetProductByIdAsync(id, cancellationToken)
              .ConfigureAwait(false);

            if (updatedProduct is null)
            {
              return Results.NotFound();
            }

            await service
              .UpdateProductAsync(updatedProduct, inputProduct, cancellationToken)
              .ConfigureAwait(false);

            return Results.NoContent();
          }
        )
        .WithName("UpdateProduct")
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status404NotFound);

      group
        .MapDelete(
          "/{id}",
          async (int id, ProductService service, HttpContext httpContext) =>
          {
            var cancellationToken = httpContext.RequestAborted;

            var deletedProduct = await service
              .GetProductByIdAsync(id, cancellationToken)
              .ConfigureAwait(false);

            if (deletedProduct is null)
            {
              return Results.NotFound();
            }

            await service
              .DeleteProductAsync(deletedProduct, cancellationToken)
              .ConfigureAwait(false);

            return Results.NoContent();
          }
        )
        .WithName("DeleteProduct")
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status404NotFound);
    }
  }
}
