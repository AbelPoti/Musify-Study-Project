using System.ComponentModel.DataAnnotations;

namespace Musify.Dtos.ShopItemDtos
{
    public class ShopItemUpdateNotFoundResponseDto
    {
        [Required]
        public required string Message { get; set; }
    }
}
