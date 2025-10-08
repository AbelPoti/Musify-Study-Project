using System.ComponentModel.DataAnnotations;

namespace Musify.Models
{
    public class InstrumentAttributeValue : ModelBase
    {
        public int InstrumentId { get; set; }

        [Required]
        public required Instrument Instrument { get; set; }

        public int AttributeDefinitionId { get; set; }

        [Required]
        public required AttributeDefinition AttributeDefinition { get; set; }

        public required string Value { get; set; }
    }
}
