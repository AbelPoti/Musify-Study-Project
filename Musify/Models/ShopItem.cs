using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace Musify.Models
{
    public class ShopItem : ModelBase
    {
        public int InstrumentId { get; set; }

        public Instrument? Instrument { get; set; }

        [Precision(18, 2)]
        public decimal Price { get; set; }

        public int Stock { get; set; }

        [Required]
        [MaxLength(128)]
        public required string Condition { get; set; } = ShopItemCondition.New;
    }
}
