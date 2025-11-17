using System.ComponentModel.DataAnnotations;

namespace Musify.Dtos.AuthDtos
{
    public record EmailConfirmOkResponseDto
    {
        [Required]
        public required string Message { get; set; }
    }
}
