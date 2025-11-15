using Musify.Models;
using System.ComponentModel.DataAnnotations;

namespace Musify.Dtos.InstrumentDtos
{
    public record InstrumentReadMinimalDto
    {
        public int Id { get; set; }

        [Required]
        public required string Name { get; set; }

        [Required]
        public required string Brand { get; set; }

        public int CategoryId { get; set; }

        public string? Description { get; set; }

        // This property is here to mark the shape of the DTO, so it is fine even though it is never mutated 
        // ReSharper disable once CollectionNeverUpdated.Global
        public ICollection<InstrumentAttributeValue> Attributes { get; set; } = [];
    }
}
