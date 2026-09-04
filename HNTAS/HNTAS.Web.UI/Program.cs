using Amazon.S3;
using GovUk.OneLogin.AspNetCore;
using HNTAS.Api.Client.Api;
using HNTAS.Api.Client.Client;
using HNTAS.Api.Client.Model;
using HNTAS.Web.UI.Authorization;
using HNTAS.Web.UI.Filters;
using HNTAS.Web.UI.Helpers;
using HNTAS.Web.UI.Routing;
using HNTAS.Web.UI.Services;
using HNTAS.Web.UI.Services.Core;
using HNTAS.Web.UI.Workflows;
using HNTAS.Web.UI.Workflows.Enums;
using HNTAS.Web.UI.Workflows.Models.Data;
using HNTAS.Web.UI.Workflows.Services;
using HNTAS.Web.UI.Workflows.Validation;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.IdentityModel.Tokens;
using System.Diagnostics.CodeAnalysis;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);


if (builder.Environment.EnvironmentName == "Local")
{
    builder.Services.AddDataProtection();
    Console.WriteLine("DataProtection Enabled: " + builder.Environment.EnvironmentName);
}
else
{
    builder.Services.AddDataProtection()
        .PersistKeysToAWSSystemsManager("/HNTAS/DataProtection")
        .SetDefaultKeyLifetime(TimeSpan.FromDays(8));
    Console.WriteLine("DataProtection Enabled: " + builder.Environment.EnvironmentName);
}

// Security fix : Configure HSTS (Strict-Transport-Security) for production environments
builder.Services.AddHsts(options =>
{
    options.Preload = true;
    options.IncludeSubDomains = true;
    options.MaxAge = TimeSpan.FromDays(365); // Standard 1-year duration
});


// Configure RouteOptions
builder.Services.Configure<RouteOptions>(options =>
    {
        options.LowercaseUrls = false;
        options.LowercaseQueryStrings = false; // Optional: for query string parameters too
    });

// Register the parameter transformer globally for controllers/actions
builder.Services.AddControllersWithViews(options =>
{
    options.Conventions.Add(new SlugifiedRouteConvention());
    options.Conventions.Add(new RouteTokenTransformerConvention(new SlugifyParameterTransformer()));
});

builder.Services.AddHttpContextAccessor();

var coreApiBaseUrl = Environment.GetEnvironmentVariable("CORE_BASE_URL") ?? throw new InvalidOperationException("Core API URL is not configured. Set CORE_BASE_URL environment variable.");

