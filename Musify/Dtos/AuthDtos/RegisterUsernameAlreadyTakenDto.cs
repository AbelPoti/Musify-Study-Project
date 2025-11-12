using System.ComponentModel.DataAnnotations;

namespace Musify.Dtos.AuthDtos
{
    public class RegisterUsernameAlreadyTakenDto
    {
        [Required]
        public required string Message { get; set; }
    }
}
