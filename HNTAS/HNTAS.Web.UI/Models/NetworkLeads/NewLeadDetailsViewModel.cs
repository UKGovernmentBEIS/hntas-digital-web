using System.ComponentModel.DataAnnotations;
using HNTAS.Web.UI.ModelValidation;

namespace HNTAS.Web.UI.Models.NetworkLeads
{
    public class NewLeadDetailsViewModel
    {
        [Required(ErrorMessage = "Enter your first name.")]
        [RegularExpression(@"^[a-zA-Z0-9\s\.\,\:\'\&\-]+$", ErrorMessage = "Enter a valid first name using letters, numbers or common punctuation only.")]
        [MaxLength(50, ErrorMessage = "First name cannot exceed 50 characters.")]
        public string FirstName 
        { 
            get => _firstName; 
            set => _firstName = value?.Trim(); 
        }

        private string? _firstName;

        [Required(ErrorMessage = "Enter your last name.")]
        [RegularExpression(@"^[a-zA-Z0-9\s\.\,\:\'\&\-]+$", ErrorMessage = "Enter a valid last name using letters, numbers or common punctuation only.")]
        [MaxLength(50, ErrorMessage = "Last name cannot exceed 50 characters.")]
        public string LastName 
        { 
            get => _lastName; 
            set => _lastName = value?.Trim(); 
        }

        private string? _lastName;

        [Required(ErrorMessage = "Enter an email address")]
        [RegularExpression(@"^[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Za-z]{2,}$", ErrorMessage = "Enter an email address in the correct format, like name@example.com")]
        public string? EmailId
        {
            get => _emailId;
            set => _emailId = value?.Trim();
        }

        private string? _emailId;

        [MustBeTrue(ErrorMessage = "Confirm you are authorised to give this person a permission to add heat networks for this organisation.")]
        public bool ConfirmedDeclaration { get; set; }
    }
}
