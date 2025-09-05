using System.ComponentModel.DataAnnotations;

namespace Basket.Models
{
  internal sealed class ShoppingBasket
  {
    public List<ShoppingBasketItem> Items { get; set; } = [];
    public decimal TotalPrice => Items.Sum(i => i.Price * i.Quantity);

    [Required]
    [MinLength(1)]
    [MaxLength(50)]
    public string Username { get; set; } = string.Empty;
  }
}
