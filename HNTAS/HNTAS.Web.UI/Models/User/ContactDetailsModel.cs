using System.ComponentModel.DataAnnotations;

namespace HNTAS.Web.UI.Models
{
    public enum PreferredContactType
    {
        [Display(Name = "Landline")]
        Landline,
        [Display(Name = "Mobile")]
        Mobile
    }

    public class ContactDetailsModel 
    {
        [Required(ErrorMessage = "Email address is missing.")]
        [EmailAddress(ErrorMessage = "Email address is not in the correct format.")]
        public string? EmailAddress { get; set; }

        [Required(ErrorMessage = "Enter your first name.")]
        [RegularExpression(@"^[a-zA-Z ]+$", ErrorMessage = "First name can only contain letters and spaces.")]
        [MaxLength(50, ErrorMessage = "First name cannot exceed 50 characters.")]
        public string? FirstName { get; set; }

        [Required(ErrorMessage = "Enter your last name.")]
        [RegularExpression(@"^[a-zA-Z ]+$", ErrorMessage = "Last name can only contain letters and spaces.")]
        [MaxLength(50, ErrorMessage = "Last name cannot exceed 50 characters.")]
        public string? LastName { get; set; }

        [Required(ErrorMessage = "Select a preferred contact number type.")]
        public PreferredContactType PreferredContactType { get; set; }

        [RegularExpression(@"^\+?\d{1,3}[\s-]?\(?\d{1,4}\)?[\s-]?\d{1,4}[\s-]?\d{1,4}[\s-]?\d{1,9}$", ErrorMessage = "Landline number is not in a valid format.")]
        [MaxLength(20, ErrorMessage = "Landline number cannot exceed 20 characters.")]
        public string? LandlineNumber { get; set; }

        [RegularExpression(@"^\d*$", ErrorMessage = "Extension must be numeric.")]
        [MaxLength(10, ErrorMessage = "Extension cannot exceed 10 characters.")]
        public string? ContactNumberExtension { get; set; }

        [RegularExpression(@"^\+?\d{1,3}[\s-]?\(?\d{1,4}\)?[\s-]?\d{1,4}[\s-]?\d{1,4}[\s-]?\d{1,9}$", ErrorMessage = "Mobile number is not in a valid format.")]
        [MaxLength(13, ErrorMessage = "Mobile number cannot exceed 13 characters.")]
        public string? MobileNumber { get; set; }

        [Required(ErrorMessage = "Enter your job title.")]
        [MaxLength(100, ErrorMessage = "Job title cannot exceed 100 characters.")]
        [RegularExpression(@"^[a-zA-Z ]+$", ErrorMessage = "Job title can only contain letters and spaces.")]
        public string? JobTitle { get; set; }
    }
}