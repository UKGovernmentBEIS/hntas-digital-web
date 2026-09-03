using HNTAS.Api.Client.Model;
using HNTAS.Web.UI.Models.NetworkElements;
using Microsoft.AspNetCore.Mvc;

namespace HNTAS.Web.UI.Services.Core
{
    public interface IHeatNetworkService
    {
        Task<HeatNetworkResponse?> GetAsync(string hnId);
        Task<HeatNetworkResponse> AddHeatNetwork(HeatNetwork heatNetwork);
        Task<List<HeatNetworkResponse>> GetAllHeatNetworks();
        Task<HeatNetworkResponse> UpdateNetworkElements(string hnId, NetworkElements2 request);
        Task<List<HeatNetworkResponse>> GetHeatNetworkByUserId(string userId, RegistrationSource2 registrationSource = RegistrationSource2.HNTAS);
        Task<ExistingNetworkResponse> GetExistingNetworkByUserId(ExistingNetworkRequest request);
        Task<HeatNetworkResponse> RegisterOfgemNetwork(HeatNetwork heatNetwork);

        Task<PagedResultOfHeatNetworkResponse> GetHeatNetworkByUserIdPaginatedAsync(
            string userId,
            RegistrationSource2 registrationSource = RegistrationSource2.HNTAS,
            int pageNumber = 1,
            int pageSize = 10,
            string sortBy = "Name",
            string sortDirection = "asc",
            CancellationToken cancellationToken = default);
    }
}
