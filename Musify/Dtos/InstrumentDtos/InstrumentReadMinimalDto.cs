using Musify.Models;
using System.ComponentModel.DataAnnotations;

namespace Musify.Dtos.InstrumentDtos
{
    public class InstrumentReadMinimalDto
    {
        public int Id { get; set; }

        [Required]
        public required string Name { get; set; }

        [Required]
        public required string Brand { get; set; }

        public int CategoryId { get; set; }

        public string? Description { get; set; }

        public ICollection<InstrumentAttributeValue> Attributes { get; set; } = [];
    }
}
