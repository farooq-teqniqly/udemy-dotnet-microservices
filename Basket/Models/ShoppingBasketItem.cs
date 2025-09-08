using System.ComponentModel.DataAnnotations;

namespace Basket.Models;

internal sealed class ShoppingBasketItem
{
  [Required]
  [MinLength(1)]
  [MaxLength(100)]
  public string Name { get; set; } = null!;

  [DataType(DataType.Currency)]
  [Range(typeof(decimal), "0.01", "99999.99")]
  public decimal Price { get; set; }

  [Required]
  [Range(typeof(int), "1", "999")]
  public int ProductId { get; set; }

  [Required]
  [Range(typeof(int), "0", "1000")]
  public int Quantity { get; set; }
}
