using System.ComponentModel.DataAnnotations;

namespace Musify.Dtos.ShopItemDtos
{
    public record ShopItemGetByIdNotFoundResponseDto
    {
        [Required]
        public required string Message { get; set; }
    }
}
