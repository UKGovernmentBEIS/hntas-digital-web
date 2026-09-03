using DocumentFormat.OpenXml.Wordprocessing;
using System.ComponentModel.DataAnnotations;

namespace HNTAS.Web.UI.Models.User
{
    public class OrganisationContactDetailsModel : ContactDetailsModel
    {
        [Required]
        [RegularExpression(@"^[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Za-z]{2,}$", ErrorMessage = "Enter an email address in the correct format, like name@example.com")]
        public string? EmailAddress { get; set; }

        [Required(ErrorMessage = "Enter your job title")]
        [MaxLength(100, ErrorMessage = "Job title cannot exceed 100 characters")]
        [RegularExpression(@"^[a-zA-Z0-9\s\.\,\:\'\&]+$", ErrorMessage = "Enter a valid job title using letters, numbers or common punctuation only")]
        public string? JobTitle { get; set; }
    }
}