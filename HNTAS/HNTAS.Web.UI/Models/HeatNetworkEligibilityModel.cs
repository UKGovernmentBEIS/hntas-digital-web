using System.ComponentModel.DataAnnotations;

namespace HNTAS.Web.UI.Models
{
    public class  AreYouTheRPModel
    {
        [Required(ErrorMessage = "Select an option.")]
        public string? AreYouTheRP { get; set; }
    }
    public class  IsYourOrgWorkingOnANewHNModel
    {
        [Required(ErrorMessage = "Select an option.")]
        public string? IsYourOrgWorkingOnANewHN { get; set; }
    }
    public class  IsHNLocatedInEnglandScotlandWalesModel
    {
        [Required(ErrorMessage = "Select an option.")]
        public string? IsHNLocatedInEnglandScotlandWales { get; set; }
    }  
    public class  HowManyDwellingsIncludedModel
    {
        [Required(ErrorMessage = "Select if the network have or help supply 6 or more dwallings or units.")]
        public string HowManyDwellingsIncluded { get; set; }
    }
}