// Register CompaniesHouseService with HttpClientFactory
builder.Services.AddSingleton(new JsonSerializerOptions
{

    PropertyNameCaseInsensitive = true, // Common setting for JSON deserialization
    PropertyNamingPolicy = JsonNamingPolicy.CamelCase, // Common setting for JSON serialization
    Converters = {
        new UserJsonConverter(),
        new InitialUserRegistrationRequestJsonConverter(),
        new UserRoleJsonConverter(),
        new UserResponseJsonConverter(),
        new HeatNetworkUserResponseJsonConverter(),
        new EnumItemResponseJsonConverter(),
        new ContributorRoleJsonConverter(),
        new UserDetailsResponseJsonConverter(),
        new OrganisationResponseJsonConverter(),
        new HeatNetworkResponseJsonConverter(),
        new RegisteredAddressJsonConverter(),
        new ManagedUserResponseJsonConverter(),
        new InvitedUserResponseJsonConverter(),
        new HnRoleMappingJsonConverter(),
        new SoaJourneyDataJsonConverter(),
        new NetworkTypeSelectionJsonConverter(),
        new ConnectionTypeJsonConverter(),
        new HeatNetworkElementJsonConverter(),
        new UploadedDocumentJsonConverter(),
        new HeatNetworkResponseJsonConverter(),
        new SoaResponseJsonConverter(),
        new SoaStatusJsonConverter(),
        new SoaJsonConverter(),
        new JourneyDataResponseJsonConverter(),
        new NetworkTypeResponseJsonConverter(),
        new ConnectionTypeJsonConverter(),
        new HeatNetworkElementResponseJsonConverter(),
        new UploadedDocumentResponseJsonConverter(),
        new UploadedAssessmentDocumentResponseJsonConverter(),
        new UploadedAssessorDocumentResponseJsonConverter(),
        new UploadedCertifierDocumentResponseJsonConverter(),
        new HeatNetworkInfoJsonConverter(),
        new CountryAndTerritoryJsonConverter(),
        new UserRoleDetailResponseJsonConverter(),
        new OrganisationJsonConverter(),
        new AssessorSearchResultJsonConverter(),
        new RegisteredAddress2JsonConverter(),
        new ECDetailsJsonConverter(),
        new NetworkElementsResponseJsonConverter(),
        new MeteringAndMonitoringStrategyResponseJsonConverter(),
        new AssessmentPlanResponseJsonConverter(),
        new DesignConstructionLogResponseJsonConverter(),
        new AuditLogResponseJsonConverter(),
        new ElementJsonConverter(),
        new NetworkDetailsUploadedDocumentJsonConverter(),
        new SoaStagesJsonConverter(),
        new HeatNetworkConnectionsJsonConverter(),
        new HeatNetworkTypeJsonConverter(),
        new AuditLogJsonConverter(),
        new ElementSoaAssignAssessorRequestJsonConverter(),
        new SoaAssessorJsonConverter(),
        new NotificationHistoryRequestJsonConverter(),
        new NotificationHistoryResponseJsonConverter(),
        new NotificationHistoryDataJsonConverter(),
        new AssignedAssessorRequestJsonConverter(),
        new AssignedAssessorResponseJsonConverter(),
        new AssignedAssessorJsonConverter(),
        new HeatNetworkDashboardResponseJsonConverter(),
        new HeatNetworkDashboardRowJsonConverter(),
        new HeatNetworkDetailsResponseJsonConverter(),
        new ElementGroupDtoJsonConverter(),
        new KpiDetailDtoJsonConverter(),
        new KpiHistoryResponseJsonConverter(),
        new AggregatedKpiJsonConverter(),
        new SoaStatusWithCountJsonConverter(),
        new ElementGroupJsonConverter(),
        new ExistingNetworkResponseJsonConverter(),
        new CarbonInputUiDisplayJsonConverter(),
        new ImportResultJsonConverter(),
        new ElementSoaAssignAssessorRequestForExistingNetworkJsonConverter(),
        new ElementSoaStatusUpdateRequestForExistingNetworkJsonConverter(),
        new SoaMilestoneJsonConverter(),
        new SoaAssessorExistingNetworkJsonConverter(),
        new SoaStatusWithCountExistingNetworkJsonConverter(),
        new PagedResultOfUserNetworkDetailsResponseJsonConverter(),
        new UserNetworkDetailsResponseJsonConverter()
    }
});
builder.Services.AddSingleton<JsonSerializerOptionsProvider>();

builder.Services.AddSingleton<UsersApiEvents>();
builder.Services.AddHttpClient<IUsersApi, UsersApi>(client =>
{
    client.BaseAddress = new Uri(coreApiBaseUrl);
    client.DefaultRequestHeaders.Add("Accept", "application/json");
});

builder.Services.AddSingleton<OrganisationsApiEvents>();
builder.Services.AddHttpClient<IOrganisationsApi, OrganisationsApi>(client =>
{
    client.BaseAddress = new Uri(coreApiBaseUrl);
    client.DefaultRequestHeaders.Add("Accept", "application/json");
});

builder.Services.AddSingleton<HeatNetworksApiEvents>();
builder.Services.AddHttpClient<IHeatNetworksApi, HeatNetworksApi>(client =>
{
    client.BaseAddress = new Uri(coreApiBaseUrl);
    client.DefaultRequestHeaders.Add("Accept", "application/json");
});

builder.Services.AddSingleton<InvitationsApiEvents>();
builder.Services.AddHttpClient<IInvitationsApi, InvitationsApi>(client =>
{
    client.BaseAddress = new Uri(coreApiBaseUrl);
    client.DefaultRequestHeaders.Add("Accept", "application/json");
});


