using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace HNTAS.Web.UI.Models
{
    public class RunningAHNViewModel
    {
        [Required(ErrorMessage = "Please select yes or no.")]
        public bool? IsRunningHeatNetwork { get; set; }
    }

    public class ServesGt10DwellingsViewModel
    {
        [Required(ErrorMessage = "Please select yes or no.")]
        public bool? ServesMoreThan10Dwellings { get; set; }
    }

    public class LocatedInUkViewModel
    {
        [Required(ErrorMessage = "Please select yes or no.")]
        public bool? IsInUK { get; set; }
    }

    public class OperatingAHNViewModel
    {
        [Required(ErrorMessage = "Please select yes or no.")]
        public bool? IsExistingOrPlanned { get; set; }
    }
}