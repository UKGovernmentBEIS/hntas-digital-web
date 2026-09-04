using HNTAS.Web.UI.Models.Address;
using HNTAS.Web.UI.Models.Common;
using HNTAS.Web.UI.Models.HeatNetworkRegistration;
using System.ComponentModel.DataAnnotations;

namespace HNTAS.Web.UI.Models
{
    public class HeatNetworkOrganisationModel
    {
        public List<SelectItemOption> OrganisationList { get; set; } = new List<SelectItemOption>();
        [Required(ErrorMessage = "Select an organisation to add the network")]
        public string SelectedOrganisation { get; set; }
    }

    public class HeatNetworkNameModel
    {
        private string _heatNetworkName;

        [Required(ErrorMessage = "Please enter the heat network name.")]
        [StringLength(100, ErrorMessage = "The heat network name cannot exceed 100 characters.")]
        [RegularExpression(@"^[A-Za-z0-9 :;\-]+$", ErrorMessage = "The heat network name contains invalid characters.")]
        [Display(Name = "HeatNetwork Name")]
        public string HeatNetworkName
        {
            get => _heatNetworkName;
            set => _heatNetworkName = value?.Trim();
        }

        [RegularExpression(@"^[^<>]*$", ErrorMessage = "Additional description must not include < or >")]
        public string? AdditionalDescription
        {
            get => _additionalDescription;
            set => _additionalDescription = string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        }

        private string? _additionalDescription;
    }

    public class HeatNetworkLocationModel
    {   
        public AddressByStreetOrTownModel? HNAddressByStreet { get; set; }               
    }

    public class ECDetailsModel
    {
        [Required(ErrorMessage = "Enter the latitude and longitude")]
        public string LatitudeLongitude { get; set; }
        public AddressByLatLongModel ECAddressByLatLong { get; set; } = new AddressByLatLongModel();
    }

    public class HeatNetworkPhaseModel
    {
        [Required(ErrorMessage = "Select the phase your heat network is in")]
        public string HeatNetworkPhase { get; set; }
    }   

    public class PathwayModel
    {
        public string Pathway { get; set; }
    }

    public class CheckYourAnswersHeatNetworkModel
    {
        public string DoesHnHaveMoreThan6Dwellings { get; set; }
        public string? OrgId { get; set; }
        public string HeatNetworkType { get; set; }
        public string HasOwnEnergyCenter { get; set; }
        public string ECSuppliesOneCommunalBuilding { get; set; }
        public HeatNetworkConnectionsViewModel? HeatNetworkConnectionsModel { get; set; }
        public ECDetailsModel ECDetailsModel { get; set; }
        public HeatNetworkNameModel HeatNetworkNameModel { get; set; }
        public AddressByStreetOrTownModel? HeatNetworkAddressModel { get; set; }        
        public HeatNetworkPhaseModel HeatNetworkPhaseModel { get; set; }     
        public PathwayModel PathwayModel { get; set; }

        // The ConfirmedDeclaration property, now part of this specific ViewModel
        [Display(Name = "I confirm that")]
        [Range(typeof(bool), "true", "true", ErrorMessage = "You must confirm the declaration to proceed.")]
        public bool ConfirmedDeclaration { get; set; }
    }

    public class HeatNetworkSuccessRedirection
    {
        public string NextAction { get; set; }
    }

}
