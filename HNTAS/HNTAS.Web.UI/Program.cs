using Microsoft.AspNetCore.Authentication.Cookies;
using GovUk.OneLogin.AspNetCore;
using System.Security.Cryptography;
using Microsoft.IdentityModel.Tokens;
using HNTAS.Web.UI.Services;

var builder = WebApplication.CreateBuilder(args);

// Dynamically load appsettings.{ENVIRONMENT}.json if it exists
builder.Configuration.AddJsonFile(
    $"appsettings.{builder.Environment.EnvironmentName}.json",
    optional: true,
    reloadOnChange: true);


//Configure onelogin settings
builder.Services.AddAuthentication(defaultScheme: OneLoginDefaults.AuthenticationScheme)
    .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddOneLogin(options =>
    {
        options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;

        options.Environment = OneLoginEnvironments.Integration;
        options.ClientId = builder.Configuration["OneLogin:ClientId"];
        options.CallbackPath = "/onelogin-callback";
        options.SignedOutCallbackPath = "/onelogin-logout-callback";
        options.Scope.Add("openid");
        options.Scope.Add("email");
        options.Scope.Add("phone");
        // options.Scope.Add("profile"); // If your service needs name, birthdate, etc.

        using (var rsa = RSA.Create())
        {
            rsa.ImportFromPem(builder.Configuration["OneLogin:PrivateKey"]);
            options.ClientAuthenticationCredentials = new SigningCredentials(
                new RsaSecurityKey(rsa.ExportParameters(true)), // Fix: Ensure RsaSecurityKey is resolved
                SecurityAlgorithms.RsaSha256);
        }

        options.VectorsOfTrust = ["Cl"];
    });


builder.Services.AddControllersWithViews();
builder.Services.AddHttpClient<AddressLookupService>();

var app = builder.Build();



// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();


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

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");



app.Run();
