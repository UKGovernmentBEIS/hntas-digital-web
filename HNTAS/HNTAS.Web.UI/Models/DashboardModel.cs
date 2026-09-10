using HNTAS.Api.Client.Model;

namespace HNTAS.Web.UI.Models
{
    public class DashboardModel
    {
        public string OrganisationName { get; set; } = null!;
        public bool IsResponsiblePerson { get; set; }
        public List<UserRole> UserRoles { get; set; } = null!;

        public bool HasHntasNetworks { get; set; }
        public bool HasOfgemNetworks { get; set; }
    }
}
