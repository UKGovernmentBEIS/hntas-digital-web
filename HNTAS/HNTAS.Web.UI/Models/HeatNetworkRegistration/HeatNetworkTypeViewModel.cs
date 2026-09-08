using System.ComponentModel.DataAnnotations;

namespace HNTAS.Web.UI.Models.HeatNetworkRegistration
{
    public class IsHnTypeCommunalViewModel
    {
        [Required(ErrorMessage = "Select the type of network you are registering")]
        public bool? IsHnTypeCommunal { get; set; }
    }

    public class DoesCommunalHnHaveOwnEcViewModel
    {
        [Required(ErrorMessage = "Select if this heat network has its own energy centre")]
        public bool? HasOwnEc { get; set; }
    }

    public class DoesDistrictHnHaveOwnEcViewModel
    {
        [Required(ErrorMessage = "Select if this heat network has its own main energy centre")]
        public bool? HasOwnEc { get; set; }
    }

    public class DoesCommunalEcSupplyOneBlockViewModel
    {
        [Required(ErrorMessage = "Select if the energy centre only supply this one communal building")]
        public bool? SuppliesOneBlock { get; set; }
    }

}