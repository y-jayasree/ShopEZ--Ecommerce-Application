using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ShopEZ_OrderService.Models
{
    public class Order
    {
        public int OrderId { get; set; }

        [Required]
        public int UserId { get; set; }
        [Required]
        [StringLength(30)]
        public string OrderNumber { get; set; } = "";

        public DateTime OrderDate { get; set; } = DateTime.UtcNow;

        [Column(TypeName = "decimal(10,2)")]
        public decimal TotalAmount { get; set; }

        [StringLength(50)]
        public string Status { get; set; } = "PENDING";

        [StringLength(500)]
        public string? ShippingAddress { get; set; }

        [StringLength(50)]
        public string PaymentMethod { get; set; } = "CASH_ON_DELIVERY";

        public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
    }
}