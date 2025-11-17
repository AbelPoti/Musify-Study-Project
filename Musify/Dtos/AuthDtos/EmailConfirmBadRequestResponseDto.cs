using System.ComponentModel.DataAnnotations;

namespace Musify.Dtos.AuthDtos
{
    public record EmailConfirmBadRequestResponseDto
    {
        [Required]
        public required string Message { get; set; }

        [Required]
        public required IEnumerable<string> Errors { get; set; }
    }
}
