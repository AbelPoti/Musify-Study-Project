using System.ComponentModel.DataAnnotations;

namespace Musify.Dtos.AuthDtos
{
    public class EmailConfirmNotFoundResponseDto
    {
        [Required]
        public required string Message { get; set; }
    }
}
