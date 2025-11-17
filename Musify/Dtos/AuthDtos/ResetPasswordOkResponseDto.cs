using System.ComponentModel.DataAnnotations;

namespace Musify.Dtos.AuthDtos
{
    public record ResetPasswordOkResponseDto
    {
        [Required]
        public required string Message { get; set; }
    }
}
