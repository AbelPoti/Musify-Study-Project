using System.ComponentModel.DataAnnotations;

namespace Musify.Dtos.AuthDtos
{
    public class EmailConfirmOkResponseDto
    {
        [Required]
        public required string Message { get; set; }
    }
}
