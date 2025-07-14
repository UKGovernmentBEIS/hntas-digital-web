// HNTAS.Web.UI.Models/ContactDetailsModel.cs
using System.ComponentModel.DataAnnotations;

namespace HNTAS.Web.UI.Models
{
    public class ContactDetailsModel
    {
        [Required(ErrorMessage = "Email address is missing")]
        [EmailAddress(ErrorMessage = "Email address is not in the correct format")]
        public string? EmailAddress { get; set; }

        [Required(ErrorMessage = "Enter your first name")]
        public string? FirstName { get; set; }

        [Required(ErrorMessage = "Enter your last name")]
        public string? LastName { get; set; }

        [Required(ErrorMessage = "Select a preferred contact number type")]
        public string? PreferredContactType { get; set; } // "Landline" or "Mobile"

        [Phone(ErrorMessage = "Landline number is not in a valid format.")]
        [RegularExpression(@"^\+?\d{1,3}[\s-]?\(?\d{1,4}\)?[\s-]?\d{1,4}[\s-]?\d{1,4}[\s-]?\d{1,9}$", ErrorMessage = "Landline number is not in a valid format.")]
        [MaxLength(20, ErrorMessage = "Landline number cannot exceed 20 characters.")] // Added MaxLength validation for LandlineNumber
        public string? LandlineNumber { get; set; }
        public string? ContactNumberExtension { get; set; } // This will only apply to LandlineNumber
        [Phone(ErrorMessage = "Mobile number is not in a valid format.")]
        [RegularExpression(@"^\+?\d{1,3}[\s-]?\(?\d{1,4}\)?[\s-]?\d{1,4}[\s-]?\d{1,4}[\s-]?\d{1,9}$", ErrorMessage = "Mobile number is not in a valid format.")]
        [MaxLength(20, ErrorMessage = "Mobile number cannot exceed 20 characters.")] // Added MaxLength validation for MobileNumber
        public string? MobileNumber { get; set; }

        [Required(ErrorMessage = "Enter your job title")]
        public string? JobTitle { get; set; }
    }
}