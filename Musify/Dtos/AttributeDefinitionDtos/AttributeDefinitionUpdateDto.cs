using System.ComponentModel.DataAnnotations;

namespace Musify.Dtos.AttributeDefinitionDtos
{
    public class AttributeDefinitionUpdateDto
    {
        public int Id { get; set; }

        [Required]
        public required string Name { get; set; }

        [Required]
        public required string DataType { get; set; }

        public int CategoryId { get; set; }
    }
}