builder.Services.AddSingleton<SOAApiEvents>();
builder.Services.AddHttpClient<ISOAApi, SOAApi>(client =>
{
    client.BaseAddress = new Uri(coreApiBaseUrl);
    client.DefaultRequestHeaders.Add("Accept", "application/json");
});

builder.Services.AddSingleton<CountriesAndTerritoriesApiEvents>();
builder.Services.AddHttpClient<ICountriesAndTerritoriesApi, CountriesAndTerritoriesApi>(client =>
{
    client.BaseAddress = new Uri(coreApiBaseUrl);
    client.DefaultRequestHeaders.Add("Accept", "application/json");
});

builder.Services.AddSingleton<OrganisationsApiEvents>();
builder.Services.AddHttpClient<IOrganisationsApi, OrganisationsApi>(client =>
{
    client.BaseAddress = new Uri(coreApiBaseUrl);
    client.DefaultRequestHeaders.Add("Accept", "application/json");
});


builder.Services.AddSingleton<OrganisationUserApiEvents>();
builder.Services.AddHttpClient<IOrganisationUserApi, OrganisationUserApi>(client =>
{
    client.BaseAddress = new Uri(coreApiBaseUrl);
    client.DefaultRequestHeaders.Add("Accept", "application/json");
});

builder.Services.AddSingleton<CarbonCalculatorApiEvents>();
builder.Services.AddHttpClient<ICarbonCalculatorApi, CarbonCalculatorApi>(client =>
{
    client.BaseAddress = new Uri(coreApiBaseUrl);
    client.DefaultRequestHeaders.Add("Accept", "application/json");
});

builder.Services.AddSingleton<AssessorApiEvents>();
builder.Services.AddHttpClient<IAssessorApi, AssessorApi>(client =>
{
    client.BaseAddress = new Uri(coreApiBaseUrl);
    client.DefaultRequestHeaders.Add("Accept", "application/json");
});


builder.Services.AddSingleton<AuditApiEvents>();
builder.Services.AddHttpClient<IAuditApi, AuditApi>(client =>
{
    client.BaseAddress = new Uri(coreApiBaseUrl);
    client.DefaultRequestHeaders.Add("Accept", "application/json");
});

builder.Services.AddSingleton<NotificationHistoryApiEvents>();
builder.Services.AddHttpClient<INotificationHistoryApi, NotificationHistoryApi>(client =>
{
    client.BaseAddress = new Uri(coreApiBaseUrl);
    client.DefaultRequestHeaders.Add("Accept", "application/json");
});

builder.Services.AddSingleton<AssignedAssessorApiEvents>();
builder.Services.AddHttpClient<IAssignedAssessorApi, AssignedAssessorApi>(client =>
{
    client.BaseAddress = new Uri(coreApiBaseUrl);
    client.DefaultRequestHeaders.Add("Accept", "application/json");
});


builder.Services.AddSingleton<ArmsDashboardApiEvents>();
builder.Services.AddHttpClient<IArmsDashboardApi, ArmsDashboardApi>(client =>
{
    client.BaseAddress = new Uri(coreApiBaseUrl);
    client.DefaultRequestHeaders.Add("Accept", "application/json");
});

builder.Services.AddSingleton<ImportApiEvents>();
builder.Services.AddHttpClient<IImportApi, ImportApi>(client =>
{
    client.BaseAddress = new Uri(coreApiBaseUrl);
    client.DefaultRequestHeaders.Add("Accept", "text/plain");
});

builder.Services.AddSingleton<SuperUserApiEvents>();
builder.Services.AddHttpClient<ISuperUserApi, SuperUserApi>(client =>
{
    client.BaseAddress = new Uri(coreApiBaseUrl);
    client.DefaultRequestHeaders.Add("Accept", "application/json");
});
builder.Services.AddTransient<FeedbackApiEvents>();
builder.Services.AddHttpClient<IFeedbackApi, FeedbackApi>(client =>
{
    client.BaseAddress = new Uri(coreApiBaseUrl);
});

