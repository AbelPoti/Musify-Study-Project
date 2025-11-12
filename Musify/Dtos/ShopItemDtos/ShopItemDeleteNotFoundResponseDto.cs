using System.ComponentModel.DataAnnotations;

namespace Musify.Dtos.ShopItemDtos
{
    public class ShopItemDeleteNotFoundResponseDto
    {
        [Required]
        public required string Message { get; set; }
    }
}
