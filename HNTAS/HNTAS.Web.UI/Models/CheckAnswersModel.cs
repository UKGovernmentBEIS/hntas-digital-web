using HNTAS.Web.UI.Models.User;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.ComponentModel.DataAnnotations;

namespace HNTAS.Web.UI.Models
{
    public class CheckAnswersModel
    {
        // Use [BindNever] to prevent the model binder from attempting to bind these
        // properties from the incoming form data on a POST request.
        // Properties to hold the data from your session models
        [BindNever]
        public OrganisationModel Organisation { get; set; }
        [BindNever]
        public UserModel User { get; set; }

        // The ConfirmedDeclaration property, now part of this specific ViewModel
        [Display(Name = "I confirm that")]
        [Range(typeof(bool), "true", "true", ErrorMessage = "You must confirm the declaration to proceed.")]
        public bool ConfirmedDeclaration { get; set; }
    }
}