builder.Services.AddScoped<ISessionHelper, SessionHelper>();

builder.Services.AddScoped<IWorkflowManager, WorkflowManager>();
// Since it's a generic filter, you can register a specific type for each workflow
builder.Services.AddScoped<WorkflowValidationFilter<AddNewContributorWorkflowModel, ContributorWorkflowStep>>();
builder.Services.AddScoped<WorkflowValidationFilter<AddExistingContributorWorkflowModel, ExistingContributorWorkflowStep>>();
builder.Services.AddScoped<WorkflowValidationFilter<AddOrganisationUserWorkflowModel, AddOrganisationUserWorkflowStep>>();
builder.Services.AddScoped<IRedirectResolver<AddNewContributorWorkflowModel, ContributorWorkflowStep>, NewContributorRedirectResolver>();
builder.Services.AddScoped<IRedirectResolver<AddExistingContributorWorkflowModel, ExistingContributorWorkflowStep>, ExistingContributorRedirectResolver>();
builder.Services.AddScoped<IRedirectResolver<AddOrganisationUserWorkflowModel, AddOrganisationUserWorkflowStep>, AddOrganisationUserRedirectResolver>();
builder.Services.AddScoped<IRedirectResolver<AddExistingOrganisationUserWorkflowModel, ExistingOrganisationUserWorkflowStep>, ExistingOrganisationUserRedirectResolver>();

builder.Services.AddScoped<EnsureSessionForOrganisationFlowOnGetAttribute>();
builder.Services.AddScoped<EnsureSessionForOrganisationFlowOnPostAttribute>();

builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IOrganisationService, OrganisationService>();
builder.Services.AddScoped<IInvitationService, InvitationService>();
builder.Services.AddScoped<ISoaService, SoaService>();
builder.Services.AddScoped<IHeatNetworkService, HeatNetworkService>();
builder.Services.AddScoped<ICountriesAndTerritoriesService, CountriesAndTerritoriesService>();
builder.Services.AddScoped<IOrganisationService, OrganisationService>();
builder.Services.AddScoped<IOrganisationUserService, OrganisationUserService>();
builder.Services.AddScoped<ICarbonCalculatorService, CarbonCalculatorService>();
builder.Services.AddHttpClient<ICompaniesHouseService, CompaniesHouseService>();
builder.Services.AddScoped<IAddressLookupService, AddressLookupService>();
builder.Services.AddScoped<IInvitationTokenService, InvitationTokenService>();
builder.Services.AddScoped<IAuditService, AuditService>();
builder.Services.AddScoped<INotificationHistoryService, NotificationHistoryService>();
builder.Services.AddScoped<IAssignedAssessorService, AssignedAssessorService>();
builder.Services.AddScoped<IArmsDashboardService, ArmsDashboardService>();
builder.Services.AddScoped<IImportExistingNetworksService, ImportExistingNetworksService>();
builder.Services.AddSingleton<CertifierEmailGeneratorService>();

builder.Services.AddSingleton<IAmazonS3>(sp =>
{
    var config = sp.GetRequiredService<IConfiguration>();
    return S3ClientHelper.Create(config);
});

builder.Services.AddSingleton(sp =>
{
    var config = sp.GetRequiredService<IConfiguration>();

    var client = new HttpClient();
    client.DefaultRequestHeaders.UserAgent.Clear();
    client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("HNTAS", "1.0"));

    return client;
});

// Decide which authentication to use based on the environment variable
var useGovUkSimulator = Environment.GetEnvironmentVariable("SIMULATOR_PROP4");

