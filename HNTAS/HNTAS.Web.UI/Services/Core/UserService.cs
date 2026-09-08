using HNTAS.Api.Client.Api;
using HNTAS.Api.Client.Model;
using HNTAS.Web.UI.Extensions;

namespace HNTAS.Web.UI.Services.Core
{
    public class UserService : IUserService
    {
        private readonly IUsersApi _usersApi;
        private readonly IHeatNetworksApi _heatNetworksApi;
        private readonly ILogger<UserService> _logger;
        private readonly ISuperUserApi _superUserApi;

        public UserService(IUsersApi usersApi, ILogger<UserService> logger, IHeatNetworksApi heatNetworksApi, ISuperUserApi superUserApi)
        {
            _usersApi = usersApi;
            _logger = logger;
            _heatNetworksApi = heatNetworksApi;
            _superUserApi = superUserApi;
        }

        public async Task<UserResponse?> GetUserById(string id)
        {
            _logger.LogInformation("Retrieving user by OneLogin ID: {OneLoginId}", id);

            try
            {
                var userResponse = await _usersApi.GetUserByIdAsync(id);

                if (userResponse.IsOk)
                {
                    return userResponse.Ok();
                }
                else if (userResponse.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    return null;
                }

                throw new Exception($"Failed to retrieve user with status code: {userResponse.StatusCode}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user by Id: {Id}", id);
                throw;
            }
        }

        public async Task<UserResponse?> GetUserByOneLoginId(string oneLoginId)
        {
            _logger.LogInformation("Retrieving user by OneLogin ID: {OneLoginId}", oneLoginId);

            try
            {
                var userResponse = await _usersApi.GetUserByOneLoginIdAsync(oneLoginId);

                if (userResponse.IsOk)
                {
                    return userResponse.Ok();
                }
                else if (userResponse.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    return null;
                }

                throw new Exception($"Failed to retrieve user with status code: {userResponse.StatusCode}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user by OneLogin ID: {OneLoginId}", oneLoginId);
                throw;
            }
        }

        public async Task<UserResponse?> GetUserByEmailIdAsync(string emailId)
        {
            _logger.LogInformation("Retrieving user by email ID");

            try
            {
                var userResponse = await _usersApi.ApiUsersEmailEmailIdGetAsync(emailId);

                if (userResponse.IsOk)
                {
                    return userResponse.Ok();
                }
                else if (userResponse.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    return null;
                }

                throw new Exception($"Failed to retrieve user with status code: {userResponse.StatusCode}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user by email ID");
                throw;
            }
        }


        public async Task<string?> CreateUser(InitialUserRegistrationRequest request)
        {
            try
            {
                _logger.LogInformation("Creating user with OneLogin ID: {OneLoginId}", request.OneLoginId);

                var response = await _usersApi.ApiUsersInitialEntryPostAsync(request);

                if (response.IsCreated)
                {
                    // Check if the response indicates a successful creation
                    _logger.LogInformation("User created successfully with ID: {UserId}", response.Created());
                    return response.Created();
                }

                throw new Exception($"Failed to create user with status code: {response.StatusCode}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error logging user creation for OneLogin ID: {OneLoginId}", request.OneLoginId);
                throw;
            }
        }

        public async Task<string?> UpdateUserOrganisation(string id, UpdateUserOrganisationRequest request)
        {
            _logger.LogInformation("Updating user organisation for ID: {UserId}", id);
            try
            {
                var response = await _usersApi.ApiUsersIdOrgDetailsPatchAsync(id, request);

                if (response.IsOk)
                {
                    return response.Ok()?.OrgId;
                }

                throw new Exception($"Failed to create user with status code: {response.StatusCode}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error logging user creation for user ID: {userId}", id);
                throw;
            }
        }

        public async Task UpdateUserWithExistingOrganisationId(string userId, string orgId)
        {
            var response = await _usersApi.ApiUsersUpdateOrgidPatchAsync(new UpdateUserOrgIdRequest(userId, orgId));

            if (response.IsNoContent)
            {
                _logger.LogInformation("User organisation ID updated successfully for user ID: {userId}", userId);
                return;
            }
            throw new Exception($"Failed to update user organisation ID with status code: {response.StatusCode}");
        }

