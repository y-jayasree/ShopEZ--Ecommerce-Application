using System.ComponentModel.DataAnnotations;
namespace ShopEZ_Shared_Lib.DTOs
{
    public class OrderItemDTO
    {
        [Required]
        [Range(1, int.MaxValue)]
        public int ProductId { get; set; }
        [Required]
        [StringLength(200)]
        public string ProductName { get; set; } = "";
        [Range(1, 100)]
        public int Quantity { get; set; }
        [Range(0.01, double.MaxValue)]
        public decimal UnitPrice { get; set; }
        [StringLength(500)]
        public string? ImageUrl { get; set; } 
    }
    public class CreateOrderDTO
    {
        [Required(ErrorMessage = "At least one item is required")]
        [MinLength(1, ErrorMessage = "Order must have at least one item")]
        public List<OrderItemDTO> Items { get; set; } = new();
        [StringLength(500)]
        public string? ShippingAddress { get; set; }
        [StringLength(50)]
        public string PaymentMethod { get; set; } = "CASH_ON_DELIVERY";
    }
    public class OrderResponseDTO
    {
        public int OrderId { get; set; }
        public int UserId { get; set; }
        public string OrderNumber { get; set; } = "";
        public DateTime OrderDate { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; } = "";
        public string? ShippingAddress { get; set; }
        public string PaymentMethod { get; set; } = "";
        public List<OrderItemResponseDTO> Items { get; set; } = new();
    }
    public class OrderItemResponseDTO
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = "";
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public string? ImageUrl { get; set; }  
        public decimal Subtotal => UnitPrice * Quantity;
    }
    public class UpdateOrderStatusDTO
    {
        [Required]
        [RegularExpression("PENDING|CONFIRMED|SHIPPED|DELIVERED|CANCELLED",
            ErrorMessage = "Invalid status value")]
        public string Status { get; set; } = "";
    }
}