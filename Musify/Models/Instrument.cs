using System.ComponentModel.DataAnnotations;

namespace Musify.Models
{
    public class Instrument : ModelBase
    {
        [Required]
        public required string Name { get; set; }

        [Required]
        public required string Brand { get; set; }

        public int CategoryId { get; set; }

        [Required]
        public required Category Category { get; set; }

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
