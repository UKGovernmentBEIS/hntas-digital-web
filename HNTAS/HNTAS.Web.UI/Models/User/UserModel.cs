using System.ComponentModel.DataAnnotations;

namespace HNTAS.Web.UI.Models.User
{
    public class UserModel
    {
        [Required(ErrorMessage = "Select yes if you are the regulatory contact")]
        public bool? ifRPisRC { get; set; }

        // Add this property if you want the organisation name to be dynamic
        public string OrganisationName { get; set; } = string.Empty;

        public ContactDetailsModel ContactDetails { get; set; } = new ContactDetailsModel();
    }
}
