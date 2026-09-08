using HNTAS.Api.Client.Api;
using HNTAS.Api.Client.Client;
using HNTAS.Api.Client.Model;

namespace HNTAS.Web.UI.Services.Core
{
    public class HeatNetworkService : IHeatNetworkService
    {
        private readonly ILogger<UserService> _logger;
        private readonly IHeatNetworksApi _heatNetworksApi;

        public HeatNetworkService(ILogger<UserService> logger, IHeatNetworksApi heatNetworksApi)
        {
            _logger = logger;
            _heatNetworksApi = heatNetworksApi;
        }

        public async Task<HeatNetworkResponse?> GetAsync(string hnId)
        {
            // _logger.LogInformation("Fetching heat network with ID: {HnId}", hnId);

            var response = await _heatNetworksApi.ApiHeatNetworksHnIdGetAsync(hnId);

            if (response.IsOk)
            {
                //_logger.LogInformation("Successfully retrieved heat network: {HnId}", hnId);
                return response.Ok();
            }
            else if (response.IsNotFound)
            {
                //_logger.LogWarning("Heat network with ID: {HnId} not found", hnId);
                return null;
            }

            //_logger.LogError("Failed to fetch heat network {HnId}. Status code: {StatusCode}", hnId, response.StatusCode);
            throw new Exception($"Failed to fetch heat network '{hnId}' — status code: {response.StatusCode}");
        }

        public async Task<List<HeatNetworkResponse>> GetHeatNetworkByUserId(string userId, RegistrationSource2 registrationSource = RegistrationSource2.HNTAS)
        {
            try
            {
                var response = await _heatNetworksApi.ApiHeatNetworksHeatNetworkByUserIdGetAsync(userId, registrationSource);

                if (response.IsOk)
                {
                    var networks = response.Ok();
                    _logger.LogInformation("Retrieved {Count} heat networks for user ID: {UserId}.", networks.Count, userId);
                    return networks;
                }
                else
                {
                    return new List<HeatNetworkResponse>();
                }

                throw new InvalidOperationException($"Failed to retrieve heat networks for user ID: {userId}. Status code: {response.StatusCode}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving heat networks for user ID: {UserId}.", userId);
                throw;
            }
        }


        public async Task<PagedResultOfUserNetworkDetailsResponse> GetHeatNetworkByUserIdPaginatedAsync(
            string userId,
            RegistrationSource2 registrationSource = RegistrationSource2.HNTAS,
            int pageNumber = 1,
            int pageSize = 10,
            string sortBy = "Name",
            string sortDirection = "asc",
            CancellationToken cancellationToken = default)
        {
            try
            {
                var response = await _heatNetworksApi.ApiHeatNetworksHeatNetworkByUserIdPaginatedGetAsync(
                    userId: new Option<string>(userId),
                    registrationSource: new Option<RegistrationSource2>(registrationSource),
                    pageNumber: new Option<int>(pageNumber),
                    pageSize: new Option<int>(pageSize),
                    sortBy: new Option<string>(sortBy),
                    sortDirection: new Option<string>(sortDirection),
                    cancellationToken: cancellationToken);

                if (response.IsOk)
                {
                    var pagedResult = response.Ok();
                    _logger.LogInformation("Retrieved {Count} heat networks for user ID: {UserId}.", pagedResult.Items?.Count ?? 0, userId);
                    return pagedResult;
                }

                _logger.LogWarning("Failed to retrieve heat networks for user ID: {UserId}. Status code: {StatusCode}", userId, response.StatusCode);
                return new PagedResultOfUserNetworkDetailsResponse();
            }
            catch (ApiException ex)
            {
                _logger.LogError(ex, "API call failed while retrieving heat networks for user ID: {UserId}.", userId);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error retrieving heat networks for user ID: {UserId}.", userId);
                throw;
            }
        }

        public async Task<ExistingNetworkResponse> GetExistingNetworkByUserId(ExistingNetworkRequest request)
        {

            if (string.IsNullOrWhiteSpace(request.UserId))
            {
                throw new ArgumentException("User ID cannot be null or empty.", nameof(request.UserId));
            }

            var response = await _heatNetworksApi.ApiHeatNetworksExistingNetworkByUserIdGetAsync(request);

            if (response.IsNotFound)
            {
                _logger.LogWarning("No existing network found");
                return new ExistingNetworkResponse();
            }

            if (!response.IsOk)
            {
                _logger.LogError("Failed to fetch existing network. Status code: {StatusCode}", response.StatusCode);
                throw new HttpRequestException($"Failed to fetch existing network. Service returned {response.StatusCode}");
            }

            return response.Ok() ?? new ExistingNetworkResponse();
        }


        public async Task<HeatNetworkResponse> AddHeatNetwork(HeatNetwork heatNetwork)
        {
            // Implementation for adding a heat network
            // _logger.LogInformation("Adding heat network: {HeatNetworkName}", heatNetwork.Name);
            try
            {
                var response = await _heatNetworksApi.ApiHeatNetworksAddHeatNetworkPostAsync(heatNetwork);

                if (response.IsCreated)
                {
                    //_logger.LogInformation("Heat network created successfully with ID: {Id}", heatNetwork.Id);
                    return response.Created();
                }
                throw new Exception($"Failed to add heat network with status code: {response.StatusCode}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error submitting heat network answers.");
                throw;
            }
        }

        public async Task<HeatNetworkResponse> RegisterOfgemNetwork(HeatNetwork heatNetwork)
        {            
            try
            {
                var response = await _heatNetworksApi.ApiHeatNetworksRegisterOfgemNetworkPutAsync(heatNetwork);

                if (response.IsOk)
                {                    
                    return response.Ok()!;
                }
                throw new Exception($"Failed to register Ofgem network with status code: {response.StatusCode}");
            }
            catch (Exception ex)
            {   
                _logger.LogError(ex, "Error submitting Ofgem network registration.");
                throw;
            }
        }

        public async Task<HeatNetworkResponse> UpdateNetworkElements(string hnId, NetworkElements2 request)
        {
            try
            {
                var response = await _heatNetworksApi.ApiHeatNetworksNetworkElementsPutAsync(request, hnId);
                if (response.IsOk)
                {
                    var updatedNetwork = response.Ok();
                    _logger.LogInformation("Updated network elements for heat network ID: {HnId}", hnId);
                    return updatedNetwork;
                }
                throw new Exception($"Failed to update network elements for heat network '{hnId}' with status code: {response.StatusCode}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating network elements for heat network ID: {HnId}", hnId);
                throw;
            }
        }
        public async Task<List<HeatNetworkResponse>> GetAllHeatNetworks()
        {
            try
            {
                var response = await _heatNetworksApi.ApiHeatNetworksGetAsync();

                if (response.IsOk)
                {
                    var networks = response.Ok();
                    _logger.LogInformation("Retrieved {Count} heat networks.", networks.Count);
                    return networks;
                }

                throw new InvalidOperationException($"Failed to retrieve heat networks. Status code: {response.StatusCode}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving heat networks.");
                throw;
            }
        }        
    }
}
