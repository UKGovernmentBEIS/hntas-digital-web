using HNTAS.Api.Client.Model;
using Microsoft.AspNetCore.Authorization;

namespace HNTAS.Web.UI.Authorization
{
    public static class AuthorizationExtensions
    {
        public static IServiceCollection AddApplicationAuthorization(this IServiceCollection services)
        {
            services.AddScoped<IAuthorizationHandler, RolesHandler>();
            services.AddScoped<IRoleService, RoleService>();

            services.AddAuthorization(options =>
            {
                // Mapping your Role Matrix Image to Policies

                options.AddPolicy(SecurityConstants.Policies.CanStartRegistration, p => p.Requirements.Add(new RolesRequirement()));

                options.AddPolicy(SecurityConstants.Policies.CanAddContributingOrganisation, p => p.Requirements.Add(new RolesRequirement(UserRole.NetworkManager, UserRole.DesignatedDutyHolder, UserRole.Contributor)));

                options.AddPolicy(SecurityConstants.Policies.CanAddHeatNetwork, p => p.Requirements.Add(new RolesRequirement(UserRole.ResponsibleParty, UserRole.NetworkManager)));

                options.AddPolicy(SecurityConstants.Policies.CanAddDDHAndContributor, p => p.Requirements.Add(new RolesRequirement(UserRole.ResponsibleParty, UserRole.NetworkManager, UserRole.DesignatedDutyHolder)));

                options.AddPolicy(SecurityConstants.Policies.CanAddHeatNetworkDetail, p => p.Requirements.Add(new RolesRequirement(UserRole.ResponsibleParty, UserRole.NetworkManager, UserRole.DesignatedDutyHolder, UserRole.Contributor)));

                options.AddPolicy(SecurityConstants.Policies.CanUpdatePersonalDetail, p => p.Requirements.Add(new RolesRequirement(UserRole.ResponsibleParty, UserRole.NetworkManager, UserRole.DesignatedDutyHolder, UserRole.Contributor)));

                options.AddPolicy(SecurityConstants.Policies.CanAssignAssessor, p => p.Requirements.Add(new RolesRequirement(UserRole.ResponsibleParty, UserRole.NetworkManager)));
            });

            return services;
        }
    }
}
