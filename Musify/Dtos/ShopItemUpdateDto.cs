using System.ComponentModel.DataAnnotations;

namespace Musify.Dtos
{
    public class ShopItemUpdateDto
    {
        public int Id { get; set; }

        public int InstrumentId { get; set; }

        public decimal Price { get; set; }

        public int Stock { get; set; }

        [Required]
        public required string Condition { get; set; }
    }
}
