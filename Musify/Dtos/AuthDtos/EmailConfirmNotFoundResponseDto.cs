using System.ComponentModel.DataAnnotations;

namespace Musify.Dtos.AuthDtos
{
    public record EmailConfirmNotFoundResponseDto
    {
        [Required]
        public required string Message { get; set; }
    }
}
