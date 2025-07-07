namespace HNTAS.Web.UI.Models
{
    public class HeatNetworkEligibilityModel
    {
        public int CurrentStep { get; set; } = 1;
        public bool? IsRunningHeatNetwork { get; set; } = true;
        public bool? ServesMoreThan10Dwellings { get; set; } = true;
        public bool? IsInUK { get; set; } = true;
        public bool? IsExistingOrPlanned { get; set; } = true;
        public string ResultMessage { get; set; }
    }
}
