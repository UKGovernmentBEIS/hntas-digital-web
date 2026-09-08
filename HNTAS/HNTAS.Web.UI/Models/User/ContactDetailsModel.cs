using HNTAS.Web.UI.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace HNTAS.Web.UI.Models.User
{
    public class ContactDetailsModel
    {
        private string? _firstName;
        private string? _lastName;
        private string? _landlineNumber;
        private string? _contactNumberExtension;
        private string? _mobileNumber;

        [Required(ErrorMessage = "Enter your first name")]
        [RegularExpression(@"^[a-zA-Z0-9\s\.\,\:\'\&\-]+$", ErrorMessage = "Enter a valid first name using letters, numbers or common punctuation only")]
        [MaxLength(50, ErrorMessage = "First name cannot exceed 50 characters")]
        public string? FirstName
        {
            get => _firstName;
            set => _firstName = value?.Trim();
        }

        [Required(ErrorMessage = "Enter your last name")]
        [RegularExpression(@"^[a-zA-Z0-9\s\.\,\:\'\&\-]+$", ErrorMessage = "Enter a valid last name using letters, numbers or common punctuation only")]
        [MaxLength(50, ErrorMessage = "Last name cannot exceed 50 characters")]
        public string? LastName
        {
            get => _lastName;
            set => _lastName = value?.Trim();
        }

        [Required(ErrorMessage = "Enter either a landline number or a mobile number")]
        public PreferredContactType? PreferredContactType { get; set; }

        [RegularExpression(@"^[\d\s\+\-]+$", ErrorMessage = "Enter a valid landline number — use only numbers, spaces, plus or hyphens")]
        [MaxLength(20, ErrorMessage = "Landline number cannot exceed 20 characters")]
        public string? LandlineNumber
        {
            get => _landlineNumber;
            set => _landlineNumber = value?.Trim();
        }

        [RegularExpression(@"^[\d\s\+\-]+$", ErrorMessage = "Enter a valid extension — use only numbers, spaces, plus or hyphens")]
        [MaxLength(10, ErrorMessage = "Extension cannot exceed 10 characters")]
        public string? ContactNumberExtension
        {
            get => _contactNumberExtension;
            set => _contactNumberExtension = value?.Trim();
        }

        [RegularExpression(@"^[\d\s\+\-]+$", ErrorMessage = "Enter a valid mobile number — use only numbers, spaces, plus or hyphens")]
        [MaxLength(13, ErrorMessage = "Mobile number cannot exceed 13 characters")]
        public string? MobileNumber
        {
            get => _mobileNumber;
            set => _mobileNumber = value?.Trim();
        }
    }
}
