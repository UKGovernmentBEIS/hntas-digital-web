using HNTAS.Api.Client.Model;

namespace HNTAS.Web.UI.Models
{
    public class HeatNetworksViewModel
    {
        public List<HeatNetworkModel> HeatNetworks { get; set; } = new List<HeatNetworkModel>();

        public bool IsResponsiblePerson { get; set; }

        public bool IsHntasCoordinator { get; set; }

        // Pagination & Sorting properties
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public int TotalCount { get; set; }
        public int TotalPages { get; set; }
        public string SortBy { get; set; } = "Name";
        public string SortDirection { get; set; } = "asc";
        public bool HasPreviousPage => PageNumber > 1;
        public bool HasNextPage => PageNumber < TotalPages;
    }

    public class HeatNetworkModel
    {
        public string HnId { get; set; }
        public string Name { get; set; }
        public string? HnDescription { get; set; }
        public string OrganisationName { get; set; }
        public string Role { get; set; }
    }
}