if (!string.IsNullOrEmpty(useGovUkSimulator) && useGovUkSimulator.Equals("true", StringComparison.OrdinalIgnoreCase))
{
    // GOV.UK Simulator authentication
    builder.Services.AddAuthentication("GovUkSimulator")
        .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme)
        .AddOpenIdConnect("GovUkSimulator", options =>
        {
            options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            options.Authority = Environment.GetEnvironmentVariable("SIMULATOR_PROP1");
            options.ClientId = Environment.GetEnvironmentVariable("SIMULATOR_PROP2");
            options.CallbackPath = "/onelogin-callback";
            options.RequireHttpsMetadata = false;
            options.SignedOutCallbackPath = "/account/signed-out";
            options.ResponseType = "code";
            options.SaveTokens = true;
            options.Scope.Clear();
            options.Scope.Add("openid");
            options.Scope.Add("email");
            options.ResponseMode = "query";
            options.GetClaimsFromUserInfoEndpoint = true;
            // Add more options as needed for your simulator
            //options.TokenEndpointAuthenticationMethod = OpenIdConnectRedirectBehavior.Post;

            options.Events = new Microsoft.AspNetCore.Authentication.OpenIdConnect.OpenIdConnectEvents
            {
                OnAuthorizationCodeReceived = async context =>
                {
                    // Load your RSA private key (PEM format from environment variable or file)
                    var privateKeyPem = Environment.GetEnvironmentVariable("SIMULATOR_PROP3");
                    using var rsa = RSA.Create();
                    rsa.ImportFromPem(privateKeyPem.Replace("\\n", "\n"));

                    var now = DateTime.UtcNow;
                    var clientId = options.ClientId; // This must match your OIDC client_id

                    var handler = new JwtSecurityTokenHandler();
                    var tokenDescriptor = new SecurityTokenDescriptor
                    {
                        Issuer = clientId,
                        Subject = new ClaimsIdentity(new[]
                        {
                            new Claim("sub", clientId ?? string.Empty), // Must match client_id
                            new Claim("jti", Guid.NewGuid().ToString()) // Required unique JWT ID
                        }),
                        Audience = options.Authority.TrimEnd('/') + "/token",
                        Expires = now.AddMinutes(5),
                        SigningCredentials = new SigningCredentials(
                            new RsaSecurityKey(rsa.ExportParameters(true)),
                            SecurityAlgorithms.RsaSha256)
                        // Do NOT set NotBefore
                    };
                    var jwt = handler.CreateEncodedJwt(tokenDescriptor);

                    context.TokenEndpointRequest.ClientAssertionType = "urn:ietf:params:oauth:client-assertion-type:jwt-bearer";
                    context.TokenEndpointRequest.ClientAssertion = jwt;
                },
                OnTokenValidated = context =>
                {
                    var identity = (ClaimsIdentity)context.Principal.Identity!;
                    // Existing mapping code
                    var email = context.Principal.FindFirst("email")?.Value;
                    if (!string.IsNullOrEmpty(email))
                        identity.AddClaim(new Claim(ClaimTypes.Email, email));
                    var sub = context.Principal.FindFirst("sub")?.Value;
                    if (!string.IsNullOrEmpty(sub))
                        identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, sub));
                    return Task.CompletedTask;
                }
            };
        });

}
else
{
    // GOV.UK One Login authentication
    builder.Services.AddAuthentication(defaultScheme: OneLoginDefaults.AuthenticationScheme)
        .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
        {
            options.AccessDeniedPath = "/account/access-denied"; // The browser will go here for 403s
        })
        .AddOneLogin(options =>
        {
            options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            options.Environment = OneLoginEnvironments.Integration;
            options.ClientId = Environment.GetEnvironmentVariable("ONELOGIN_CLIENT_ID");
            options.CallbackPath = "/onelogin-callback";
            options.SignedOutCallbackPath = "/onelogin-logout-callback";
            options.Scope.Add("openid");
            options.Scope.Add("email");
            options.Scope.Add("phone");
            // ... your existing OneLogin event handlers and configuration ...
            options.Events.OnRedirectToIdentityProvider = context =>
            {
                var invitationId = context.HttpContext.Session.GetString(SessionKeys.InvitationId)?.Trim('"');

                if (!string.IsNullOrWhiteSpace(invitationId))
                {
                    var customState = $"{invitationId}";
                    context.ProtocolMessage.State = customState;
                }

                return Task.CompletedTask;
            };

            options.Events.OnTokenValidated = context =>
            {
                var state = context.ProtocolMessage.State;

                if (!string.IsNullOrWhiteSpace(state))
                {
                    var identity = (ClaimsIdentity)context.Principal.Identity!;
                    identity.AddClaim(new Claim("hntas.invitationId", state));
                }

                return Task.CompletedTask;
            };
            using (var rsa = RSA.Create())
            {
                rsa.ImportFromPem(Environment.GetEnvironmentVariable("ONELOGIN_PRIVATE_KEY").AsSpan().ToString().Replace("\\n", "\n"));
                options.ClientAuthenticationCredentials = new SigningCredentials(
                    new RsaSecurityKey(rsa.ExportParameters(true)),
                    SecurityAlgorithms.RsaSha256);
            }

            options.VectorsOfTrust = [builder.Configuration.GetValue<string>("OneLogin:VectorsOfTrust")];
        });

}


