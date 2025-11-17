using System.ComponentModel.DataAnnotations;

namespace Musify.Dtos.ShopItemDtos
{
    public record ShopItemPatchPropertyNotFoundResponseDto
    {
        [Required]
        public required string Message { get; set; }
    }
}
