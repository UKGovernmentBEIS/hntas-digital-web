using System.ComponentModel.DataAnnotations;

namespace HNTAS.Web.UI.Models.Contributors
{
    public class AddContributorViewModel
    {
        [Required(ErrorMessage = "Select how you want to add an eligible contributor")]
        public bool? InviteNewContributor { get; set; }
    }
}
