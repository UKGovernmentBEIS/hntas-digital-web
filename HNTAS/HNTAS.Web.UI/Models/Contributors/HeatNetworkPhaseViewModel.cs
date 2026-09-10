using HNTAS.Web.UI.CustomValidation;
using HNTAS.Web.UI.Models.Common;

namespace HNTAS.Web.UI.Models.Contributors
{
    public class HeatNetworkPhaseViewModel
    {
        public List<SelectItemOption> Phases { get; set; } = new List<SelectItemOption>();

        [MustHaveOneItem(ErrorMessage = "Select at least one phase")]
        public List<string>? SelectedPhases { get; set; } = new List<string>();
    }
}
