using HNTAS.Web.UI.Models.CompaniesHouse;
using System.ComponentModel.DataAnnotations;

namespace HNTAS.Web.UI.Models
{
    public class OrganisationModel
    {
        [Required(ErrorMessage = "Select which type of organisation you work for")]
        public string SelectedOrganisationType { get; set; }


        [Required(ErrorMessage = "Enter your company number")]
        public string CompanyNumber { get; set; }

        // Property to hold the company details, including the registered office address, from the confirmation screen
        public CompanyDetailsModel? CompanyDetails { get; set; } = null;
    }
}
