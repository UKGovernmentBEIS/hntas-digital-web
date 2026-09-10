using HNTAS.Api.Client.Model;

namespace HNTAS.Web.UI.Services.Core
{
    public interface IHeatNetworkService
    {
        Task<HeatNetworkResponse?> GetAsync(string hnId);
        Task<HeatNetworkResponse> AddHeatNetwork(HeatNetwork heatNetwork);
        Task<List<HeatNetworkResponse>> GetAllHeatNetworks();
        Task<HeatNetworkResponse> UpdateNetworkElements(string hnId, NetworkElements2 request);
        Task<ExistingNetworkResponse> GetExistingNetworkByUserId(ExistingNetworkRequest request);
        Task<HeatNetworkResponse> RegisterOfgemNetwork(HeatNetwork heatNetwork);

        Task<PagedResultOfUserNetworkDetailsResponse> GetHeatNetworkByUserIdPaginatedAsync(
            string userId,
            RegistrationSource2 registrationSource = RegistrationSource2.HNTAS,
            int pageNumber = 1,
            int pageSize = 10,
            string sortBy = "Name",
            string sortDirection = "asc",
            CancellationToken cancellationToken = default);
    }
}
