using System.ComponentModel.DataAnnotations;

namespace Musify.Models
{
    public class ShopItem : ModelBase
    {
        public int InstrumentId { get; set; }

        public Instrument? Instrument { get; set; }

        public decimal Price { get; set; }

        public int Stock { get; set; }

        [Required]
        public required string Condition { get; set; } = "New";
    }
}
