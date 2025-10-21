using System.ComponentModel.DataAnnotations;

namespace Musify.Models
{
    public class AttributeDefinition : ModelBase
    {
        [Required]
        [MaxLength(128)]
        public required string Name { get; set; }

        [Required]
        public required AttributeDefinitionDataType DataType { get; set; }

        public int CategoryId { get; set; }

        [Required]
        public required Category Category { get; set; }
    }
}
