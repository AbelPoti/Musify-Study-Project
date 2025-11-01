using System.ComponentModel.DataAnnotations;

namespace Musify.Dtos.AuthDtos
{
    public class RegisterGoodResponseDto
    {
        [Required]
        public required string Message { get; set; }

        [Required]
        public required string JwtToken { get; set; }
    }
}
