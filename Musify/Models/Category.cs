using System.ComponentModel.DataAnnotations;

namespace Musify.Models
{
    public class Category : ModelBase
    {
        [Required]
        public required string Name { get; set; }

        // If null, this category is a top-level category
        public int? ParentId { get; set; }
    }
}
