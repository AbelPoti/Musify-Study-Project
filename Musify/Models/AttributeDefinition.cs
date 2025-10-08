using System.ComponentModel.DataAnnotations;

namespace Musify.Models
{
    public class AttributeDefinition
    {
        public int Id { get; set; }

        [Required]
        public required string Name { get; set; }

        [Required]
        public required string DataType { get; set; }

        public int CategoryId { get; set; }

        [Required]
        public required Category Category { get; set; }
    }
}
