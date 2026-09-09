using System.ComponentModel.DataAnnotations;

namespace HNTAS.Web.UI.Models.Contributors
{
    public class NewContributorRoleViewModel
    {
        [Required(ErrorMessage = "Select who do you want to add")]
        public bool? IsDDH { get; set; }
    }
}
