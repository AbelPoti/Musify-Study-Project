using System.ComponentModel.DataAnnotations;

namespace Musify.Dtos.ShopItemDtos
{
    public class ShopItemUpdateBadRequestResponseDto
    {
        [Required]
        public required string Message { get; set; }
    }
}
