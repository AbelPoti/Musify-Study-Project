using Musify.Models;
using System.ComponentModel.DataAnnotations;

namespace Musify.Dtos.AttributeDefinitionDtos
{
    public class AttributeDefinitionReadDetailedDto
    {
        public int Id { get; set; }

        [Required]
        public required string Name { get; set; }

        [Required]
        public required AttributeDefinitionDataType DataType { get; set; }

        public int CategoryId { get; set; }

        public required Category Category { get; set; }
    }
}
