using System.ComponentModel.DataAnnotations;

namespace Musify.Dtos.AuthDtos
{
    public class LoginUnauthorizedResponseDto
    {
        [Required]
        public required string Message { get; set; }
    }
}
