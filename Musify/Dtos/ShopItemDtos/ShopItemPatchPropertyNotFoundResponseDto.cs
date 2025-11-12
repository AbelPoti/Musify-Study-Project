using System.ComponentModel.DataAnnotations;

namespace Musify.Dtos.ShopItemDtos
{
    public class ShopItemPatchPropertyNotFoundResponseDto
    {
        [Required]
        public required string Message { get; set; }
    }
}
