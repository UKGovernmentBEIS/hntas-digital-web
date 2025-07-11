namespace HNTAS.Web.UI.Models
{
    public class HeatNetworkEligibilityModel
    {
        public int CurrentStep { get; set; } = 1;
        public bool? IsRunningHeatNetwork { get; set; }
        public bool? ServesMoreThan10Dwellings { get; set; }
        public bool? IsInUK { get; set; }
        public bool? IsExistingOrPlanned { get; set; }
        public string ResultMessage { get; set; }
    }
}
