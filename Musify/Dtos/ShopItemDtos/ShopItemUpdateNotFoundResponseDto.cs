using System.ComponentModel.DataAnnotations;

namespace Musify.Dtos.ShopItemDtos
{
    public record ShopItemUpdateNotFoundResponseDto
    {
        [Required]
        public required string Message { get; set; }
    }
}
