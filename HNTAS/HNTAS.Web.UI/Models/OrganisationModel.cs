using HNTAS.Web.UI.Models.CompaniesHouse;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace HNTAS.Web.UI.Models
{
    public class OrganisationModel
    {
       

        // This list will hold the display text and value for each radio button
        public List<SelectListItem> OrganisationTypes { get; set; } = new List<SelectListItem>();

        [Required(ErrorMessage = "Select which type of organisation you work for")]
        public string? SelectedOrganisationType { get; set; }
        public string? SelectedOrganisationTypeText { get; set; }


        [Required(ErrorMessage = "Enter your company number")]
        public string? CompanyNumber { get; set; }

        // Property to hold the company details, including the registered office address, from the confirmation screen
        public CompanyDetailsModel? CompanyDetails { get; set; } = null;
    }
}
