using System.ComponentModel.DataAnnotations;

namespace Musify.Dtos.CategoryDtos
{
    public class CategoryCreateDto
    {
        [Required]
        public required string Name { get; set; }

        // If null, this category is a top-level category
        public int? ParentId { get; set; }
    }
}
