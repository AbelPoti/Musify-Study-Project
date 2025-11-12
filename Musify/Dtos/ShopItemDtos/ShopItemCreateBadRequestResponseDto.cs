using System.ComponentModel.DataAnnotations;

namespace Musify.Dtos.ShopItemDtos
{
    public class ShopItemCreateBadRequestResponseDto
    {
        [Required]
        public required string Message { get; set; }
    }
}
