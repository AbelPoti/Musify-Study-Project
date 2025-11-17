using System.ComponentModel.DataAnnotations;

namespace Musify.Dtos.ShopItemDtos
{
    public record ShopItemDeleteNotFoundResponseDto
    {
        [Required]
        public required string Message { get; set; }
    }
}