// Custom authorization logic for role-based access control, policies, and handlers
builder.Services.AddApplicationAuthorization();

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});


var app = builder.Build();

// Security clickjacking fix : Add Security Headers Middleware
app.Use(async (context, next) =>
{
    context.Response.Headers["X-Frame-Options"] = "SAMEORIGIN";

    var connectSrc = string.Join(" ",
    [
        "connect-src",
        "'self'",
        "https://*.powerbi.com",
        "https://*.analysis.windows.net",
        "https://login.microsoftonline.com",
        "https://*.google-analytics.com"
    ]);

    if (builder.Environment.EnvironmentName == "Local")
    {
        connectSrc += " http://localhost:* ws://localhost:*";
    }

    var csp =
           "default-src 'self'; " +
           "font-src 'self'; " +
           "img-src 'self' data: https://*.powerbi.com https://www.googletagmanager.com https://*.google-analytics.com; " + 
           "object-src 'none'; " +
           "script-src 'self' 'unsafe-eval' 'unsafe-inline' https://www.googletagmanager.com; " + 
           "style-src 'self' 'unsafe-inline'; " +
           "frame-src 'self' https://app.powerbi.com https://*.powerbi.com; " +
           connectSrc + ";";

    context.Response.Headers["Content-Security-Policy"] = csp;

    context.Response.Headers["X-Content-Type-Options"] = "nosniff";
    context.Response.Headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
    context.Response.Headers["Permissions-Policy"] = "camera=(), microphone=(), geolocation=(), accelerometer=()";

    await next();
});

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

//Status Code Pages & HTTPS Redirection
app.UseStatusCodePagesWithReExecute("/Home/Error", "?code={0}");

app.UseHttpsRedirection();
app.UseStaticFiles();


//This is to check if the application is in maintenance mode

var maintenanceMode = Environment.GetEnvironmentVariable("MAINTENANCE_MODE");
if (!string.IsNullOrEmpty(maintenanceMode) && maintenanceMode.Equals("true", StringComparison.OrdinalIgnoreCase))
{
    app.Use(async (context, next) =>
    {
        // Allow access to the maintenance page and static assets
        var path = context.Request.Path.Value;
        if (path != null && (path.Equals("/maintenance.html", StringComparison.OrdinalIgnoreCase) ||
                             path.StartsWith("/assets") ||
                             path.StartsWith("/css") ||
                             path.StartsWith("/js")))
        {
            await next();
        }
        else
        {
            context.Response.Redirect("/maintenance.html");
        }
    });
}

try
{
    var govukAssetPath = Path.Combine(app.Environment.ContentRootPath, "node_modules/govuk-frontend/dist/govuk/assets");
    app.UseStaticFiles(new StaticFileOptions
    {
        FileProvider = new Microsoft.Extensions.FileProviders.PhysicalFileProvider(govukAssetPath),
        RequestPath = "/assets"
    });
}
catch (Exception ex)
{
    app.Logger.LogError(ex, "Unable to map govuk_frontend assets.");
}

app.UseRouting();

app.UseSession();

app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/", () => Results.Redirect("/home/start-page"));

app.MapControllerRoute(
    name: "default",
    pattern: "[controller]/[action]/{id?}",
    defaults: new { controller = "Home", action = "StartPage" });

app.Run();

[ExcludeFromCodeCoverage]
public partial class Program { }