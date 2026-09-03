using HNTAS.Web.UI.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace HNTAS.Web.UI.Models.User
{
    public class ContactDetailsModel
    {
        [Required(ErrorMessage = "Enter your first name")]
        [RegularExpression(@"^[a-zA-Z0-9\s\.\,\:\'\&\-]+$", ErrorMessage = "Enter a valid first name using letters, numbers or common punctuation only")]
        [MaxLength(50, ErrorMessage = "First name cannot exceed 50 characters")]
        public string? FirstName { get; set; }


        [Required(ErrorMessage = "Enter your last name")]
        [RegularExpression(@"^[a-zA-Z0-9\s\.\,\:\'\&\-]+$", ErrorMessage = "Enter a valid last name using letters, numbers or common punctuation only")]
        [MaxLength(50, ErrorMessage = "Last name cannot exceed 50 characters")]
        public string? LastName { get; set; }

        [Required(ErrorMessage = "Enter either a landline number or a mobile number")]
        public PreferredContactType? PreferredContactType { get; set; }

        [RegularExpression(@"^[\d\s\+\-]+$", ErrorMessage = "Enter a valid landline number — use only numbers, spaces, plus or hyphens")]
        [MaxLength(20, ErrorMessage = "Landline number cannot exceed 20 characters")]
        public string? LandlineNumber { get; set; }

        [RegularExpression(@"^[\d\s\+\-]+$", ErrorMessage = "Enter a valid extension — use only numbers, spaces, plus or hyphens")]
        [MaxLength(10, ErrorMessage = "Extension cannot exceed 10 characters")]
        public string? ContactNumberExtension { get; set; }

        [RegularExpression(@"^[\d\s\+\-]+$", ErrorMessage = "Enter a valid mobile number — use only numbers, spaces, plus or hyphens")]
        [MaxLength(13, ErrorMessage = "Mobile number cannot exceed 13 characters")]
        public string? MobileNumber { get; set; }
    }
}
