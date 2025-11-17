using Musify.Models;
using System.ComponentModel.DataAnnotations;

namespace Musify.Dtos.InstrumentDtos
{
    public record InstrumentReadMinimalDto
    {
        public int Id { get; init; }

        [Required]
        public required string Name { get; init; }

        [Required]
        public required string Brand { get; init; }

        public int CategoryId { get; init; }

        public string? Description { get; init; }

        // This property is here to mark the shape of the DTO, so it is fine even though it is never mutated 
        // ReSharper disable once CollectionNeverUpdated.Global
        public ICollection<InstrumentAttributeValue> Attributes { get; init; } = [];
    }
}
