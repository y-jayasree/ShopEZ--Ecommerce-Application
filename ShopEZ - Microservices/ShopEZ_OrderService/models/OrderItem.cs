using ShopEZ_OrderService.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class OrderItem
{
    public int OrderItemId { get; set; }
    public int OrderId { get; set; }
    [ForeignKey(nameof(OrderId))]
    public Order? Order { get; set; }
    [Required]
    public int ProductId { get; set; }
    [Required]
    [StringLength(200)]
    public string ProductName { get; set; } = "";
    [Range(1, 100)]
    public int Quantity { get; set; }
    [Column(TypeName = "decimal(10,2)")]
    public decimal UnitPrice { get; set; }
    [StringLength(500)]
    public string? ImageUrl { get; set; } 
}