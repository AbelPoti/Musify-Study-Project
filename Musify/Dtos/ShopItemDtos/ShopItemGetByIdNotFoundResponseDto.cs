using System.ComponentModel.DataAnnotations;

namespace Musify.Dtos.ShopItemDtos
{
    public class ShopItemGetByIdNotFoundResponseDto
    {
        [Required]
        public required string Message { get; set; }
    }
}
