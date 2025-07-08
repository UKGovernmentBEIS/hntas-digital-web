using System.ComponentModel.DataAnnotations;

namespace HNTAS.Web.UI.Models.Address
{
    public class NoPostcodeAddressModel
    {
        [Required(ErrorMessage = "Please enter the url.")]
        public string what3wordsUrl { get; set; }
    }
}