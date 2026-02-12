using System.ComponentModel.DataAnnotations;

namespace eprogram.Models
{
    public class EmailViewModel
    {
        [Required(ErrorMessage = "From email is required")]
        [EmailAddress(ErrorMessage = "Enter a valid email address")]
        public string From { get; set; } = string.Empty;

        [Required(ErrorMessage = "To email is required")]
        [EmailAddress(ErrorMessage = "Enter a valid email address")]
        public string To { get; set; } = string.Empty;

        [Required(ErrorMessage = "Subject is required")]
        public string Subject { get; set; } = string.Empty;

        [Required(ErrorMessage = "Message is required")]
        public string Message { get; set; } = string.Empty;
    }
}