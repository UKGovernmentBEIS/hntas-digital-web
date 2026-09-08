using HNTAS.Api.Client.Model;
using System.ComponentModel.DataAnnotations;

namespace HNTAS.Web.UI.Models.Components
{
    public class StatusTag
    {
        public string Text { get; set; }
        public string CssClass { get; set; }        
    }

    public class InvitationStatusTag : StatusTag
    {        
        public InvitationStatusTag([AllowedValues("invited", "active", "pending", "inactive", "rejected")]  string text)
        {
            Text = text;
            switch (text.ToLower())
            {
                case "invited":
                    CssClass = "govuk-tag--blue";
                    break;
                case "active":
                    CssClass = "govuk-tag--green";
                    break;
                case "pending":
                    CssClass = "govuk-tag--yellow";
                    break;
                case "inactive":
                case "rejected":
                    CssClass = "govuk-tag--red";
                    break;
                default:
                    CssClass = "govuk-tag--grey"; // Default for unknown status
                    break;
            }
        }
    }
}
