using HNTAS.Web.UI.Models.Common;
using System.ComponentModel.DataAnnotations;

namespace HNTAS.Web.UI.Models.Contributors
{
    public class NewContributorHeatNetworkViewModel
    {
        [Required(ErrorMessage = "Select a heat network.")]
        public string SelectedHeatNetwork { get; set; } = null!;
        public List<SelectItemOption> HeatNetworks { get; set; } = new();
    }
}