        public async Task<Organisation?> UpdateOrganisationLinkUser(string userId, OrganisationRequest organisationRequest)
        {
            _logger.LogInformation("Updating organisation link for user ID: {UserId}", userId);
            try
            {
                var response = await _usersApi.ApiUsersRegisterOrgAndLinkUserIdPostAsync(userId, organisationRequest);
                if (response.IsCreated)
                {
                    _logger.LogInformation("Organisation link updated successfully for user ID: {UserId}", userId);
                    return response.Created();
                }
                throw new Exception($"Failed to update organisation link with status code: {response.StatusCode}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating organisation link for user ID: {UserId}", userId);
                throw;
            }
        }


        public async Task UpdateUserDetails(string id, UpdateUserDetailsRequest request)
        {
            _logger.LogInformation("Attempting to update user details for ID: {UserId}", id);
            try
            {
                var response = await _usersApi.ApiUsersIdUserDetailsPatchAsync(id, request);

                if (response.IsNoContent)
                {
                    _logger.LogInformation("Successfully updated user details for ID: {UserId}. Status: 204 No Content.", id);
                    return;
                }

                throw new Exception($"Failed to update user details for ID '{id}'. Status Code: {response.StatusCode}.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "A critical error occurred during user details update for user ID: {userId}", id);
                throw;
            }
        }



        public async Task<bool?> IsOrganisationExists(string companiesHouseNumber)
        {
            _logger.LogInformation("Checking if organisation with Companies House number {CompaniesHouseNumber} has RP user", companiesHouseNumber);
            try
            {
                var response = await _usersApi.ApiUsersOrganisationExistsGetAsync(companiesHouseNumber);
                if (response.IsOk)
                {
                    return response.Ok();
                }
                throw new Exception($"Failed to check organisation with status code: {response.StatusCode}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if organisation has RP user for Companies House number: {CompaniesHouseNumber}", companiesHouseNumber);
                throw;
            }
        }

        public async Task<List<HeatNetworkUserResponse>?> GetUserHeatNetworks(string id)
        {
            var user = await GetUserDetails(id);

            return user.HeatNetworks;

        }

        public async Task<List<EnumItemResponse>> GetContributorRolesAsync()
        {
            var response = await _usersApi.ApiUsersContributorRolesGetAsync();

            if (response.IsOk)
            {
                return response.Ok();
            }
            else
            {
                _logger.LogError("Failed to retrieve contributor roles with status code: {StatusCode}", response.StatusCode);
                throw new Exception($"Failed to retrieve contributor roles with status code: {response.StatusCode}");
            }

        }

        public async Task<List<EnumItemResponse>> GetUserRolesAsync()
        {
            var response = await _usersApi.ApiUsersUserRolesGetAsync();

            if (response.IsOk)
            {
                return response.Ok();
            }
            else
            {
                _logger.LogError("Failed to retrieve contributor roles with status code: {StatusCode}", response.StatusCode);
                throw new Exception($"Failed to retrieve contributor roles with status code: {response.StatusCode}");
            }
        }


        public async Task<UserDetailsResponse> GetUserDetails(string userId)
        {

            _logger.LogInformation("Getting user details for user ID: {UserId}", userId);
            if (string.IsNullOrWhiteSpace(userId))
            {
                _logger.LogError("User ID is null or empty");
                throw new ArgumentNullException(nameof(userId), "User ID cannot be null or empty");
            }

            try
            {
                var user = await _usersApi.ApiUsersUserDetailsByIdGetAsync(userId);
                if (user.IsOk)
                {
                    return user.Ok();
                }
                throw new Exception($"Failed to get user details with status code: {user.StatusCode}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user details for user ID: {UserId}", userId);
                throw;
            }
        }

        public async Task<List<ManagedUserResponse>> GetManagedUsers(string userId, bool networkManagersOnly = false)
        {
            _logger.LogInformation("Getting managed users for user ID: {UserId}", userId);
            if (string.IsNullOrWhiteSpace(userId))
            {
                _logger.LogError("User ID is null or empty");
                throw new ArgumentNullException(nameof(userId), "User ID cannot be null or empty");
            }
            try
            {
                var users = await _usersApi.ApiUsersManagedUsersGetAsync(userId, networkManagersOnly);
                if (users.IsOk)
                {
                    return users.Ok();
                }
                throw new Exception($"Failed to get managed users with status code: {users.StatusCode}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting managed users for user ID: {UserId}", userId);
                throw;
            }
        }

        public async Task<List<InvitedUserResponse>> GetNetworkLeads(string userId)
        {
            _logger.LogInformation("Getting network leads for user ID: {UserId}", userId);
            if (string.IsNullOrWhiteSpace(userId))
            {
                _logger.LogError("User ID is null or empty");
                throw new ArgumentNullException(nameof(userId), "User ID cannot be null or empty");
            }
            try
            {
                var users = await _usersApi.ApiUsersNetworkManagersGetAsync(userId);
                if (users.IsOk)
                {
                    return users.Ok();
                }
                throw new Exception($"Failed to get network leads with status code: {users.StatusCode}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting network leads for user ID: {UserId}", userId);
                throw;
            }
        }



        public async Task<List<UserResponse>> GetRegisteredUsersAsync(string rpUserId)
        {
            var users = await _usersApi.ApiUsersRegisteredUsersGetAsync(rpUserId);
            if (users.IsOk)
            {
                return users.Ok();
            }

            _logger.LogError("Failed to retrieve registered users with status code: {StatusCode}", users.StatusCode);
            throw new Exception($"Failed to retrieve registered users with status code: {users.StatusCode}");

        }

        public async Task<bool?> IsRpUserAsync(string emailId)
        {
            var users = await _usersApi.ApiUsersIsRpUserEmailIdGetAsync(emailId);
            if (users.IsOk)
            {
                return users.Ok();
            }
            else if (users.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return null;
            }
            var errorMessage = $"Unable to determine Responsible Party status. API call failed with status code: {users.StatusCode}.";
            _logger.LogError(errorMessage);
            throw new Exception(errorMessage);
        }


        public async Task<bool?> IsActiveUserAsync(string emailId)
        {
            var users = await _usersApi.ApiUsersIsActiveUserEmailIdGetAsync(emailId);
            if (users.IsOk)
            {
                return users.Ok();
            }
            else if (users.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return null;
            }
            var errorMessage = $"Unable to determine Responsible Party status. API call failed with status code: {users.StatusCode}.";
            _logger.LogError(errorMessage);
            throw new Exception(errorMessage);
        }

        public async Task<List<UserRoleDetailResponse>> GetHeatNetworkUserRoles(string heatNetworkId)
        {
            _logger.LogInformation("Retrieving user roles for heat network ID: {HeatNetworkId}", heatNetworkId);
            try
            {
                var response = await _usersApi.ApiUsersHeatNetworkHnIdRolesGetAsync(heatNetworkId);
                if (response.IsOk)
                {
                    return response.Ok();
                }
                if (response.IsNotFound)
                {
                    return null;
                }
                throw new Exception($"Failed to retrieve user roles with status code: {response.StatusCode}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user roles for heat network ID: {HeatNetworkId}", heatNetworkId);
                throw;
            }
        }


        public async Task<(bool IsAssigned, string UserId)> IsRoleAlreadyAssigned(string heatNetworkId, string roleName)
        {
            var userRolesDetailsResponse = await GetHeatNetworkUserRoles(heatNetworkId.ToUpper());
            //check the role is present in the list or not
            if (userRolesDetailsResponse != null && userRolesDetailsResponse.Any())
            {
                var existingRole = userRolesDetailsResponse.FirstOrDefault(x => x.RoleDescription.Equals(roleName, StringComparison.OrdinalIgnoreCase));
                if (existingRole != null)
                {
                    return (true, existingRole.UserId);
                }
            }
            return (false, string.Empty);
        }

        public async Task<List<UserResponse>> GetUsersByOrganisationIdAsync(string organisationId)
        {
            var usersResponse = await _usersApi.ApiUsersOrganisationOrganisationIdGetAsync(organisationId);
            if (usersResponse.IsOk)
            {
                return usersResponse.Ok();
            }
            _logger.LogError("Failed to retrieve users by organisation ID with status code: {StatusCode}", usersResponse.StatusCode);
            throw new Exception($"Failed to retrieve users by organisation ID with status code: {usersResponse.StatusCode}");
        }        

        public async Task<bool> IsSuperUser(string emailId)
        {
            var response = await _superUserApi.ApiSuperUserIsSuperUserEmailIdGetAsync(emailId);
            if (response.IsOk)
            {
                return response.Ok() ?? false;
            }
            _logger.LogError("An unexpected error occurred while checking super user status");
            throw new Exception($"Failed to retrieve super user with status code: {response.StatusCode}");
        }
    }
}


