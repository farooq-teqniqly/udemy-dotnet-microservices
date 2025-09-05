using System.ComponentModel.DataAnnotations;

namespace Basket.Models;

internal sealed class ShoppingBasketItem
{
  [DataType(DataType.Currency)]
  [Range(typeof(decimal), "0.01", "99999.99")]
  public decimal Price { get; set; }

  [Required]
  [Range(typeof(int), "0", "1000")]
  public int Quantity { get; set; }
}
