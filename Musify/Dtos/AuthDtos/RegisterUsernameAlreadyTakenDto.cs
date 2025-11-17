using System.ComponentModel.DataAnnotations;

namespace Musify.Dtos.AuthDtos
{
    public record RegisterUsernameAlreadyTakenDto
    {
        [Required]
        public required string Message { get; set; }
    }
}
