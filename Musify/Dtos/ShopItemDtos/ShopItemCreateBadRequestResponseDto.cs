using System.ComponentModel.DataAnnotations;

namespace Musify.Dtos.ShopItemDtos
{
    public record ShopItemCreateBadRequestResponseDto
    {
        [Required]
        public required string Message { get; set; }
    }
}
