using Musify.Models;
using System.ComponentModel.DataAnnotations;

namespace Musify.Dtos.InstrumentAttributeValueDtos
{
    public class InstrumentAttributeValueCreateDto
    {
        public int InstrumentId { get; set; }

        public int AttributeDefinitionId { get; set; }

        public required string Value { get; set; }
    }
}
