using System.ComponentModel.DataAnnotations;

namespace Musify.Models
{
    public class Category : ModelBase
    {
        [Required]
        [MaxLength(128)]
        public required string Name { get; set; }

        // If null, this category is a top-level category
        public int? ParentId { get; set; }
    }
}
