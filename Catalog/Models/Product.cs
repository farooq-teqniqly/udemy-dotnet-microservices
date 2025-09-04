using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace Catalog.Models
{
  internal sealed class Product
  {
    [MaxLength(1000)]
    public string Description { get; set; } = null!;
    public int Id { get; set; }

    [MaxLength(100)]
    public string ImageFilename { get; set; } = null!;

    [MaxLength(100)]
    public string Name { get; set; } = null!;

    [Precision(7, 2)]
    [Range(typeof(decimal), "0.01", "99999.99")]
    public decimal Price { get; set; }
  }
}
