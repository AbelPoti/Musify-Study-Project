using System.ComponentModel.DataAnnotations;

namespace Musify.Dtos.ShopItemDtos
{
    public record ShopItemUpdateBadRequestResponseDto
    {
        [Required]
        public required string Message { get; set; }
    }
}
