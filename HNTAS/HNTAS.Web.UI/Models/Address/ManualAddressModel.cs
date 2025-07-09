using System.ComponentModel.DataAnnotations;

namespace HNTAS.Web.UI.Models.Address
{
    public class ManualAddressModel
    {
        [Required(ErrorMessage = "Address line 1 is required.")]
        public string AddressLine1 { get; set; } = string.Empty; // Initialize with a default value to avoid CS8618

        public string AddressLine2 { get; set; } = string.Empty; // Initialize with a default value to avoid CS8618

        [Required(ErrorMessage = "Town or city is required.")]
        public string AddressTown { get; set; } = string.Empty; // Initialize with a default value to avoid CS8618

        public string AddressCounty { get; set; } = string.Empty; // Initialize with a default value to avoid CS8618

        [Required(ErrorMessage = "Postcode is required.")]
        public string Postcode { get; set; } = string.Empty; // Initialize with a default value to avoid CS8618

        public string Fulladdress { get; set; } = string.Empty; // Initialize with a default value to avoid CS8618
    }
}
