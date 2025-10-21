using System.ComponentModel.DataAnnotations;

namespace Musify.Models
{
    public class Instrument : ModelBase
    {
        [Required]
        [MaxLength(256)]
        public required string Name { get; set; }

        [Required]
        [MaxLength(128)]
        public required string Brand { get; set; }

        public int CategoryId { get; set; }

        [Required]
        public required Category Category { get; set; }

        [MaxLength(1024)]
        public string? Description { get; set; }

        public ICollection<InstrumentAttributeValue> Attributes { get; set; } = [];

        public Instrument()
        {
            
        }

        public Instrument(string name, string brand, Category category, string description)
        {
            Name = name;
            Brand = brand;
            Category = category;
            Description = description;
        }
    }
}
