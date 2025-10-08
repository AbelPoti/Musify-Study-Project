using System.ComponentModel.DataAnnotations;

namespace Musify.Dtos.ShopItem
{
    public class ShopItemCreateDto
    {
        public int InstrumentId { get; set; }

        public decimal Price { get; set; }

        public int Stock { get; set; }

        [Required]
        public required string Condition { get; set; }
    }
}
