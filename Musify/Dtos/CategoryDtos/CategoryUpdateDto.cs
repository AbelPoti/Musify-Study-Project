using System.ComponentModel.DataAnnotations;

namespace Musify.Dtos.CategoryDtos
{
    public class CategoryUpdateDto
    {
        public int Id { get; set; }

        [Required]
        public required string Name { get; set; }

        public int? ParentId { get; set; }
    }
}
