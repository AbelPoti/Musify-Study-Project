using System.ComponentModel.DataAnnotations;

namespace Musify.Dtos.ShopItemDtos
{
    public record ShopItemPatchPropertyBadRequestResponseDto
    {
        [Required]
        public required string Message { get; set; }
    }
}
