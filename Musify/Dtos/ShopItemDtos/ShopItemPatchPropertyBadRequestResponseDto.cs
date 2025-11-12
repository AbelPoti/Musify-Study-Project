using System.ComponentModel.DataAnnotations;

namespace Musify.Dtos.ShopItemDtos
{
    public class ShopItemPatchPropertyBadRequestResponseDto
    {
        [Required]
        public required string Message { get; set; }
    }
}
