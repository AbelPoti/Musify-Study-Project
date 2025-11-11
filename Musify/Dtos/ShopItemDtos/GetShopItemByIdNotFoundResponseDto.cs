using System.ComponentModel.DataAnnotations;

namespace Musify.Dtos.ShopItemDtos
{
    public class GetShopItemByIdNotFoundResponseDto
    {
        [Required]
        public required string Message { get; set; }
    }
}
